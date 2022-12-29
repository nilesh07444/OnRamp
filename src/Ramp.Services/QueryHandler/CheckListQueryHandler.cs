using com.sun.org.apache.bcel.@internal.generic;
using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static Ramp.Contracts.ViewModel.UserActivityAndPerformanceViewModel;


using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler {
	public class CheckListQueryHandler : IQueryHandler<FetchByIdQuery, CheckListModel>, IQueryHandler<FetchByCustomDocumentIdQuery, CheckList>,
										  IQueryHandler<CheckListQuery, IEnumerable<CheckListListModel>>,
										  IQueryHandler<CheckListQuery, IEnumerable<DocumentListModel>>,
										IQueryHandler<RecycleChecklistQuery, IEnumerable<DocumentListModel>>,
										  IQueryHandler<CheckListSubmissionReportQuery, IEnumerable<ChecklistInteractionModel>>,
										  IQueryHandler<FetchByCategoryIdQuery, ChecklistChartViewModel>,
										  IQueryHandler<FetchTotalDocumentsQuery<CheckList>, int>,

		IQueryHandler<AllCheckListSubmissionReportQuery, IEnumerable<CheckListModel>> {
		private readonly IRepository<CheckListChapterUserResult> _checkListChapterUserResultRepository;
		private readonly IRepository<CheckListUserResult> _checkListUserResultRepository;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly ITransientReadRepository<CheckList> _repository;
		private readonly ITransientReadRepository<Upload> _uploadRepository;
		private readonly IRepository<StandardUser> _userRepository;
		private readonly IQueryExecutor _queryExecutor;
		private readonly ITransientReadRepository<CheckListChapter> _checkListChapterRepository;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ITransientReadRepository<DocumentUsage> _documentUsageRepository;
		private readonly IRepository<DocumentUrl> _documentUrlRepository;
		private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;
		private readonly IRepository<CustomerGroup> _groupRepository;

		private readonly IRepository<StandardUser> _standardUser;

		

		public CheckListQueryHandler(
			IRepository<StandardUserGroup> standardUserGroupRepository,
			IRepository<CustomerGroup> groupRepository,
			ITransientReadRepository<CheckList> repository,
			IRepository<CheckListChapterUserResult> checkListChapterUserResultRepository,
			IRepository<CheckListUserResult> checkListUserResultRepository,
			IRepository<AssignedDocument> assignedDocumentRepository,
			ITransientReadRepository<Upload> uploadRepository,
			IRepository<StandardUser> userRepository,
			ITransientReadRepository<DocumentUsage> documentUsageRepository,
			ITransientReadRepository<CheckListChapter> checkListChapterRepository,
			ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor, IRepository<DocumentUrl> documentUrlRepository, IRepository<StandardUser> standardUser) {

			_standardUserGroupRepository = standardUserGroupRepository;
			_groupRepository = groupRepository;
			_repository = repository;
			_commandDispatcher = commandDispatcher;
			_queryExecutor = queryExecutor;
			_uploadRepository = uploadRepository;
			_checkListChapterUserResultRepository = checkListChapterUserResultRepository;
			_checkListUserResultRepository = checkListUserResultRepository;
			_assignedDocumentRepository = assignedDocumentRepository;
			_userRepository = userRepository;
			_documentUsageRepository = documentUsageRepository;
			_checkListChapterRepository = checkListChapterRepository;
			_documentUrlRepository = documentUrlRepository;

			

_standardUser = standardUser;

		}


	#region this code  added to get upload based on Id 
	public Upload ExecuteQuery(FetchUploadByIdQuery query) {
			if (query == null)
				return null;
			Upload result = new Upload();

			result = _uploadRepository.Find(query.Id);
			return result;
		}

		#endregion

		public ChecklistChartViewModel ExecuteQuery(FetchByCategoryIdQuery query) {
			var id = Convert.ToString(query.Id);
			var checkLists = _repository.List.Where(c => c.DocumentCategoryId == id);
			var result = new ChecklistChartViewModel() {
				ChecklistCount= checkLists.Count()
			};
			return result;
		}
        public CheckList ExecuteQuery(FetchByCustomDocumentIdQuery query)
        {
            return _repository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
        }
        public CheckListModel ExecuteQuery(FetchByIdQuery query)     
		{

			var manual = _repository.Find(query.Id?.ToString());
			if (manual == null || (manual != null && manual.Deleted))
				return new CheckListModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
			var model = Project.CheckList_CheckListModel.Compile().Invoke(manual);
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

		public int ExecuteQuery(FetchTotalDocumentsQuery<CheckList> query) {
			return _repository.List.AsQueryable().Count();
		}
		public IEnumerable<DocumentListModel> ExecuteQuery(RecycleChecklistQuery query) 
		{
			
			var entries = _repository.List.AsQueryable().Where(x => x.Deleted );
			var statusCollection = query.DocumentStatus.String_DocumentStatus();
			if (statusCollection.Any())
				entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
			var models = entries.OrderBy(x => x.Title).Select(Project.CheckList_DocumentListModel);
			return models;
		}
		public IEnumerable<DocumentListModel> ExecuteQuery(CheckListQuery query) 
		{
			var entries = Filter(query).Where(x => x.IsCustomDocument ==null).Select(Project.CheckList_DocumentListModel).ToList();
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

		IEnumerable<CheckListListModel> IQueryHandler<CheckListQuery, IEnumerable<CheckListListModel>>.ExecuteQuery(CheckListQuery query) {
			var entries = Filter(query).Select(Project.CheckList_CheckListModel).ToList();
			entries.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));
			return entries;
		}

		public IOrderedQueryable<CheckList> Filter(DocumentListQueryBase query) 
		{
			var entries = _repository.List.AsQueryable().Where(x => !x.Deleted);
			if (!string.IsNullOrWhiteSpace(query.CategoryId)) {
				var categoryIds =
					_queryExecutor.Execute<DocumentCategoryAndDescendantsIdQuery, IEnumerable<string>>(
						new DocumentCategoryAndDescendantsIdQuery { CategoryId = query.CategoryId }).ToList();
				entries = entries.Where(x => categoryIds.Contains(x.DocumentCategoryId));
			}
			var statusCollection = query.DocumentStatus.String_DocumentStatus();
			if (statusCollection.Any())
				entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
			return entries.OrderBy(x => x.Title);

		}

		/// <summary>
		/// this is used to get filter checklist with user and other details
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public IEnumerable<ChecklistInteractionModel> ExecuteQuery(CheckListSubmissionReportQuery query) 
		{

			var checkLists = _repository.List.AsQueryable().Where(x => !x.Deleted && query.CheckListIds.Contains(x.Id)).ToList();
			
			var assignedDocs = _assignedDocumentRepository.GetAll().Where(c => query.CheckListIds.Contains(c.DocumentId) && !c.Deleted).ToList();
			
			var docUses = _documentUsageRepository.List.AsQueryable().OrderByDescending(p=>p.ViewDate).Where(c => query.CheckListIds.Contains(c.DocumentId)).ToList();
			var model = new List<ChecklistInteractionModel>();
			var result = new List<CheckLisInteractionModel>();
			
			if (assignedDocs!=null && assignedDocs.Count > 0) {
				 result = (from a in assignedDocs
							  join u in _userRepository.GetAll() on Guid.Parse(a.UserId) equals u.Id
							  join c in _repository.List.AsQueryable() on a.DocumentId equals c.Id
							  select new CheckLisInteractionModel {
								  Id = a.DocumentId,
								  DocumentTitle = c.Title,
								  DateAssigned = a.AssignedDate,
								  UserName = u.FirstName + " " + u.LastName,
								  AssignedDocId = a.Id,
								  EmployeeCode = u.EmployeeNo,
								  IdNumber = u.IDNumber,
								  UserId = a.UserId
							  }

						 ).ToList();

				foreach (var item in result) {

					var viewDocs = docUses.Where(c => c.UserId == item.UserId).ToList();

					item.Viewed = (viewDocs != null && viewDocs.Any()) ? true : false;
					item.DateViewed = (viewDocs != null && viewDocs.Any()) ? viewDocs.FirstOrDefault().ViewDate : DateTime.UtcNow;

					var checkListUserResult1 = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == item.AssignedDocId).FirstOrDefault();

					item.DateSubmitted = (checkListUserResult1 != null) ? checkListUserResult1.SubmittedDate : DateTime.MinValue.Date;
					item.Completed = (checkListUserResult1 != null && checkListUserResult1.Status) ? "Completed" : "InComplete";
					item.Status = (checkListUserResult1 != null && checkListUserResult1.Status) ? "1" : "0";
					item.Access = (checkListUserResult1 != null && checkListUserResult1.IsGlobalAccessed) ? "Global" : "Assigned";

					var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == item.AssignedDocId).ToList();
					var checkListChapter = _checkListChapterRepository.List.AsQueryable().Where(c => c.CheckListId == item.Id && !c.Deleted).ToList();

					item.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";

				}

				if (query.Status != null && query.Status.Any()) {
					result = result.Where(c => query.Status.Contains(c.Status)).ToList();
				}
				if (query.Access != null && query.Access.Any()) {
					result = result.Where(c => query.Access.Contains(c.Access)).ToList();
				}
				if (query.ToDate.HasValue && query.FromDate.HasValue) {
					result = result.Where(c => c.DateAssigned.Date >= query.FromDate.Value.Date && c.DateAssigned.Date <= query.ToDate.Value.Date).ToList();
				} else if (query.ToDate.HasValue) {
					result = result.Where(c => c.DateAssigned.Date <= query.ToDate.Value.Date).ToList();
				} else if (query.FromDate.HasValue) {
					result = result.Where(c => c.DateAssigned.Date >= query.FromDate.Value.Date).ToList();
				}
				var check = result.GroupBy(c => c.DocumentTitle).Select(f => f.FirstOrDefault()).ToList();
				//foreach (var item in check) {
				//	var m1 = new ChecklistInteractionModel {
				//		DocumentTitle = item.DocumentTitle,
				//		Id = item.Id,
				//		Checklist = result.Where(c => c.Id == item.Id).ToList()
				//	};
				//	model.Add(m1);
				//}
			}
			
			var checklist = checkLists.FirstOrDefault();
			var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.DocumentId == checklist.Id).ToList();
			
			foreach (var item in checkListUserResult) 
			{
				var checklistresult = new CheckLisInteractionModel 
				{
					Id = item.DocumentId,
					DocumentTitle = checklist.Title,
					UserId = item.UserId
				};

				var viewDocs = docUses.Where(c => c.UserId == item.UserId).ToList();
				checklistresult.Viewed = (viewDocs != null && viewDocs.Any()) ? true : false;
				checklistresult.DateViewed = (viewDocs != null && viewDocs.Any()) ? viewDocs.FirstOrDefault().ViewDate : DateTime.UtcNow;

				checklistresult.DateSubmitted = item.SubmittedDate;
				checklistresult.Completed = item.Status ? "Completed" : "InComplete";
				checklistresult.Status = item.Status ? "1" : "0";
				checklistresult.Access = item.IsGlobalAccessed ? "Global" : "Assigned";

				var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.DocumentId == item.DocumentId && c.IsGlobalAccessed && c.UserId == item.UserId).ToList();
				var checkListChapter = _checkListChapterRepository.List.AsQueryable().Where(c => c.CheckListId == item.DocumentId && !c.Deleted).ToList();
				checklistresult.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";
				var userId = Guid.Parse(item.UserId);
				var userDetail = _userRepository.List.Where(c => c.Id == userId).FirstOrDefault();

				if (userDetail != null) {
					checklistresult.UserName = userDetail.FirstName + " " + userDetail.LastName;
					checklistresult.EmployeeCode = userDetail.EmployeeNo;
					checklistresult.IdNumber = userDetail.IDNumber;
				}
				result.Add(checklistresult);
			}
			
			if (query.Status != null && query.Status.Any()) {
				result = result.Where(c => query.Status.Contains(c.Status)).ToList();
			}
			
			if (query.Access != null && query.Access.Any()) {
				result = result.Where(c => query.Access.Contains(c.Access)).ToList();
			}
			
			if (query.ToDate.HasValue && query.FromDate.HasValue) 
			{
				result = result.Where(c => c.DateSubmitted.Value.Date >= query.FromDate.Value.Date && c.DateSubmitted.Value.Date <= query.ToDate.Value.Date).ToList();
			} 
			else if (query.ToDate.HasValue) 
			{
				result = result.Where(c => c.DateSubmitted.Value.Date <= query.ToDate.Value.Date).ToList();

			} 
			else if (query.FromDate.HasValue) 
			{
				result = result.Where(c => c.DateSubmitted.Value.Date >= query.FromDate.Value.Date).ToList();
			}
			
			foreach (var obj in checkLists) 
			{
				var list = new List<CheckLisInteractionModel>();
				foreach (var item in result) {
					if (obj.Id == item.Id) {
						var user = _userRepository.Find(Guid.Parse(item.UserId));
						item.Group = user?.Group?.Title ?? "";
						list.Add(item);
					}					
				}
				var m = new ChecklistInteractionModel {
					DocumentTitle = obj.Title,
					Id = obj.Id,
					Checklist = list
				};
				model.Add(m);
			}

			//below code added by neeraj
			foreach (var u in model)
			{
				
				
				foreach(var rr in u.Checklist)
				{

					var groups = _groupRepository.List;

					var groupList = _standardUserGroupRepository.List.Where(c => c.UserId.ToString() == rr.UserId).ToList();

					string name = null;
					//List<string> name = new List<string>();
					if (groupList.Count > 0)
					{
						foreach (var g in groupList)
						{
							foreach (var gl in groups)
							{
								if (gl.Id == g.GroupId)
								{
									//name.Add(gl.Title);
									if (name != null)
										name = name + "," + gl.Title;
									else name = name + gl.Title;
								}
							}
						}
					}
					if (name != null)
					{
						rr.Group = name;
					}

				}
			
			}

			return model;
		}

		/// <summary>
		/// this is used to get all checklist
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public IEnumerable<CheckListModel> ExecuteQuery(AllCheckListSubmissionReportQuery query) {
			var checkLists = _repository.List.AsQueryable().Where(x => !x.Deleted).ToList();
			var checkListModel = new List<CheckListModel>();
			foreach (var checkList in checkLists) {
				var model = Project.CheckList_CheckListModel.Compile().Invoke(checkList);
				checkListModel.Add(model);
				_commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model });
			}
			return checkListModel;
		}
	}

}
namespace Ramp.Services.Projection {
	public static partial class Project {

