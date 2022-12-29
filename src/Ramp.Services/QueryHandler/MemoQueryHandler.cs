using Common.Command;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models.Memo;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ramp.Contracts.Query.DocumentCategory;
using Data.EF.Customer;
using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models;
using Domain.Customer.Models.TrainingManual;

namespace Ramp.Services.QueryHandler {
	
	public class MemoQueryHandler : IQueryHandler<FetchByIdQuery, MemoModel>,
									IQueryHandler<MemoListQuery, IEnumerable<MemoListModel>>,
									IQueryHandler<MemoListQuery, IEnumerable<DocumentListModel>>,
									IQueryHandler<RecycleMemoQuery, IEnumerable<DocumentListModel>>,
									IQueryHandler<FetchByCategoryIdQuery, MemoChartViewModel>,
									IQueryHandler<FetchTotalDocumentsQuery<Memo>, int> ,IQueryHandler<FetchByCustomDocumentIdQuery, Memo>{
		readonly ITransientReadRepository<Memo> _repository;
		readonly ICommandDispatcher _commandDispatcher;
		private readonly IQueryExecutor _queryExecutor;
		private readonly IRepository<DocumentUrl> _documentUrlRepository;
		private readonly ITransientRepository<DocumentUsage> _documentUsageRepository;

		private readonly IRepository<StandardUser> _standardUser;

		public MemoQueryHandler(
			ITransientRepository<DocumentUsage> documentUsageRepository,
			ITransientReadRepository<Memo> repository, ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor, 
			IRepository<DocumentUrl> documentUrlRepository,
			IRepository<StandardUser> standardUser
			) {
			_documentUsageRepository = documentUsageRepository;
			_repository = repository;
			_commandDispatcher = commandDispatcher;
			_queryExecutor = queryExecutor;
			_documentUrlRepository = documentUrlRepository;
			_standardUser = standardUser;
		}

		public MemoChartViewModel ExecuteQuery(FetchByCategoryIdQuery query) 
		{
			var id = Convert.ToString(query.Id);
			var memo = _repository.List.Where(c => c.CategoryId == id);
			var result = new MemoChartViewModel() {
				MemoCount = memo.Count()
			};
			return result;
		}
        public Memo ExecuteQuery(FetchByCustomDocumentIdQuery query)
        {
            return _repository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
        }
        public MemoModel ExecuteQuery(FetchByIdQuery query) 
		{
			var manual = _repository.Find(query.Id?.ToString());
			if (manual == null || (manual != null && manual.Deleted))
				return new MemoModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
			var model = Project.Memo_MemoModel.Invoke(manual);
			var docUrls = _documentUrlRepository.GetAll().Where(x => x.DocumentId == model.Id).ToList();
			//if (docUrls.Count > 0) {
				foreach (var contentModel in model.ContentModels) {
					contentModel.DocLinks = new List<DocumentUrlViewModel>();
					foreach (var url in docUrls) {
						if (contentModel.Id == url.ChapterId) {
							var urlViewModel = new DocumentUrlViewModel() {
								DocumentId = url.DocumentId,
								Id = url.Id,
								Url = url.Url,
								ChapterId = url.ChapterId,
								Name = url.Name
							};
							contentModel.DocLinks.Add(urlViewModel);
						}
					}
					contentModel.DocLinkAndAttachmentCount = contentModel.DocLinks.Count + contentModel.Attachments.Count();
				}
			//}
			_commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model });


			var status = _documentUsageRepository.List
					.Where(x => x.DocumentId.ToString() == query.Id.ToString()).FirstOrDefault(); 

