using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Common;
using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Data.EF;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Models;
using Newtonsoft.Json;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Events.Document;
using Ramp.Contracts.Events.VirtualClassroom;
using Ramp.Contracts.Query.Bundle;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.Query.Group;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.Query.User;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Web.UI.Helpers;

namespace Web.UI.Controllers {
	public class SendController : RampController {

		private readonly ICommandDispatcher _dispacther;
		private readonly IQueryExecutor _executor;
		private CustomerContext _context;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardUserGroupRepository;

		public SendController(IRepository<AssignedDocument> assignedDocumentRepository,
			IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardUserGroupRepository) {
			_dispacther = new CommandDispatcher();
			_executor = new QueryExecutor();
			_assignedDocumentRepository = assignedDocumentRepository;
			_standardUserGroupRepository = standardUserGroupRepository;
		}

		// GET: Send
		public ActionResult Index() {
			var custom = ExecuteQuery<GetCustomConfigurationQueryParameter, CustomConfigurationViewModel>(new GetCustomConfigurationQueryParameter());
			var model = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(
				new DocumentListQuery {
					DocumentFilters = new string[]
					{
						"Status:Published",
						"IsGlobalAccessed:false"
					},
					EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument
				});
			//ViewBag.Groups =
			//	ExecuteQuery<GroupsWithUsersQuery, IEnumerable<GroupViewModelShort>>(new GroupsWithUsersQuery());
			var n = model.Count();
			var groups = ExecuteQuery<CustomerGroup, IEnumerable<JSTreeViewModel>>(new CustomerGroup()).ToList();
			//groups.ForEach(x => {
			//	if (x.parent == "#")
			//		x.parent = $"{Guid.Empty}";
			//});
			//(groups as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Groups" });
			var ng = new List<GroupViewModelShort>();
			foreach(var g in groups) {
				ng.Add(new GroupViewModelShort {
					Id = Guid.Parse(g.id),
					Name = g.text,
					Selected = false
				});
			}

			ViewBag.Groups = ng;

			return View(model);
		}

