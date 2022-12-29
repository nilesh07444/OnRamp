using Common.Command;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models.Policy;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ramp.Contracts.Query.DocumentCategory;
using Data.EF.Customer;
using Ramp.Contracts.Query.RecycleBinQuery;
using Domain.Customer.Models.Document;
using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.Memo;

namespace Ramp.Services.QueryHandler {
	public class PolicyQueryHandler : IQueryHandler<FetchByIdQuery, PolicyModel>,IQueryHandler<FetchByCustomDocumentIdQuery, Policy>,
									IQueryHandler<FetchByIdQuery<Policy>, PolicyModel>,
									  IQueryHandler<PolicyListQuery, IEnumerable<PolicyListModel>>,
									  IQueryHandler<PolicyListQuery, IEnumerable<DocumentListModel>>,
									  IQueryHandler<FetchTotalDocumentsQuery<Policy>, int>,
									IQueryHandler<RecyclePolicyQuery, IEnumerable<DocumentListModel>>,
									  IQueryHandler<FetchByCategoryIdQuery, PolicyChartViewModel> {
		readonly ITransientReadRepository<Policy> _repository;
		readonly ICommandDispatcher _commandDispatcher;
		private readonly IQueryExecutor _queryExecutor;
		private readonly IRepository<DocumentUrl> _documentUrlRepository;
		private readonly IRepository<StandardUser> _standardUser;
		private readonly ITransientReadRepository<ConditionalTable> _ConditionalRepository;


		public PolicyQueryHandler(ITransientReadRepository<Policy> repository, ICommandDispatcher commandDispatcher,
			IQueryExecutor queryExecutor, IRepository<DocumentUrl> documentUrlRepository, IRepository<StandardUser> standardUser,
			ITransientReadRepository<ConditionalTable> ConditionalRepository) {
			_repository = repository;
			_commandDispatcher = commandDispatcher;
			_queryExecutor = queryExecutor;
			_documentUrlRepository = documentUrlRepository;
			_standardUser = standardUser;
			_ConditionalRepository = ConditionalRepository;
		}
		public PolicyChartViewModel ExecuteQuery(FetchByCategoryIdQuery query) {
			var id = Convert.ToString(query.Id);
			var policy = _repository.List.Where(c => c.CategoryId == id);
			var result = new PolicyChartViewModel {
				PolicyCount= policy.Count()
			};
			return result;
		}
        public Policy ExecuteQuery(FetchByCustomDocumentIdQuery query)
        {
            return _repository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
        }
        public PolicyModel ExecuteQuery(FetchByIdQuery query) {
			var e = query.Id == null ? null : _repository.List.AsQueryable().FirstOrDefault(x => x.Id == query.Id.ToString());
			if (e == null || (e != null && e.Deleted))
				return new PolicyModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
			var model = Project.Policy_PolicyModel.Compile().Invoke(e);
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
			return model;
		}
		public IEnumerable<PolicyListModel> ExecuteQuery(PolicyListQuery query) {
			var entries = Filter(query).Select(Project.Policy_PolicyListModel).ToList();
			entries.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));
			return entries;
		}
		public IEnumerable<DocumentListModel> ExecuteQuery(RecyclePolicyQuery query) {
			var entries = _repository.List.AsQueryable().Where(x => x.Deleted );
			var statusCollection = query.DocumentStatus.String_DocumentStatus();
			if (statusCollection.Any())
				entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
			var models = entries.OrderBy(x => x.Title).Select(Project.Policy_DocumentListModel).ToList();
			return models;
		}

		PolicyModel IQueryHandler<FetchByIdQuery<Policy>, PolicyModel>.ExecuteQuery(FetchByIdQuery<Policy> query)
		{
			var e = query.Id == null ? null : _repository.List.AsQueryable().FirstOrDefault(x => x.Id == query.Id.ToString());
			if (e == null || (e != null && e.Deleted))
				return new PolicyModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
			var model = Project.Policy_PolicyModel.Compile().Invoke(e);
			var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(model.Id)).ToList();
			if (conditionals.Count > 0)
			{
				model.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
				model.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
				model.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
				model.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
				model.IsConditionalLogic = true;
			}
			return model;
		}

		public int ExecuteQuery(FetchTotalDocumentsQuery<Policy> query) {
			return _repository.List.AsQueryable().Count();
		}

		IEnumerable<DocumentListModel> IQueryHandler<PolicyListQuery, IEnumerable<DocumentListModel>>.ExecuteQuery(PolicyListQuery query) {
			var entries = Filter(query).Where(x => x.IsCustomDocument == null).Select(Project.Policy_DocumentListModel).ToList();
			entries.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));

			var users = _standardUser.List.ToList();

			entries.ForEach(x =>
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

			return entries;
		}
		public IOrderedQueryable<Policy> Filter(DocumentListQueryBase query) {
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
	public static partial class Project {
		public static readonly Expression<Func<Policy, PolicyListModel>> Policy_PolicyListModel =
		   x => new PolicyListModel {
			   Approver = x.Approver,
			   ApproverId = x.ApproverId,
			   PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
			   CreatedBy = x.CreatedBy.ToString(),
			   CreatedOn = x.CreatedOn,
			   Deleted = x.Deleted,
			   Description = x.Description,
			   Id = x.Id.ToString(),
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
			   IsGlobalAccessed = x.IsGlobalAccessed,
			   Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
		   };
		public static readonly Expression<Func<Policy, DocumentListModel>> Policy_DocumentListModel =
			x => new DocumentListModel {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				CreatedBy = x.CreatedBy,
				CreatedOn = x.CreatedOn,
				Deleted = x.Deleted,
				Description = x.Description,
				DocumentType = DocumentType.Policy,
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
				TrainingLabels = string.IsNullOrEmpty(x.TrainingLabels) ? "none" : x.TrainingLabels,
				IsGlobalAccessed = x.IsGlobalAccessed,
				Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
			};
		public static readonly Expression<Func<Policy, DocumentListModel>> Policy_DocumentListModel_WithCategory =x => new DocumentListModel {
			   CreatedBy = x.CreatedBy,
			   CreatedOn = x.CreatedOn,
			   Deleted = x.Deleted,
			   Description = x.Description,
			   DocumentType = DocumentType.Policy,
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
			   Approver = x.Approver,
			   ApproverId = x.ApproverId,
			   PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
			   IsGlobalAccessed = x.IsGlobalAccessed,
			   Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
		   };
		public static readonly Expression<Func<Policy, PolicyModel>> Policy_PolicyModel =
		  x => new PolicyModel {
			  Approver = x.Approver,
			  ApproverId = x.ApproverId,
			  PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
			  CreatedBy = x.CreatedBy,
			  CreatedOn = x.CreatedOn,
			  LastEditedBy = x.LastEditedBy,
			  Deleted = x.Deleted,
			  Description = x.Description,
			  Id = x.Id.ToString(),
			  DocumentStatus = x.DocumentStatus,
			  DocumentType = DocumentType.Policy,
			  LastEditDate = x.LastEditDate,
			  Points = x.Points,
			  PreviewMode = x.PreviewMode,
			  Printable = x.Printable,
			  ReferenceId = x.ReferenceId,
			  Title = x.Title,
			  CallToAction = x.CallToAction,
			  CallToActionMessage = x.CallToActionMessage,
			  TrainingLabels = x.TrainingLabels,
			  CoverPicture = x.CoverPicture != null ? Upload_UploadResultViewModel.Invoke(x.CoverPicture) : null,
			  CoverPictureId = x.CoverPictureId,
			  Category = x.Category != null ? Category_CategoryViewModelShort.Invoke(x.Category) : null,
			  CategoryId = x.CategoryId,
			  CustomDocumentOrder=x.CustomDocumentOrder,
			  IsGlobalAccessed = x.IsGlobalAccessed,
			  ContentModels = x.ContentBoxes.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(PolicyContentBox_PolicyContentBoxModel).ToArray(),
			  Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
		  };
		public static readonly Expression<Func<PolicyContentBox, PolicyContentBoxModel>> PolicyContentBox_PolicyContentBoxModel =
		  x => new PolicyContentBoxModel {
			  Content = x.Content,
			  Deleted = x.Deleted,
			  Id = x.Id.ToString(),
			  Number = x.Number,
			  ParentId = x.PolicyId.ToString(),
			  Title = x.Title,
			  CustomDocumentOrder = x.CustomDocumentOrder,
			  Attachments = x.Uploads.AsQueryable().Where(e => !e.Deleted).OrderBy(e => e.Order).Select(Upload_UploadResultViewModel).ToArray(),
			  ContentToolsUploads = x.ContentToolsUploads.AsQueryable().Where(u => !u.Deleted).Select(Upload_UploadFromContentToolsResultModel).ToArray(),

			  //added by softude
			  IsAttached = x.IsAttached,
			  IsSignOff = x.IsSignOff,
			  NoteAllow = x.NoteAllow,
			  AttachmentRequired = x.AttachmentRequired,
			  IsConditionalLogic=x.IsConditionalLogic,
			  CheckRequired = x.CheckRequired

		  };



	}
}