		public static readonly Expression<Func<CheckList, CheckListModel>> CheckList_CheckListModel =
			x => new CheckListModel {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				CreatedBy = x.CreatedBy,
				CheckListExpiresNumberDaysFromAssignment=x.CheckListExpiresNumberDaysFromAssignment,
				CreatedOn = x.CreatedOn,
				LastEditedBy = x.LastEditedBy,
				Deleted = x.Deleted,
				Description = x.Description,
				Id = x.Id.ToString(),
				DocumentStatus = x.DocumentStatus,
				LastEditDate = x.LastEditDate,
				Points = x.Points,
				PreviewMode = x.PreviewMode,
				Printable = x.Printable,
				IsGlobalAccessed = x.IsGlobalAccessed,
				IsChecklistTracked = x.IsChecklistTracked,
				ReferenceId = x.ReferenceId,
				Title = x.Title,
				TrainingLabels = x.TrainingLabels,
				DocumentType = DocumentType.Checklist,
				CoverPicture = x.CoverPicture != null ? Upload_UploadResultViewModel.Invoke(x.CoverPicture) : null,
				CoverPictureId = x.CoverPictureId,
				Category = x.Category != null ? Category_CategoryViewModelShort.Invoke(x.Category) : null,
				CategoryId = x.DocumentCategoryId,
				ContentModels = x.Chapters.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(CheckListChapter_CheckListChapterModel).ToArray(),
				Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
			};