		public JsonResult GetJSTreeDocuments() {
			var root = new List<dynamic>
			{
				new
				{
					text = "Category",
					id = Guid.Empty.ToString(),
					parent = "#",
					isParentNode = true,
					state = new
					{
						opened = true
					},					
				}
			};
			var categories =
				ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery())
					.Select(c => {
						if (c.parent == "#" || c.parent==null)
						{
							c.parent = Guid.Empty.ToString();
						}
						c.isParentNode = true;
						return c;
					});
			var model = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(
				new DocumentListQuery {
					DocumentFilters = new string[]
					{
						"Status:Published",
						"IsGlobalAccessed:false"
					},
					EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument
				}).Where(z=>z.DocumentStatus== DocumentStatus.Published).Select(d => new JSTreeViewModel {
					text = d.Title,
					id = d.Id,
					parent =d.Category!=null? d.Category.Id: Guid.Empty.ToString(),
					type = d.DocumentType.ToString()
				}).ToArray();
			IEnumerable<dynamic> result = categories.Concat(model).Concat(root);
			return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[AllowAnonymous]
		public ActionResult VirtualMeetingReminderEmailScheduler() {
			string _smtpFrom = ConfigurationManager.AppSettings["SMTPFrom"];
			MainContext _mainContext = new MainContext();
			var emailTo = new List<string>();

			var todayDateTime = DateTime.Now.AddMinutes(15);
			var meeting = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery() { }).Take(1).ToList();//ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery() { }).Where(c => c.StartDateTime == todayDateTime).ToList();
			if (meeting.Any()) {
				foreach (var document in meeting) {

					var assined = ExecuteQuery<FetchByDocumentIdQuery, List<AssignedDocumentModel>>(new FetchByDocumentIdQuery() { Id = document.Id }).ToList();

					foreach (var user in assined) {

						var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = user.Id.ToString() });
						var domain = string.IsNullOrEmpty(PortalContext.Current.UserCompany.JitsiServerName) ? "meet.jit.si" : PortalContext.Current.UserCompany.JitsiServerName;
						var VirtualClassroomMeeting = new VirtualMeetingReminderEvent {
							UserViewModel = new UserViewModel() { FirstName = userDetail.Name, EmailAddress = userDetail.Email },
							MeetingName = document.VirtualClassRoomName,
							StartDate = document.StartDate,
							EndDate = document.EndDate,
							MeetingUrl = "",
							CompanyViewModel = new CompanyViewModel(),
							Subject = $"Please note that you have a Meeting that will being in 15 minutes time"
						};

						var eventPublisher = new EventPublisher();
						eventPublisher.Publish(VirtualClassroomMeeting);

					}
				}
			}


			return null;
		}



		[HttpPost]
		public ActionResult UsersNotAssignedDocument(string[] groupIds, string[] allDocumentId)
		{
			if (!groupIds.Any())
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var users = ExecuteQuery<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>(
				new UsersNotAssignedDocumentQuery
				{
					GroupIds = groupIds
				});

			var assignedDocuments = _assignedDocumentRepository.List.Where(x => !x.Deleted && allDocumentId.Contains(x.DocumentId)).ToList();

			//assignedDocuments = await Task.FromResult(assignedDocuments.ToList());

			var assignedUsers = new List<AssignedDocument>();

			var result = new List<DocumentListModel>();

			var memoList = ExecuteQuery<MemoListQuery, IEnumerable<DocumentListModel>>(new MemoListQuery()).Where(c => !c.Deleted).ToList();

			//memoList = await Task.FromResult(memoList.ToList());

			var manualList = ExecuteQuery<TrainingManualListQuery, IEnumerable<DocumentListModel>>(new TrainingManualListQuery()).Where(c => !c.Deleted).ToList();

			//manualList = await Task.FromResult(manualList.ToList());

			var testList = ExecuteQuery<TestListQuery, IEnumerable<DocumentListModel>>(new TestListQuery()).Where(c => !c.Deleted).ToList();

			//testList = await Task.FromResult(testList.ToList());

			var policyList = ExecuteQuery<PolicyListQuery, IEnumerable<DocumentListModel>>(new PolicyListQuery()).Where(c => !c.Deleted).ToList();

			//policyList = await Task.FromResult(policyList.ToList());

			var checklistList = ExecuteQuery<CheckListQuery, IEnumerable<DocumentListModel>>(new CheckListQuery()).Where(c => !c.Deleted).ToList();

			//checklistList = await Task.FromResult(checklistList.ToList());

			result.AddRange(memoList);
			result.AddRange(manualList);
			result.AddRange(testList);
			result.AddRange(policyList);
			result.AddRange(checklistList);
			foreach (var user in users)
			{
				var ADList = assignedDocuments.Where(z => z.UserId == user.Id.ToString()).ToList();
				foreach (var assign in ADList)
				{
					var assignedDocumentUsers = new AssignedDocumentInfo();
					//if (user.Id == assign.UserId.ConvertToGuid())
					//{
					assignedDocumentUsers.DocumentId = assign.DocumentId;
					assignedDocumentUsers.IsDocumentAssign = true;
					user.AssignedDocumentUsers.Add(assignedDocumentUsers);
					// }
				}
			}

			foreach (var user in users)
			{
				if (user.AssignedDocumentUsers.Count > 0)
				{
					foreach (var assigned in user.AssignedDocumentUsers.ToList())
					{
						foreach (var a in allDocumentId)
						{
							var assignedDocumentUsers = new AssignedDocumentInfo();
							if (assigned.DocumentId != a)
							{
								assignedDocumentUsers.DocumentId = a;
								assignedDocumentUsers.IsDocumentAssign = false;
								user.AssignedDocumentUsers.Add(assignedDocumentUsers);
							}
						}
					}
				}
			}

			foreach (var user in users)
			{
				foreach (var docs in user.AssignedDocumentUsers)
				{
					foreach (var doc in result.Where(z => z.Id == docs.DocumentId).ToList())
					{
						//if (docs.DocumentId == doc.Id)
						//{
						docs.Title = doc.Title;
						//}
					}
				}
			}

			foreach (var usr in assignedDocuments)
			{
				var user = assignedDocuments.Where(x => x.UserId == usr.UserId).ToList();
				if (user.Count == allDocumentId.Count())
				{
					assignedUsers.Add(usr);
				}
			}

			var userIds = assignedUsers.Select(x => x.UserId.ConvertToGuid()).Distinct().ToList();
			//userIds = await Task.FromResult(userIds.ToList());

			var matchUsers = users.Where(y => userIds.Any(x => x == y.Id)).ToList();
			//matchUsers = await Task.FromResult(matchUsers.ToList());

			users = users.Except(matchUsers);

			var groupList = _standardUserGroupRepository.List.ToList();

			var n = new List<Domain.Customer.Models.Groups.StandardUserGroup>();
			var grpList = groupList.Where(z => groupIds.Contains(z.GroupId.ToString())).ToList();
			foreach (var gl in grpList)
			{
				//foreach (var g in groupIds)
				//{
				// if (gl.GroupId.ToString() == g)
				//{
				n.Add(gl);
				//}
				// }
			}
			var newResult = new List<UserModelShort>();
			foreach (var u in users)
			{
				foreach (var g in n)
				{
					if (g.UserId == u.Id)
					{
						newResult.Add(u);
					}
				}
			}
			//var newUser = new List<UserModelShort>();

			//foreach(var x in users) {

			//	var r = ExecuteQuery<StandardUserGroupByUserIdQuery, StandardUserGroupViewModel>(new StandardUserGroupByUserIdQuery() { UserId =x.Id.ToString() });


			//}

			//var newResult = new List<UserModelShort>();

			//foreach (var n in newUser) {
			// foreach(var g in n.GroupId) {
			//		if (groupIds.Contains(g.GroupId.ToString())) {
			//			newResult.Add(n);
			//		}
			//	}
			//}

			return new JsonResult { Data = newResult.Distinct(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

        #region ["Optimized a code by Pawan on 17 may 2022"]
        //public ActionResult UsersNotAssignedDocument(string[] groupIds, string[] allDocumentId) {
        //	if (!groupIds.Any()) {
        //		return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //	}

        //	var users = ExecuteQuery<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>(
        //		new UsersNotAssignedDocumentQuery {
        //			GroupIds = groupIds
        //		});

        //	var assignedDocuments =  _assignedDocumentRepository.List.Where(x => !x.Deleted && allDocumentId.Contains(x.DocumentId)).ToList();

        //	//assignedDocuments = await Task.FromResult(assignedDocuments.ToList());

        //	var assignedUsers = new List<AssignedDocument>();

        //	var result = new List<DocumentListModel>();

        //	var memoList = ExecuteQuery<MemoListQuery, IEnumerable<DocumentListModel>>(new MemoListQuery()).Where(c => !c.Deleted).ToList();

        //	//memoList = await Task.FromResult(memoList.ToList());

        //	var manualList = ExecuteQuery<TrainingManualListQuery, IEnumerable<DocumentListModel>>(new TrainingManualListQuery()).Where(c => !c.Deleted).ToList();

        //	//manualList = await Task.FromResult(manualList.ToList());

        //	var testList = ExecuteQuery<TestListQuery, IEnumerable<DocumentListModel>>(new TestListQuery()).Where(c => !c.Deleted).ToList();

        //	//testList = await Task.FromResult(testList.ToList());

        //	var policyList = ExecuteQuery<PolicyListQuery, IEnumerable<DocumentListModel>>(new PolicyListQuery()).Where(c => !c.Deleted).ToList();

        //	//policyList = await Task.FromResult(policyList.ToList());

        //	var checklistList = ExecuteQuery<CheckListQuery, IEnumerable<DocumentListModel>>(new CheckListQuery()).Where(c => !c.Deleted).ToList();

        //	//checklistList = await Task.FromResult(checklistList.ToList());

        //	result.AddRange(memoList);
        //	result.AddRange(manualList);
        //	result.AddRange(testList);
        //	result.AddRange(policyList);
        //	result.AddRange(checklistList);
        //	foreach (var user in users) {
        //		foreach (var assign in assignedDocuments) {
        //			var assignedDocumentUsers = new AssignedDocumentInfo();
        //			if (user.Id == assign.UserId.ConvertToGuid()) {
        //				assignedDocumentUsers.DocumentId = assign.DocumentId;
        //				assignedDocumentUsers.IsDocumentAssign = true;
        //				user.AssignedDocumentUsers.Add(assignedDocumentUsers);
        //			}
        //		}
        //	}

        //	foreach (var user in users) {
        //		if (user.AssignedDocumentUsers.Count > 0) {
        //			foreach (var assigned in user.AssignedDocumentUsers.ToList()) {
        //				foreach (var a in allDocumentId) {
        //					var assignedDocumentUsers = new AssignedDocumentInfo();
        //					if (assigned.DocumentId != a) {
        //						assignedDocumentUsers.DocumentId = a;
        //						assignedDocumentUsers.IsDocumentAssign = false;
        //						user.AssignedDocumentUsers.Add(assignedDocumentUsers);
        //					}			
        //				}
        //			}
        //		}
        //	}

        //	foreach (var user in users) {
        //		foreach (var docs in user.AssignedDocumentUsers) {
        //			foreach (var doc in result) {
        //				if (docs.DocumentId == doc.Id) {
        //					docs.Title = doc.Title;
        //				}
        //			}
        //		}
        //	}

        //	foreach (var usr in assignedDocuments) {
        //		var user = assignedDocuments.Where(x => x.UserId == usr.UserId).ToList();
        //		if (user.Count == allDocumentId.Count()) {
        //			assignedUsers.Add(usr);
        //		}
        //	}

        //	var userIds =  assignedUsers.Select(x => x.UserId.ConvertToGuid()).Distinct().ToList();
        //	//userIds = await Task.FromResult(userIds.ToList());

        //	var matchUsers =   users.Where(y => userIds.Any(x => x == y.Id)).ToList();
        //	//matchUsers = await Task.FromResult(matchUsers.ToList());

        //	users =  users.Except(matchUsers);

        //	var groupList = _standardUserGroupRepository.List.ToList();

        //	var n = new List<Domain.Customer.Models.Groups.StandardUserGroup>();

        //	foreach(var gl in groupList) {
        //		foreach(var g in groupIds) {
        //			if(gl.GroupId.ToString() == g) {
        //				n.Add(gl);
        //			}
        //		}
        //	}
        //	 var newResult = new List<UserModelShort>();
        //	foreach (var u in users) {
        //		foreach(var g in n) {
        //			if(g.UserId == u.Id) {
        //				newResult.Add(u);
        //			}
        //		}
        //	}
        //	//var newUser = new List<UserModelShort>();

        //	//foreach(var x in users) {

        //	//	var r = ExecuteQuery<StandardUserGroupByUserIdQuery, StandardUserGroupViewModel>(new StandardUserGroupByUserIdQuery() { UserId =x.Id.ToString() });


        //	//}

        //	//var newResult = new List<UserModelShort>();

        //	//foreach (var n in newUser) {
        //	// foreach(var g in n.GroupId) {
        //	//		if (groupIds.Contains(g.GroupId.ToString())) {
        //	//			newResult.Add(n);
        //	//		}
        //	//	}
        //	//}

        //	return  new JsonResult { Data = newResult.Distinct(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}

        [HttpPost]
        public ActionResult UsersAssignedDocument(string[] groupIds, string[] allDocumentId)
        {
            if (!groupIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var users = ExecuteQuery<UsersAssignedDocumentQuery, IEnumerable<UserModelShort>>(
                new UsersAssignedDocumentQuery
                {
                    GroupIds = groupIds,
                    AllDocumentId = allDocumentId
                });

            var groupList = _standardUserGroupRepository.List.ToList();

            var n = new List<Domain.Customer.Models.Groups.StandardUserGroup>();

            foreach (var gl in groupList)
            {
                foreach (var g in groupIds)
                {
                    if (gl.GroupId.ToString() == g)
                    {
                        n.Add(gl);
                    }
                }
            }
            var newResult = new List<UserModelShort>();
            foreach (var u in users)
            {
                foreach (var g in n)
                {
                    if (g.UserId == u.Id)
                    {
                        newResult.Add(u);
                    }
                }
            }
            return new JsonResult { Data = newResult, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        #endregion

        [HttpPost]
		public HttpStatusCodeResult AssignDocumentsToUsers(IEnumerable<AssignmentViewModel> data) {

			var assignedDocuments = _assignedDocumentRepository.List.Where(x => !x.Deleted).ToList();

			var existDocument = data.Where(y => assignedDocuments.Any(z => z.DocumentId == y.DocumentId && z.UserId == y.UserId)).ToList();

			var result = data.Except(existDocument);

			if (result.ToList().Count > 0) {

				var response = ExecuteCommand(new AssignDocumentsToUsers {
					AssignedBy = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
					AssignmentViewModels = result,
					CompanyId = PortalContext.Current.UserCompany.Id,
					AssignedDate = data.FirstOrDefault().AssignedDate == null ? DateTime.UtcNow : data.FirstOrDefault().AssignedDate.Value.ToLocalTime(),
					MultipleAssignedDates = data.FirstOrDefault().MultipleAssignedDates,
				});

				var documentNotifications = new List<DocumentNotificationViewModel>();

				foreach (var model in data) {
					var additionalMsg = "";
					var notificationModel = new DocumentNotificationViewModel {
						AssignedDate = DateTime.Now,
						IsViewed = false,
						DocId = model.DocumentId,
						UserId = model.UserId,
						NotificationType = DocumentNotificationType.Assign.GetDescription(),
					};

					if (model.AdditionalMsg != "" && model.AdditionalMsg != null) {
						var additionalMsgList = model.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
						additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == model.DocumentId).Msg;
						if (additionalMsg != "") {
							notificationModel.Message = additionalMsg;
						}
					}
					documentNotifications.Add(notificationModel);
				}
				ExecuteCommand(documentNotifications);
				if (response.Validation.Any() || response.ErrorMessage != null) {
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
				}

				return new HttpStatusCodeResult(HttpStatusCode.OK);
			} else {
				return new HttpStatusCodeResult(HttpStatusCode.Conflict);
			}
		}
	

		[HttpPost]
		public HttpStatusCodeResult UnassignDocumentsFromUsers(IEnumerable<AssignmentViewModel> data) {
			var response = ExecuteCommand(new UnassignDocumentsFromUsers {
				AssignmentViewModels = data,
				CompanyId=PortalContext.Current.UserCompany.Id
			});

			var documentNotifications = new List<DocumentNotificationViewModel>();
			foreach (var model in data) {
				var additionalMsg = "";
				var notificationModel = new DocumentNotificationViewModel {
					AssignedDate = DateTime.Now,
					IsViewed = false,
					DocId = model.DocumentId,
					UserId = model.UserId,
					NotificationType = DocumentNotificationType.Unassign.GetDescription(),
				};
				if (model.AdditionalMsg != "" && model.AdditionalMsg != null) {
					var additionalMsgList = model.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
					additionalMsg = additionalMsgList.Where(f => f.Id == model.DocumentId && !string.IsNullOrEmpty(f.Msg)).FirstOrDefault().Msg;
					if(additionalMsg != "") {
						notificationModel.Message = additionalMsg;
					}
				}				
				documentNotifications.Add(notificationModel);
			}
			ExecuteCommand(documentNotifications);

			if (response.Validation.Any() || response.ErrorMessage != null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		[HttpPost]
		public HttpStatusCodeResult ReassignDocumentsToUsers(IEnumerable<AssignmentViewModel> data) {
			//var responseUnassign = ExecuteCommand(new UnassignDocumentsFromUsers {
			//	AssignmentViewModels = data,
			//	CompanyId = PortalContext.Current.UserCompany.Id
			//});
			
			var responseAssign = ExecuteCommand(new AssignDocumentsToUsers {
				AssignedBy = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
				AssignmentViewModels = data,
				CompanyId = PortalContext.Current.UserCompany.Id,
				IsReassigned = true,
				AssignedDate = data.FirstOrDefault().AssignedDate == null ? DateTime.UtcNow : data.FirstOrDefault().AssignedDate.Value
			});
			if (responseAssign.Validation.Any() || responseAssign.ErrorMessage != null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		[AllowAnonymous]
		public ActionResult DocumentAssignmentEmailScheduler() {

			//var companies = new MainContext().Set<Company>().AsQueryable().Where(x => x.CompanyType == Domain.Enums.CompanyType.CustomerCompany);
			//companies.ToList().ForEach(delegate (Company company) {
			//	string companyConnStringOld = ConfigurationManager.ConnectionStrings["CustomerContext"].ConnectionString;
			//	string conString = companyConnStringOld.Replace("DBNAME", "" + company.CompanyName.Replace(" ", string.Empty));
			//	_context = new CustomerContext(conString);
			//var documents = _assignedDocumentRepository.Where(d=>!d.Deleted).ToList();
			//});

			DateTime dt = DateTime.UtcNow.Date ;
		
		
			var documents = _assignedDocumentRepository.List.Where(x => !x.Deleted).ToList();

			if (documents.Any())
			{
				var ds = documents.Where(c => c.AssignedDate.Date == DateTime.UtcNow.Date).ToList();
				if (ds != null && ds.Count > 0)
				{
					foreach (var item in ds)
					{
						var userDocument = _executor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery
						{
							Id = item.DocumentId,
							DocumentType = item.DocumentType,
							AdditionalMsg = item.AdditionalMsg
						});

						var user = _executor.Execute<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery
						{
							Id = item.UserId
						});

						var documentTitles = _executor.Execute<DocumentTitlesQuery, IEnumerable<DocumentTitlesAndTypeQuery>>(
					   new DocumentTitlesQuery
					   {
						   Identifiers = documents.Select(x =>
							   new DocumentIdentifier { DocumentId = item.DocumentId, DocumentType = item.DocumentType, AdditionalMsg = item.AdditionalMsg }).GroupBy(c => c.DocumentId).Select(f => f.FirstOrDefault())
					   }).ToList();

						if (item.AssignedDate.Date == dt)
						{

							if (item.IsRecurring)
							{
								List<AssignmentViewModel> assignmentviewModels = new List<AssignmentViewModel> {
										new AssignmentViewModel() {
											DocumentId = item.DocumentId,
											UserId = item.UserId,
											DocumentType = item.DocumentType,
											AssignedDate = item.AssignedDate
										}
									};

								var responseUnassign = ExecuteCommand(new UnassignDocumentsFromUsers
								{
									AssignmentViewModels = assignmentviewModels
								});

								var responseAssign = ExecuteCommand(new AssignDocumentsToUsers
								{
									AssignedBy = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
									AssignmentViewModels = assignmentviewModels,
									CompanyId = PortalContext.Current.UserCompany.Id,
									AssignedDate = assignmentviewModels.FirstOrDefault().AssignedDate == null ? DateTime.UtcNow : assignmentviewModels.FirstOrDefault().AssignedDate.Value
								});
							}
							var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = user.Id.ToString() });
							var eventPublisher = new EventPublisher();
							string fullName = userDetail.UserName;
							var names = fullName.Split(' ');
							string firstName = names[0];
							eventPublisher.Publish(new DocumentsAssignedEvent
							{
								UserViewModel = new UserViewModel() { FirstName = firstName, EmailAddress = userDetail.Email },
								CompanyViewModel = PortalContext.Current.UserCompany,
								DocumentTitles = documentTitles,
								IsAssigned = true,
								Subject = $"{documentTitles.Count()} Document{(documentTitles.Count() > 1 ? "s" : "")} Assigned"
							});
						}
					}
				}
			}
		
			return null;
		}

		[HttpPost]
		public async Task<ActionResult> GetDocumentType(string[] documentId) {
			var result = new List<DocumentListModel>();
			var finalResult = new List<DocumentListModel>();
			if (documentId != null) {
				try
				{
					var memoList = ExecuteQuery<MemoListQuery, IEnumerable<DocumentListModel>>(new MemoListQuery()).Where(c => !c.Deleted);
					memoList = await Task.FromResult(memoList.ToList());

					var manualList = ExecuteQuery<TrainingManualListQuery, IEnumerable<DocumentListModel>>(new TrainingManualListQuery()).Where(c => !c.Deleted);
					manualList = await Task.FromResult(manualList.ToList());

					var testList = ExecuteQuery<TestListQuery, IEnumerable<DocumentListModel>>(new TestListQuery()).Where(c => !c.Deleted);
					testList = await Task.FromResult(testList.ToList());

					var policyList = ExecuteQuery<PolicyListQuery, IEnumerable<DocumentListModel>>(new PolicyListQuery()).Where(c => !c.Deleted);
					policyList = await Task.FromResult(policyList.ToList());

					var checklistList = ExecuteQuery<CheckListQuery, IEnumerable<DocumentListModel>>(new CheckListQuery()).Where(c => !c.Deleted);
					checklistList = await Task.FromResult(checklistList.ToList());

					result.AddRange(memoList);
					result.AddRange(manualList);
					result.AddRange(testList);
					result.AddRange(policyList);
					result.AddRange(checklistList);

					foreach (var res in result)
					{
						foreach (var id in documentId)
						{
							if (id == res.Id)
							{
								switch (res.DocumentType)
								{
									case DocumentType.TrainingManual:
										res.Type = 1;
										break;
									case DocumentType.Test:
										res.Type = 2;
										break;
									case DocumentType.Policy:
										res.Type = 3;
										break;
									case DocumentType.Memo:
										res.Type = 4;
										break;
									case DocumentType.Checklist:
										res.Type = 6;
										break;
								}
								finalResult.Add(res);
							}
						}
					}

					//finalResult = result.Where(x => x.Id == documentId).ToList();
					//finalResult = await Task.FromResult(result.Where(x => x.Id == documentId).ToList());
				}
				catch(Exception ex)
				{

				}
			}

			return new JsonResult { Data = finalResult, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

		}

		[HttpPost]
		public ActionResult FilterUsers(string documentId, int documentType, string[] groupIds, string searchText, string[] allDocumentId, string actionType) {

			var users = new List<UserModelShort>().AsEnumerable();
			if(actionType == "Assign") {
				users = ExecuteQuery<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>(
				new UsersNotAssignedDocumentQuery {
					DocumentId = documentId,
					DocumentType = (DocumentType)documentType,
					GroupIds = groupIds
				});
			} else {
				 users = ExecuteQuery<UsersAssignedDocumentQuery, IEnumerable<UserModelShort>>(
				new UsersAssignedDocumentQuery {
					DocumentId = documentId,
					DocumentType = (DocumentType)documentType,
					GroupIds = groupIds,
					AllDocumentId = allDocumentId
				});
			}
					 
			var groupList = _standardUserGroupRepository.List.ToList();

			var n = new List<Domain.Customer.Models.Groups.StandardUserGroup>();

			foreach (var gl in groupList)
			{
				foreach (var g in groupIds)
				{
					if (gl.GroupId.ToString() == g)
					{
						n.Add(gl);
					}
				}
			}

			var newResult = new List<UserModelShort>();
			foreach (var u in users)
			{
				foreach (var g in n)
				{
					if (g.UserId == u.Id)
					{
						newResult.Add(u);
					}
				}
			}

			if (!string.IsNullOrEmpty(searchText))
			{
				var search = searchText.ToLower();
				newResult = newResult.Where(c => c.Name.ToLower().Contains(search)).ToList();
			}
			return new JsonResult { Data = newResult, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

 
		}

	}
}