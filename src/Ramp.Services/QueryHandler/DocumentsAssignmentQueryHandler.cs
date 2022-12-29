using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.PolicyResponse;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Customer.Models.VirtualClassRooms;
using LinqKit;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.Security;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler
{
	public class
		DocumentsAssignmentQueryHandler : IQueryHandler<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>,
		IQueryHandler<DocumentsAssignedToUserQuery, IEnumerable<DocumentListModel>>,
		IQueryHandler<VirtualClassroomQuery, IEnumerable<VirtualClassModel>>,
		IQueryHandler<FetchByIdQuery, AssignedDocumentListModel>,
		IQueryHandler<FetchByIdQuery, List<AssignedDocumentModel>>,
		IQueryHandler<FetchByDocumentIdQuery, List<AssignedDocumentModel>>,
		 IQueryHandler<DocAssignedToUserQuery, AssignedDocumentModel>,
			IQueryHandler<UsersAssignedDocumentQuery, IEnumerable<UserModelShort>>,
			IQueryHandler<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>,
				IQueryHandler<ViewParticipantQuery, IEnumerable<UserModelShort>>,
			IQueryHandler<DocumentTitlesQuery, IEnumerable<DocumentTitlesAndTypeQuery>>,
			IQueryHandler<DocumentAssignedToUserQuery, bool>,
			IQueryHandler<UserCanTakeTestQuery, bool>
	{
		private readonly IRepository<CustomDocumentMessageCenter> _CustomDocumentMessageCenterRepository;

		private readonly ITransientRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly ITransientRepository<DocumentUsage> _documentUsageRepository;
		private readonly ITransientRepository<Memo> _memoRepository;
		private readonly ITransientRepository<Domain.Customer.Models.CustomDocument> _customDocumentRepository;
		private readonly ITransientRepository<Policy> _policyRepository;
		private readonly ITransientRepository<Test> _testRepository;
		private readonly ITransientRepository<TrainingManual> _trainingManualRepository;
		private readonly ITransientRepository<StandardUser> _userRepository;
		private readonly ITransientRepository<Test_Result> _testResultRepository;
		private readonly ITransientRepository<PolicyResponse> _policyResponseRepository;
		private readonly ITransientRepository<TestSession> _testSessionRepository;
		private readonly ITransientRepository<CheckList> _checkListRepository;
		private readonly ITransientRepository<VirtualClassRoom> _virtualClassRoomRepository;
		private readonly ITransientRepository<CheckListChapter> _checkListChapterRepository;
		private readonly ITransientRepository<CheckListUserResult> _checkListUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;


		public DocumentsAssignmentQueryHandler(

			IRepository<CustomDocumentMessageCenter> CustomDocumentMessageCenterRepository,

			ITransientRepository<AssignedDocument> assignedDocumentRepository,
			ITransientRepository<DocumentUsage> documentUsageRepository,
			ITransientRepository<Memo> memoRepository,
			 ITransientRepository<Domain.Customer.Models.CustomDocument> customDocumentRepository,
			ITransientRepository<Policy> policyRepository,
			ITransientRepository<Test> testRepository,
			ITransientRepository<TrainingManual> trainingManualRepository,
			ITransientRepository<StandardUser> userRepository,
			ITransientRepository<Test_Result> testResultRepository,
			 ITransientRepository<PolicyResponse> policyResponseRepository,
			 ITransientRepository<TestSession> testSessionRepository,
			 ITransientRepository<CheckList> checkListRepository,
			 ITransientRepository<CheckListUserResult> checkListUserResultRepository,
			 ITransientRepository<VirtualClassRoom> virtualClassRoomRepository,
			 ITransientRepository<CheckListChapter> checkListChapterRepository,
			ICommandDispatcher commandDispatcher)
		{

			_CustomDocumentMessageCenterRepository = CustomDocumentMessageCenterRepository;

			_assignedDocumentRepository = assignedDocumentRepository;
			_documentUsageRepository = documentUsageRepository;
			_memoRepository = memoRepository;
			_policyRepository = policyRepository;
			_testRepository = testRepository;
			_trainingManualRepository = trainingManualRepository;
			_userRepository = userRepository;
			_testResultRepository = testResultRepository;
			_policyResponseRepository = policyResponseRepository;
			_testSessionRepository = testSessionRepository;
			_checkListRepository = checkListRepository;
			_commandDispatcher = commandDispatcher;
			_virtualClassRoomRepository = virtualClassRoomRepository;
			_checkListChapterRepository = checkListChapterRepository;
			_checkListUserResultRepository = checkListUserResultRepository;
			_customDocumentRepository = customDocumentRepository;

		}


		public IEnumerable<UserModelShort> ExecuteQuery(ViewParticipantQuery query)
		{
			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
			var assignedUserIds = _assignedDocumentRepository.List.Where(x =>
					x.DocumentId == query.DocumentId && x.Deleted == false)
				.Select(x => x.UserId.ConvertToGuid()).ToList();

			var users = _userRepository.List
				.Where(u => assignedUserIds.Any(id => u.Id == id.Value) &&
							u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) &&
							 u.IsActive && !u.IsUserExpire)
				.Select(u => new UserModelShort
				{
					Id = u.Id,
					Email = u.EmailAddress,
					Name = u.FirstName + " " + u.LastName,
					GroupId = u.Group.Id
				}).OrderBy(u => u.Name).ToList();

			return users;
		}

		/// <summary>
		/// this is used to get the Virtual Classroom list towrads the standard user
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public IEnumerable<VirtualClassModel> ExecuteQuery(VirtualClassroomQuery query)
		{
			var today = DateTime.Now;
			var assignDocuments = _assignedDocumentRepository.List.Where(c => c.UserId == query.UserId && !c.Deleted && c.DocumentType == DocumentType.VirtualClassRoom).Select(x => x.DocumentId).ToArray();
			var virtualClassroom = _virtualClassRoomRepository.List.Where(c => !c.Deleted && assignDocuments.Contains(c.Id.ToString())).Select(x => new VirtualClassModel
			{
				Id = x.Id,
				VirtualClassRoomName = x.VirtualClassRoomName,
				Description = x.Description,
				IsPasswordProtection = x.IsPasswordProtection,
				Password = x.Password,
				StartDate = x.StartDate.ToLocalTime().ToString(),
				EndDate = x.EndDate.ToLocalTime().ToString(),
				CreatedOn = x.CreatedOn,
				StartDateTime = x.StartDate.ToLocalTime(),
				EndDateTime = x.EndDate.ToLocalTime(),
				CreatedBy = x.CreatedBy,
				LastEditDate = x.LastEditDate,
				LastEditedBy = x.LastEditedBy,
				Deleted = x.Deleted,
				StatusClass = x.StartDate.ToLocalTime() > today ? "bnotStarted" : x.EndDate.ToLocalTime() < today ? "ended" : "aInprogress",
				Status = x.StartDate.ToLocalTime() > today ? "PENDING" : x.EndDate.ToLocalTime() < today ? "ENDED" : "IN PROGRESS"
			}).Where(c => c.Deleted == false).ToList();

			if (!string.IsNullOrEmpty(query.SearchText))
			{
				var searchText = query.SearchText.ToLower();
				virtualClassroom = virtualClassroom.Where(c => c.VirtualClassRoomName.ToLower().Contains(searchText)).ToList();
			}

			if (!string.IsNullOrEmpty(query.Filters))
			{
				var meetingList = new List<VirtualClassModel>();
				var filters = query.Filters.ToLower().Split(',').ToList();
				foreach (var item in filters)
				{

					meetingList.AddRange(virtualClassroom.Where(c => c.StatusClass.ToLower().Contains(item)).ToList());
				}
				virtualClassroom = meetingList;
			}
			return virtualClassroom;
		}

		public List<AssignedDocumentModel> ExecuteQuery(FetchByDocumentIdQuery query)
		{

			if (query.Id != null)
			{
				var id = query.Id.ToString().ToLower();
				var assignedDocument =
				_assignedDocumentRepository.List.Where(c => !c.Deleted && c.DocumentId == id).Select(c => new AssignedDocumentModel
				{

					Id = c.Id,
					UserId = c.UserId,
					AssignedDate = c.AssignedDate,
					DocumentId = c.DocumentId,
					DocumentType = c.DocumentType,
					IsRecurring = c.IsRecurring,
					AdditionalMsg = c.AdditionalMsg

				}).ToList();
				return assignedDocument;
			}

			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
			return new List<AssignedDocumentModel>();
		}

		public List<AssignedDocumentModel> ExecuteQuery(FetchByIdQuery query)
		{

			if (query.Id == null)
			{
				var assignedDocument =
				_assignedDocumentRepository.List.Where(c => !c.Deleted).Select(c => new AssignedDocumentModel
				{

					Id = c.Id,
					UserId = c.UserId,
					AssignedDate = c.AssignedDate,
					DocumentId = c.DocumentId,
					DocumentType = c.DocumentType,
					IsRecurring = c.IsRecurring,
					AdditionalMsg = c.AdditionalMsg

				}).ToList();
				return assignedDocument;
			}

			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
			return new List<AssignedDocumentModel>();
		}

		public AssignedDocumentModel ExecuteQuery(DocAssignedToUserQuery query)
		{

			var assignedDocument =
				_assignedDocumentRepository.List.Where(x => x.UserId == query.UserId && x.Deleted == false && x.DocumentId == query.DocumentId).OrderByDescending(c => c.AssignedDate).FirstOrDefault();
			if (assignedDocument == null) return null;
			var result = new AssignedDocumentModel
			{
				Id = assignedDocument.Id,
				UserId = assignedDocument.UserId,
				AssignedDate = assignedDocument.AssignedDate,
				DocumentId = assignedDocument.DocumentId,
				DocumentType = assignedDocument.DocumentType,
				IsRecurring = assignedDocument.IsRecurring,
				AdditionalMsg = assignedDocument.AdditionalMsg
			};

			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

			return result;
		}

		//for my documents
		IEnumerable<AssignedDocumentListModel> IQueryHandler<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>.ExecuteQuery(DocumentsAssignedToUserQuery query)
		{
			if (!string.IsNullOrEmpty(query.CompanyId))
			{
				_assignedDocumentRepository.SetCustomerCompany(query.CompanyId);
				_documentUsageRepository.SetCustomerCompany(query.CompanyId);
				_memoRepository.SetCustomerCompany(query.CompanyId);
				_policyRepository.SetCustomerCompany(query.CompanyId);
				_testRepository.SetCustomerCompany(query.CompanyId);
				_trainingManualRepository.SetCustomerCompany(query.CompanyId);
				_userRepository.SetCustomerCompany(query.CompanyId);
				_testResultRepository.SetCustomerCompany(query.CompanyId);
				_checkListRepository.SetCustomerCompany(query.CompanyId);
			}

			//neeraj changes >= from <= in below assigneddate condition by neeraj alert it back before delivering
			var assignedDocuments =
				_assignedDocumentRepository.List
				.Where(x => x.UserId == query.UserId && x.Deleted == false)
				.OrderByDescending(x => x.AssignedDate)
				.ThenBy(x => x.OrderNumber).ToList();

			var results = new List<AssignedDocumentListModel>();

			foreach (var assignedDocument in assignedDocuments)
			{
				var result = new AssignedDocumentListModel
				{
					AssignedDocumentId = assignedDocument.Id,
					Id = assignedDocument.DocumentId,
					DocumentType = assignedDocument.DocumentType,
					AssignedDate = assignedDocument.AssignedDate
				};

				//removed from below query by neeraj "x.ViewDate > assignedDocument.AssignedDate &&
				var du = _documentUsageRepository.List
					.Where(x => x.DocumentId == assignedDocument.DocumentId &&
								x.UserId == query.UserId &&
								x.DocumentType == assignedDocument.DocumentType &&
								 !x.IsGlobalAccessed)
					.OrderByDescending(x => x.ViewDate).FirstOrDefault();

				//removed from below query by neeraj "x.ViewDate > assignedDocument.AssignedDate &&
				var lastViewed = _documentUsageRepository.List
					.Where(x => x.UserId == query.UserId && x.DocumentId == assignedDocument.DocumentId &&
								x.DocumentType == assignedDocument.DocumentType &&
								 !x.IsGlobalAccessed)
					.OrderByDescending(x => x.ViewDate)
					.FirstOrDefault()?.ViewDate.ToLocalTime();

				result.LastViewedDate = lastViewed;

				//below line commented by neeraj
				result.Status = lastViewed == null ? AssignedDocumentStatus.Pending : AssignedDocumentStatus.UnderReview;

				if (lastViewed.HasValue)
				{
					var test = DateTime.Compare(lastViewed.Value, result.AssignedDate);
					if (test == -1)
					{
						result.Status = AssignedDocumentStatus.Pending;
						result.LastViewedDate = null;
					}
					else if (test == 1)
					{
						//result.Status = AssignedDocumentStatus.InProgress;
					}
				}

				if (
					assignedDocument.DocumentType == DocumentType.TrainingManual || assignedDocument.DocumentType == DocumentType.Memo ||
					assignedDocument.DocumentType == DocumentType.Policy ||
					assignedDocument.DocumentType == DocumentType.Checklist || assignedDocument.DocumentType == DocumentType.custom
					)
				{

					if (lastViewed == null)
					{
						result.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed != null && du.Status == null)
					{
						result.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.InProgress)
					{
						result.Status = AssignedDocumentStatus.InProgress;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.UnderReview)
					{
						result.Status = AssignedDocumentStatus.UnderReview;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.ActionRequired)
					{
						result.Status = AssignedDocumentStatus.ActionRequired;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.Complete)
					{
						result.Status = AssignedDocumentStatus.Complete;
					}
				}
				if (assignedDocument.DocumentType == DocumentType.Policy)
				{
					var policyResponse = _policyResponseRepository.GetAll().Where(c => c.PolicyId == assignedDocument.DocumentId && c.UserId == assignedDocument.UserId && !c.IsGlobalAccessed).OrderByDescending(c => c.Created).FirstOrDefault();

					if (lastViewed == null && policyResponse == null)
					{
						result.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed != null && du.Status == null && policyResponse == null)
					{
						result.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.InProgress)
					{
						result.Status = AssignedDocumentStatus.InProgress;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.UnderReview)
					{
						result.Status = AssignedDocumentStatus.UnderReview;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.Complete)
					{
						result.Status = AssignedDocumentStatus.Complete;
					}
					else if (policyResponse != null && policyResponse.Response.HasValue)
					{
						result.Status = AssignedDocumentStatus.Complete;
					}

				}

				IDocument doc = null;

				switch (assignedDocument.DocumentType)
				{
					case DocumentType.custom:
						doc = _customDocumentRepository.Find(assignedDocument.DocumentId);
						break;
					case DocumentType.Memo:
						doc = _memoRepository.Find(assignedDocument.DocumentId);
						break;
					case DocumentType.Policy:
						doc = _policyRepository.Find(assignedDocument.DocumentId);

						var policyResponse = _policyResponseRepository.GetAll().Where(c => c.PolicyId == assignedDocument.DocumentId && c.UserId == assignedDocument.UserId && !c.IsGlobalAccessed).OrderByDescending(c => c.Created).FirstOrDefault();

						break;
					case DocumentType.Test:
						var test = _testRepository.Find(assignedDocument.DocumentId);
						if (test != null)
						{
							doc = test;
							if (test.TestExpiresNumberDaysFromAssignment.HasValue)
							{
								result.ExpiryDate =
									assignedDocument.AssignedDate.AddDays(
										test.TestExpiresNumberDaysFromAssignment.Value).ToLocalTime();
							}
							var assignedDate = assignedDocument.AssignedDate;

							//neeraj put x.Created.UtcDateTime > assignedDate after x.TestId == assignedDocument.DocumentId inside below where statement iside linq
							var testResults =
								_testResultRepository.List.Where(x => !x.Deleted && x.UserId == query.UserId && x.TestId == assignedDocument.DocumentId && !x.IsGloballyAccessed).ToList();

							if (testResults.Any())
							{
								result.CertificateUrl = testResults.OrderByDescending(x => x.Score).ToList()[0].Certificate?.Id;

								result.LastViewedDate =
									testResults.OrderByDescending(x => x.Created.DateTime).ToList()[0].Created.LocalDateTime;

								result.Status = testResults.Any(r => r.Passed)
									? AssignedDocumentStatus.Passed
									: AssignedDocumentStatus.ActionRequired;
							}
							var testSession = _testSessionRepository.GetAll().Where(c => c.UserId == assignedDocument.UserId && c.CurrentTestId == assignedDocument.DocumentId && !c.IsGlobalAccessed).FirstOrDefault();
							if (testSession != null)
							{
								result.Status = AssignedDocumentStatus.UnderReview;
								result.LastViewedDate = testSession.StartTime.Value.ToLocalTime();
							}
							else
							{
								//result.Status = result
							}

							result.PassMarks = test.PassMarks;
							result.Duration = test.Duration;
							result.AttemptsRemaining = test.MaximumAttempts - testResults.Count;
							result.EmailSummary = test.EmailSummary;
							result.HighlightAnswersOnSummary = test.HighlightAnswersOnSummary == null ? false : test.HighlightAnswersOnSummary;
						}

						break;
					case DocumentType.TrainingManual:
						doc = _trainingManualRepository.Find(assignedDocument.DocumentId);
						break;
					case DocumentType.Checklist:
						doc = _checkListRepository.Find(assignedDocument.DocumentId);
						if (!doc.Deleted)
						{

							var checkListChapters = _checkListChapterRepository.GetAll().Where(c => c.CheckListId == assignedDocument.DocumentId).ToList();
							var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignedDocument.Id && !c.IsGlobalAccessed).OrderByDescending(c => c.SubmittedDate).FirstOrDefault();
							//if (checkListUserResult != null && lastViewed != null)
							//{
							//	result.Status = AssignedDocumentStatus.Complete;
							//}
							//else if (checkListUserResult == null &&  lastViewed == null)
							//{
							//	result.Status = AssignedDocumentStatus.Pending;
							//}
							//else
							//{
							//	result.Status = AssignedDocumentStatus.UnderReview;
							//}
							//result.Status = checkListUserResult.Status ? AssignedDocumentStatus.Complete : AssignedDocumentStatus.UnderReview;

							//else
							//result.Status = AssignedDocumentStatus.Pending;
							//result.Status = (lastViewed != null) ? AssignedDocumentStatus.UnderReview : AssignedDocumentStatus.Pending;
						}
						break;
				}

				if (result.DocumentType != DocumentType.Checklist)
				{
					if (doc != null)
					{
						result.ReferenceId = doc.ReferenceId;
						result.Title = doc.Title;
						result.Description = doc.Description;
						if (doc.Category != null) { result.CategoryId = doc.Category.Id; }
						result.Printable = doc.Printable;
						var author = FindUser(doc.CreatedBy);
						result.Author = author != null ? author.Name : "Unknown";
						result.TrainingLabels = string.IsNullOrEmpty(doc.TrainingLabels) ? "none" : doc.TrainingLabels;
					}

					results.Add(result);
				}
				if (result.DocumentType == DocumentType.Checklist)
				{

					if (doc != null)
					{
						if (!doc.Deleted)
						{
							result.ReferenceId = doc.ReferenceId;
							result.Title = doc.Title;
							result.Description = doc.Description;
							result.CategoryId = doc.Category.Id;
							result.Printable = doc.Printable;
							var author = FindUser(doc.CreatedBy);
							result.Author = author != null ? author.Name : "Unknown";
							result.TrainingLabels = string.IsNullOrEmpty(doc.TrainingLabels) ? "none" : doc.TrainingLabels;
							results.Add(result);
						}

					}
				}

				result.DeclineMessages = _CustomDocumentMessageCenterRepository.List.Where(z => z.DocumentId == result.Id.ToString()).Select(c => new DeclineMessages { messages = c.Messages, CreatedOn = c.CreatedOn.ToString("MM/dd/yyyy hh: mm tt", CultureInfo.InvariantCulture), type = (byte)c.Status }).ToList();

			}

			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

			return results;
		}

		public IEnumerable<UserModelShort> ExecuteQuery(UsersAssignedDocumentQuery query)
		{
			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

			var userExist = _assignedDocumentRepository.List.Where(x => !x.Deleted && query.AllDocumentId.Contains(x.DocumentId)).ToList();

			var z = new List<AssignedDocument>();
			foreach (var usr in userExist)
			{
				var user = userExist.Where(x => x.UserId == usr.UserId).ToList();
				if (user.Count == query.AllDocumentId.Count())
				{
					z.Add(usr);
				}
			}

			var userIds = z.Select(x => x.UserId.ConvertToGuid()).Distinct().ToList();

			//var userIds = _assignedDocumentRepository.List.Where(x =>
			//		x.Deleted == false && x.DocumentId == query.DocumentId && x.DocumentType == query.DocumentType)
			//	.Select(x => x.UserId.ConvertToGuid()).ToList();

			var users = _userRepository.List
				.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) && u.IsActive && !u.IsUserExpire)
				.Select(u => new UserModelShort
				{
					Id = u.Id,
					Name = u.FirstName + " " + u.LastName,
					//GroupId = u.Group.Id
				}).OrderBy(u => u.Name).ToList();

			//var users = _userRepository.List
			//	.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) &&
			//				query.GroupIds.Any(g => g == u.Group.Id.ToString()))
			//	.Join(userIds, u => u.Id, id => id.Value, (user, id) =>
			//		new UserModelShort {
			//			Id = user.Id,
			//			Name = user.FirstName + " " + user.LastName,
			//			//GroupId = user.Group.Id
			//		}).OrderBy(u => u.Name).ToList();

			return users;
		}

		private UserModelShort FindUser(string id)
		{
			var gId = id.ConvertToGuid();
			if (gId.HasValue)
			{
				var user = _userRepository.Find(gId);
				if (user != null)
					return Ramp.Services.Projection.Project.StandardUserToUserModelShort_Firstname_LastNames.Invoke(user);
			}
			return null;
		}

		public IEnumerable<UserModelShort> ExecuteQuery(UsersNotAssignedDocumentQuery query)
		{
			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
			//var assignedUserIds = _assignedDocumentRepository.List.Where(x =>
			//		x.DocumentId == query.DocumentId && x.DocumentType == query.DocumentType && x.Deleted == false)
			//	.Select(x => x.UserId.ConvertToGuid()).ToList();

			//var users = _userRepository.List
			//	.Where(u => !assignedUserIds.Any(id => u.Id == id.Value) &&
			//				u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) &&
			//				query.GroupIds.Any(g => g == u.Group.Id.ToString()) && u.IsActive && !u.IsUserExpire)
			//	.Select(u => new UserModelShort {
			//		Id = u.Id,
			//		Name = u.FirstName + " " + u.LastName,
			//		GroupId = u.Group.Id
			//	}).OrderBy(u => u.Name).ToList();


			//var users = _userRepository.List
			//	.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) &&
			//				query.GroupIds.Any(g => g == u.Group.Id.ToString()) && u.IsActive && !u.IsUserExpire)
			//	.Select(u => new UserModelShort {
			//		Id = u.Id,
			//		Name = u.FirstName + " " + u.LastName,
			//		GroupId = u.Group.Id
			//	}).OrderBy(u => u.Name).ToList();

			var users = _userRepository.List
				.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) && u.IsActive && !u.IsUserExpire)
				.Select(u => new UserModelShort
				{
					Id = u.Id,
					Name = u.FirstName + " " + u.LastName,
					//GroupId = u.Group.Id
				}).OrderBy(u => u.Name).ToList();

			return users;
		}

		public IEnumerable<DocumentTitlesAndTypeQuery> ExecuteQuery(DocumentTitlesQuery query)
		{
			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
			var titles = new List<DocumentTitlesAndTypeQuery>();
			foreach (var id in query.Identifiers)
			{
				var title = "";
				var additionalMsg = "";
				var type = new DocumentType();
				switch (id.DocumentType)
				{
					case DocumentType.TrainingManual:
						title = _trainingManualRepository.Find(id.DocumentId).Title;
						type = DocumentType.TrainingManual;
						additionalMsg = id.AdditionalMsg;

						break;
					case DocumentType.Test:
						title = _testRepository.Find(id.DocumentId).Title;
						type = DocumentType.Test;
						additionalMsg = id.AdditionalMsg;
						break;
					case DocumentType.Policy:
						title = _policyRepository.Find(id.DocumentId).Title;
						additionalMsg = id.AdditionalMsg;
						type = DocumentType.Policy;
						break;
					case DocumentType.Memo:
						title = _memoRepository.Find(id.DocumentId).Title;
						type = DocumentType.Memo;
						additionalMsg = id.AdditionalMsg;
						break;
					case DocumentType.Checklist:
						title = _checkListRepository.Find(id.DocumentId).Title;
						type = DocumentType.Checklist;
						additionalMsg = id.AdditionalMsg;

						break;
					case DocumentType.custom:
						title = _customDocumentRepository.Find(id.DocumentId).Title;
						type = DocumentType.custom;
						additionalMsg = id.AdditionalMsg;

						break;
				}
				titles.Add(new DocumentTitlesAndTypeQuery() { DocumentId = id.DocumentId, DocumentTitle = title, DocumentType = type, AdditionalMsg = additionalMsg });
			}
			titles.OrderBy(c => c.DocumentTitle);
			return titles;
		}

		public bool ExecuteQuery(DocumentAssignedToUserQuery query)
		{
			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
			return _assignedDocumentRepository.List.FirstOrDefault(x =>
				x.Deleted == false && x.DocumentId == query.DocumentId && x.DocumentType == query.DocumentType &&
							x.UserId == query.UserId) != null;
		}

		public bool ExecuteQuery(UserCanTakeTestQuery query)
		{
			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
			if (query.IsGlobalAccessed)
			{

				var results = _testResultRepository.List.Where(x => !x.Deleted && x.IsGloballyAccessed == query.IsGlobalAccessed &&
					x.UserId == query.UserId.ToString() && x.TestId == query.TestId).ToList();
				var test = _testRepository.Find(query.TestId);

				return results.Count() < test.MaximumAttempts;

			}
			else
			{
				var lastAssignment = _assignedDocumentRepository.List
								.Where(a => !a.Deleted && a.DocumentType == DocumentType.Test && a.UserId == query.UserId.ToString() &&
											a.DocumentId == query.TestId).OrderByDescending(a => a.AssignedDate).FirstOrDefault();
				var results = _testResultRepository.List.Where(x => !x.Deleted && x.IsGloballyAccessed == query.IsGlobalAccessed &&
					x.UserId == query.UserId.ToString() && x.TestId == query.TestId && x.Created.UtcDateTime > lastAssignment.AssignedDate).ToList();
				var test = _testRepository.Find(query.TestId);

				return results.Count() < test.MaximumAttempts;
			}
		}

		AssignedDocumentListModel IQueryHandler<FetchByIdQuery, AssignedDocumentListModel>.ExecuteQuery(FetchByIdQuery query)
		{

			var documents =
				_assignedDocumentRepository.List.Where(c => c.DocumentId == query.Id.ToString()).ToList();
			IDocument doc = null;

			if (documents.Count > 0)
			{
				var assignedDocument = documents[0];
				var result = new AssignedDocumentListModel
				{
					AssignedDocumentId = assignedDocument.Id,
					Id = assignedDocument.DocumentId,
					DocumentType = assignedDocument.DocumentType,
					AssignedDate = assignedDocument.AssignedDate
				};

				doc = _checkListRepository.Find(assignedDocument.DocumentId);
				if (!doc.Deleted)
				{

					var checkListChapters = _checkListChapterRepository.GetAll().Where(c => c.CheckListId == assignedDocument.DocumentId).ToList();
					var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignedDocument.Id).FirstOrDefault();
					if (checkListUserResult != null)
						result.Status = checkListUserResult.Status ? AssignedDocumentStatus.Complete : AssignedDocumentStatus.UnderReview;
					else
						result.Status = AssignedDocumentStatus.Pending;
				}
				return result;
			}
			return null;


		}

		//for global documents
		public IEnumerable<DocumentListModel> ExecuteQuery(DocumentsAssignedToUserQuery query)
		{

			var Checklists = _checkListRepository.List.Where(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
			{
				Title = c.Title,
				Description = c.Description,
				DocumentType = DocumentType.Checklist,
				CoverPictureId = c.CoverPictureId,
				Id = c.Id,
				CreatedBy = c.CreatedBy,
				LastEditedBy = c.LastEditedBy,
				IsGlobalAccessed = c.IsGlobalAccessed,

				TrainingLabels = string.IsNullOrEmpty(c.TrainingLabels) ? "none" : c.TrainingLabels,
				CategoryId = c.DocumentCategoryId
			}).ToList();
			var Policies = _policyRepository.List.Where(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
			{
				Title = c.Title,
				Description = c.Description,
				DocumentType = DocumentType.Policy,
				CoverPictureId = c.CoverPictureId,
				Id = c.Id,
				CreatedBy = c.CreatedBy,
				LastEditedBy = c.LastEditedBy,
				IsGlobalAccessed = c.IsGlobalAccessed,
				TrainingLabels = string.IsNullOrEmpty(c.TrainingLabels) ? "none" : c.TrainingLabels,
				CategoryId = c.CategoryId
			}).ToList();
			var Test = _testRepository.List.Where(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
			{
				Title = c.Title,
				Description = c.Description,
				DocumentType = DocumentType.Test,
				CoverPictureId = c.CoverPictureId,
				Id = c.Id,
				CreatedBy = c.CreatedBy,
				LastEditedBy = c.LastEditedBy,
				IsGlobalAccessed = c.IsGlobalAccessed,
				TrainingLabels = string.IsNullOrEmpty(c.TrainingLabels) ? "none" : c.TrainingLabels,
				CategoryId = c.CategoryId,
				PassMarks = c.PassMarks,
				Duration = c.Duration
			}).ToList();
			var Memo = _memoRepository.List.Where(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
			{
				Title = c.Title,
				Description = c.Description,
				DocumentType = DocumentType.Memo,
				CoverPictureId = c.CoverPictureId,
				Id = c.Id,
				CreatedBy = c.CreatedBy,
				LastEditedBy = c.LastEditedBy,
				IsGlobalAccessed = c.IsGlobalAccessed,
				TrainingLabels = string.IsNullOrEmpty(c.TrainingLabels) ? "none" : c.TrainingLabels,
				CategoryId = c.CategoryId
			}).ToList();
			var TrainingManual = _trainingManualRepository.List.Where(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
			{
				Title = c.Title,
				Description = c.Description,
				DocumentType = DocumentType.TrainingManual,
				CoverPictureId = c.CoverPictureId,
				Id = c.Id,
				CreatedBy = c.CreatedBy,
				LastEditedBy = c.LastEditedBy,
				IsGlobalAccessed = c.IsGlobalAccessed,
				TrainingLabels = string.IsNullOrEmpty(c.TrainingLabels) ? "none" : c.TrainingLabels,
				CategoryId = c.DocumentCategoryId
			}).ToList();

			var Documents = new List<DocumentListModel>();
			Documents.AddRange(Checklists);
			Documents.AddRange(Policies);
			Documents.AddRange(Test);
			Documents.AddRange(Memo);
			Documents.AddRange(TrainingManual);
			foreach (var document in Documents)
			{
				document.DeclineMessages = _CustomDocumentMessageCenterRepository.List.Where(z => z.DocumentId == document.Id.ToString()).Select(c => new DeclineMessages { messages = c.Messages }).ToList();

				#region new code experimantal global doc status

				var du = _documentUsageRepository.List
					.Where(x => x.UserId.ToString() == query.UserId.ToString() && x.DocumentId.ToString() == document.Id.ToString() &&
								x.DocumentType == document.DocumentType &&
								 x.IsGlobalAccessed)
					.OrderByDescending(x => x.ViewDate)
					.FirstOrDefault();

				//removed from below query by neeraj "x.ViewDate > assignedDocument.AssignedDate &&
				var lastViewed = _documentUsageRepository.List
					.Where(x => x.UserId == query.UserId && x.DocumentId == document.Id &&
								x.DocumentType == document.DocumentType &&
								 x.IsGlobalAccessed)
					.OrderByDescending(x => x.ViewDate)
					.FirstOrDefault()?.ViewDate.ToLocalTime();

				document.LastViewedDate = lastViewed;

				//below line commented by neeraj
				document.Status = lastViewed == null ? AssignedDocumentStatus.Pending : AssignedDocumentStatus.UnderReview;

				if (document.DocumentType == DocumentType.TrainingManual || document.DocumentType == DocumentType.Memo ||
					document.DocumentType == DocumentType.Policy || document.DocumentType == DocumentType.Checklist)
				{

					if (lastViewed == null)
					{
						document.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed != null && du.Status == null)
					{
						document.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.UnderReview)
					{
						document.Status = AssignedDocumentStatus.UnderReview;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.Complete)
					{
						document.Status = AssignedDocumentStatus.Complete;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.InProgress)
					{
						document.Status = AssignedDocumentStatus.InProgress;
					}
				}
				if (document.DocumentType == DocumentType.Policy)
				{
					var policyResponse = _policyResponseRepository.GetAll().Where(c => c.PolicyId == document.Id && c.UserId == query.UserId && !c.IsGlobalAccessed).OrderByDescending(c => c.Created).FirstOrDefault();

					if (lastViewed == null && policyResponse == null)
					{
						document.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed != null && du.Status == null && policyResponse == null)
					{
						document.Status = AssignedDocumentStatus.Pending;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.UnderReview)
					{
						document.Status = AssignedDocumentStatus.UnderReview;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.Complete)
					{
						document.Status = AssignedDocumentStatus.Complete;
					}
					else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.InProgress)
					{
						document.Status = AssignedDocumentStatus.InProgress;
					}
					else if (policyResponse != null && policyResponse.Response.HasValue)
					{
						document.Status = AssignedDocumentStatus.Complete;

					}

				}
				#endregion


				//var lastViewed = _documentUsageRepository.List
				//	.Where(x => x.UserId.ToString() == query.UserId && x.DocumentId == document.Id &&
				//				x.DocumentType == document.DocumentType && x.IsGlobalAccessed)
				//	.OrderByDescending(x => x.ViewDate)
				//	.FirstOrDefault()?.ViewDate.ToLocalTime();

				//document.LastViewedDate = lastViewed;

				//document.Status = lastViewed == null ? AssignedDocumentStatus.Pending : AssignedDocumentStatus.InProgress;

				var authors = FindUser(document.CreatedBy);
				document.Author = authors != null ? authors.Name : "Unknown";

				IDocument doc = null;
				switch (document.DocumentType)
				{
					case DocumentType.TrainingManual:
						break;
					case DocumentType.Test:
						var test = _testRepository.Find(document.Id);
						if (test != null)
						{

							doc = test;
							var testResults = _testResultRepository.List.Where(x => !x.Deleted && x.UserId == query.UserId && x.TestId == document.Id && x.IsGloballyAccessed).ToList();

							if (testResults.Any())
							{
								//result.CertificateUrl = testResults.OrderByDescending(x => x.Score).ToList()[0].Certificate?.Id;
								document.LastViewedDate =
									testResults.OrderByDescending(x => x.Created.DateTime).ToList()[0].Created.LocalDateTime;

								document.Status = testResults.Any(r => r.Passed)
									? AssignedDocumentStatus.Passed
									: AssignedDocumentStatus.ActionRequired;
							}
							var testSession = _testSessionRepository.GetAll().Where(c => c.UserId == query.UserId && c.CurrentTestId == document.Id && c.IsGlobalAccessed).FirstOrDefault();
							if (testSession != null)
							{
								document.Status = AssignedDocumentStatus.UnderReview;
								document.LastViewedDate = testSession.StartTime.Value.ToLocalTime();
							}
							document.EmailSummary = test.EmailSummary;
							document.HighlightAnswersOnSummary = test.HighlightAnswersOnSummary;
							if (testResults.Count > 0)
							{
								document.CertificateUrl = testResults.OrderByDescending(x => x.Score).ToList()[0].Certificate?.Id;
							}
						}
						break;
					case DocumentType.Policy:
						doc = _policyRepository.Find(document.Id);

						var policyResponse = _policyResponseRepository.GetAll().Where(c => c.PolicyId == document.Id && c.UserId == query.UserId && c.IsGlobalAccessed).OrderByDescending(c => c.Created).FirstOrDefault();

						if (policyResponse != null && policyResponse.Response.HasValue)
						{
							//"neeraj"
							document.Status = policyResponse.Created != null ? AssignedDocumentStatus.Complete : AssignedDocumentStatus.Pending;

							document.LastViewedDate = policyResponse.Created.ToLocalTime();
						}
						else
						{
							//result.Status = (lastViewed != null && policyResponse.Response!=null && policyResponse.Response == null) ? AssignedDocumentStatus.UnderReview : AssignedDocumentStatus.Pending;
							document.Status = (lastViewed != null) ? AssignedDocumentStatus.UnderReview : AssignedDocumentStatus.Pending;
						}
						break;
					case DocumentType.Memo:
						break;
					case DocumentType.Checklist:

						doc = _checkListRepository.Find(document.Id);

						if (!doc.Deleted)
						{

							var checkListChapters = _checkListChapterRepository.GetAll().Where(c => c.CheckListId == document.Id).ToList();
							var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == null && c.UserId == query.UserId && c.IsGlobalAccessed && c.DocumentId == document.Id).OrderByDescending(c => c.SubmittedDate).FirstOrDefault();
							if (checkListUserResult != null)
								document.Status = checkListUserResult.Status ? AssignedDocumentStatus.Complete : AssignedDocumentStatus.UnderReview;
							else
								//result.Status = AssignedDocumentStatus.Pending;
								document.Status = (lastViewed != null) ? AssignedDocumentStatus.UnderReview : AssignedDocumentStatus.Pending;
						}
						break;
					default:
						break;
				}
			}
			return Documents;
		}
	}
}