		public static readonly Expression<Func<CheckListChapter, CheckListChapterModel>> CheckListChapter_CheckListChapterModel =
			x => new CheckListChapterModel {
				Content = x.Content,
				Deleted = x.Deleted,
				Id = x.Id.ToString(),
				Number = x.Number,
				AttachmentRequired = x.AttachmentRequired,
				CheckRequired = x.CheckRequired,
				IsSignOff=x.IsSignOff,
				IsConditionalLogic = x.IsConditionalLogic,  /*added by softude*/
				IsAttached = x.IsAttached,          /*added by softude*/
				IsChecked = x.IsChecked,
				ParentId = x.CheckListId.ToString(),
				Title = x.Title,
				NoteAllow=x.NoteAllow,
				CustomDocumentOrder = x.CustomDocumentOrder,
				Attachments = x.Uploads.AsQueryable().Where(u => !u.Deleted).OrderBy(e => e.Order).Select(Upload_UploadResultViewModel).ToArray(),
				ContentToolsUploads = x.ContentToolsUploads.AsQueryable().Where(u => !u.Deleted).Select(Upload_UploadFromContentToolsResultModel).ToArray()
			};


		public static readonly Expression<Func<CheckList, DocumentListModel>> CheckList_DocumentListModel =
		 x => new DocumentListModel {
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
			 DocumentType = DocumentType.Checklist,
			 LastEditDate = x.LastEditDate,
			 Points = x.Points,
			 Printable = x.Printable,
			 ReferenceId = x.ReferenceId,
			 Title = x.Title,
			 CategoryId = x.DocumentCategoryId,
			 CoverPictureId = x.CoverPictureId,
			 TrainingLabels = string.IsNullOrEmpty(x.TrainingLabels) ? "none" : x.TrainingLabels,
			 LastEditedBy = x.LastEditedBy,
			 Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort).ToList()
		 };
	}
}