			//below code added by neeraj
			#region
			if (status != null)
			{
			
				if (status.ViewDate == null || status.ViewDate != null && status.Status == null)
				{
					model.Status = AssignedDocumentStatus.Pending;
				}
				else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Pending)
				{
					model.Status = AssignedDocumentStatus.Pending;
				}
				else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.UnderReview)
				{
					model.Status = AssignedDocumentStatus.UnderReview;
				}
				else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Complete)
				{
					model.Status = AssignedDocumentStatus.Complete;
				}
				else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.InProgress)
				{
					model.Status = AssignedDocumentStatus.InProgress;
					;
				}
			}
			#endregion

			return model;
		}
		
		public IEnumerable<MemoListModel> ExecuteQuery(MemoListQuery query) 
		{
			
			var models = Filter(query).Select(Project.Memo_MemoListModel).ToList();
			models.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));
			return models;
		}
		
		public IEnumerable<DocumentListModel> ExecuteQuery(RecycleMemoQuery query) 
		{
			var entries = _repository.List.AsQueryable().Where(x => x.Deleted);
			var statusCollection = query.DocumentStatus.String_DocumentStatus();
			if (statusCollection.Any())
				entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
			var models = entries.OrderBy(x => x.Title).Select(Project.Memo_DocumentListModel).ToList();
			return models;
		}

		public int ExecuteQuery(FetchTotalDocumentsQuery<Memo> query) {
			return _repository.List.AsQueryable().Count();
		}

		IEnumerable<DocumentListModel> IQueryHandler<MemoListQuery, IEnumerable<DocumentListModel>>.ExecuteQuery(MemoListQuery query) 
		{
			var models = Filter(query).Where(x => x.IsCustomDocument == null).Select(Project.Memo_DocumentListModel).ToList();
			models.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));

			var users = _standardUser.List.ToList();

			models.ForEach(x =>
			{
				if (x.Approver != null)
				{
					var userIds = x.Approver.Split(',').ToList();
					string names = null;
					foreach (var i in userIds)
					{
						foreach (var u in users)
						{
							if (u.Id.ToString() == i)
							{
								if (names == null)
								{
									names = u.FirstName + " " + u.LastName;
								}
								else if (names != null)
								{
									names = names + ", " + u.FirstName + " " + u.LastName;
								}
							}
						}
					}

					x.ApproversName = names;
				}
				else x.ApproversName = "none";
			});

			return models;
		}

		public IOrderedQueryable<Memo> Filter(DocumentListQueryBase query) 
		{

			var entries = _repository.List.AsQueryable().Where(x => !x.Deleted);

			if (!string.IsNullOrWhiteSpace(query.CategoryId)) {
				var categoryIds =
					_queryExecutor.Execute<DocumentCategoryAndDescendantsIdQuery, IEnumerable<string>>(
						new DocumentCategoryAndDescendantsIdQuery { CategoryId = query.CategoryId }).ToList();
				entries = entries.Where(x => categoryIds.Contains(x.CategoryId));
			}
			var statusCollection = query.DocumentStatus.String_DocumentStatus();
			if (statusCollection.Any())
				entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
			return entries.OrderBy(x => x.Title);
		}
	}
}
namespace Ramp.Services.Projection {
	public static partial class Project 
	{
		public static readonly Expression<Func<Memo, MemoListModel>> Memo_MemoListModel =
			x => new MemoListModel {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				CreatedBy = x.CreatedBy.ToString(),
				CreatedOn = x.CreatedOn,
				Deleted = x.Deleted,
				Description = x.Description,
				Id = x.Id.ToString(),
				IsGlobalAccessed = x.IsGlobalAccessed,
				DocumentStatus = x.DocumentStatus,
				LastEditDate = x.LastEditDate,
				Points = x.Points,
				Printable = x.Printable,
				ReferenceId = x.ReferenceId,
				Title = x.Title,
				CategoryId = x.CategoryId,
				CoverPictureId = x.CoverPictureId,
				LastEditedBy = x.LastEditedBy,
				TrainingLabels = x.TrainingLabels,
				Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
				

			};
		public static readonly Expression<Func<Memo, DocumentListModel>> Memo_DocumentListModel =
			x => new DocumentListModel {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				CreatedBy = x.CreatedBy,
				CreatedOn = x.CreatedOn,
				Deleted = x.Deleted,
				IsGlobalAccessed = x.IsGlobalAccessed,
				Description = x.Description,
				DocumentType = DocumentType.Memo,
				DocumentStatus = x.DocumentStatus,
				LastEditDate = x.LastEditDate,
				Points = x.Points,
				Printable = x.Printable,
				ReferenceId = x.ReferenceId,
				Title = x.Title,
				Id = x.Id.ToString(),
				CategoryId = x.CategoryId,
				CoverPictureId = x.CoverPictureId,
				LastEditedBy = x.LastEditedBy,
				TrainingLabels = string.IsNullOrEmpty(x.TrainingLabels)?"none": x.TrainingLabels,
				Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
			};
		public static readonly Expression<Func<Memo, DocumentListModel>> Memo_DocumentListModel_WithCategory =
			x => new DocumentListModel {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				CreatedBy = x.CreatedBy,
				IsGlobalAccessed = x.IsGlobalAccessed,
				CreatedOn = x.CreatedOn,
				Deleted = x.Deleted,
				Description = x.Description,
				DocumentType = DocumentType.Memo,
				DocumentStatus = x.DocumentStatus,
				LastEditDate = x.LastEditDate,
				Points = x.Points,
				Printable = x.Printable,
				ReferenceId = x.ReferenceId,
				Title = x.Title,
				Id = x.Id.ToString(),
				CategoryId = x.CategoryId,
				Category = x.Category == null ? null : Category_CategoryViewModelShort.Compile().Invoke(x.Category),
				CoverPictureId = x.CoverPictureId,
				LastEditedBy = x.LastEditedBy,
				TrainingLabels = x.TrainingLabels,
				Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
			};
		public static readonly Expression<Func<Memo, MemoModel>> Memo_MemoModel =
		  x => new MemoModel {
			  Approver = x.Approver,
			  ApproverId = x.ApproverId,
			  PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
			  CreatedBy = x.CreatedBy,
			  IsGlobalAccessed = x.IsGlobalAccessed,
			  CreatedOn = x.CreatedOn,
			  LastEditedBy = x.LastEditedBy,
			  Deleted = x.Deleted,
			  Description = x.Description,
			  Id = x.Id.ToString(),
			  DocumentStatus = x.DocumentStatus,
			  DocumentType = DocumentType.Memo,
			  LastEditDate = x.LastEditDate,
			  Points = x.Points,
			  PreviewMode = x.PreviewMode,
			  Printable = x.Printable,
			  ReferenceId = x.ReferenceId,
			  Title = x.Title,
			  TrainingLabels = x.TrainingLabels,
			  CoverPicture = x.CoverPicture != null ? Upload_UploadResultViewModel.Invoke(x.CoverPicture) : null,
			  CoverPictureId = x.CoverPictureId,
			  Category = x.Category != null ? Category_CategoryViewModelShort.Invoke(x.Category) : null,
			  CategoryId = x.CategoryId,			  
			  ContentModels = x.ContentBoxes.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(MemoContentBox_MemoContentBoxModel).ToArray(),
			  Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
		  };
		public static readonly Expression<Func<MemoContentBox, MemoContentBoxModel>> MemoContentBox_MemoContentBoxModel =
		  x => new MemoContentBoxModel {
			  Content = x.Content,
			  Deleted = x.Deleted,
			  Id = x.Id.ToString(),
			  Number = x.Number,
			  ParentId = x.MemoId.ToString(),
			  Title = x.Title,
			  IsAttached= x.IsAttached,
			  IsSignOff = x.IsSignOff,
			  NoteAllow = x.NoteAllow,
			  AttachmentRequired= x.AttachmentRequired,
			  CustomDocumentOrder = x.CustomDocumentOrder,
			  Attachments = x.Uploads.AsQueryable().Where(u => !u.Deleted).OrderBy(e => e.Order).Select(Upload_UploadResultViewModel).ToArray(),
			  ContentToolsUploads = x.ContentToolsUploads.AsQueryable().Where(u => !u.Deleted).Select(Upload_UploadFromContentToolsResultModel).ToArray()
		  };


	}
}
