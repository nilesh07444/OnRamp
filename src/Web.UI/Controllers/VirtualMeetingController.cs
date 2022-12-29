using Common.Events;
using Common.Query;
using Data.EF;
using Domain.Customer;
using Domain.Models;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Command.VirtualClassRoom;
using Ramp.Contracts.CommandParameter.ExternalMeetingUsers;
using Ramp.Contracts.CommandParameter.VirtualClassroom;
using Ramp.Contracts.Events.VirtualClassroom;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Group;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Web.UI.Helpers;

namespace Web.UI.Controllers {
	public class VirtualMeetingController : RampController {
		// GET: Meeting
		public ActionResult Index() {
			var model = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery() { }).ToList();
			var virtualModels = new List<VirtualClassModel>();
			foreach (var item in model) {
				TimeSpan duration = DateTime.Now - item.EndDateTime;
				if (duration.TotalDays <= 1) {
					virtualModels.Add(item);
				}
			}
			var meetingList = GetMeetingList(virtualModels);
			ViewBag.InprogressMeetings = virtualModels.Where(c => c.StartDateTime <= DateTime.Now && c.EndDateTime >= DateTime.Now).ToList();
			PortalContext.Current.VirtualClassModel = null;
			ViewBag.CompanyId = PortalContext.Current.UserCompany.Id;

			return View(meetingList);
		}


		[HttpPost]
		/// <summary>
		/// this one is used to cancel the meeting
		/// </summary>
		/// <param name="id">this is for document id</param>
		/// <returns></returns>
		public ActionResult CancelMeeting(string id, string additionalMsg, string searchText, int pageIndex, int pageSize, int startPage, int endPage) {

			var users = ExecuteQuery<FetchByDocumentIdQuery, List<AssignedDocumentModel>>(
				new FetchByDocumentIdQuery {
					Id = id
				}).Select(c => new AssignmentViewModel {
					DocumentId = c.DocumentId,
					DocumentType = c.DocumentType,
					AdditionalMsg = c.DocumentId + "~" + additionalMsg,
					UserId = c.UserId,
					AssignedDate = c.AssignedDate
				}).ToList();

			if (users.Count() > 0) {
				var response = ExecuteCommand(new UnassignDocumentsFromUsers {
					AssignmentViewModels = users
				});
			}

			var meeting = ExecuteCommand(new DeleteVirtualClassroomCommand {
				DocumentId = id
			});
			var virtualClass = ExecuteQuery<FetchByIdQuery, VirtualClassModel>(
				new FetchByIdQuery {
					Id = id
				});

			#region send cancelled meeting email/sms notification

			foreach (var item in users) {
				var user = ExecuteQuery<FindUserByIdQuery, UserModelShort>(
				new FindUserByIdQuery {
					Id = item.UserId
				});

				var eventPublisher = new EventPublisher();
				eventPublisher.Publish(new CancelVirtualRoomEvent {
					UserViewModel = new UserViewModel() {
						StartDate = virtualClass.StartDate,
						EndDate = virtualClass.EndDate, FirstName ="Dear "+ user.UserName, EmailAddress = user.Email, MobileNumber = user.MobileNumber },
					CompanyViewModel = new CompanyViewModel(),
					AdditionNote = additionalMsg,
					IsConfirmed = false,
				
					MeetingName = virtualClass.VirtualClassRoomName,
					Subject = $"{virtualClass.VirtualClassRoomName} Virtual Classroom is cancelled."
				});
			}
			var externalParticipants = ExecuteQuery<FetchExternalMeetingUserQuery, IEnumerable<ExternalMeetingUserModel>>(new FetchExternalMeetingUserQuery() { MeetingId = id, UserId = SessionManager.GetCurrentlyLoggedInUserId().ToString() }).ToList();
			foreach (var item in externalParticipants) {

				var eventPublisher = new EventPublisher();
				eventPublisher.Publish(new CancelVirtualRoomEvent {
					UserViewModel = new UserViewModel() {
						StartDate = virtualClass.StartDate,
						EndDate = virtualClass.EndDate,
						FirstName = "Good day", EmailAddress = item.EmailAddress, MobileNumber = string.Empty },
					CompanyViewModel = new CompanyViewModel(),
					AdditionNote = additionalMsg,
					IsConfirmed = false,
					MeetingName = virtualClass.VirtualClassRoomName,
					Subject = $"{virtualClass.VirtualClassRoomName} Virtual Classroom is cancelled."
				});
			}
			#endregion

			var model = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery()).ToList();
			var meetingList = GetMeetingList(model, pageIndex, pageSize, startPage, endPage);
			return PartialView("_MeetingList", meetingList);
		}

		/// <summary>
		/// this one is used to show the cancel modal pop up
		/// </summary>
		/// <param name="id">this is for document id</param>
		/// <returns></returns>
		public ActionResult CancelMeetingModal(string id) {
			var model = ExecuteQuery<FetchByIdQuery, VirtualClassModel>(
				new FetchByIdQuery {
					Id = id
				});

			ViewData["Participants"] = ExecuteQuery<ViewParticipantQuery, IEnumerable<UserModelShort>>(new ViewParticipantQuery() { DocumentId = id }).ToList().Count();

			return PartialView("_CancelMeeting", model);
		}

		public ActionResult ViewParticipants(string id) {
			var model = ExecuteQuery<ViewParticipantQuery, IEnumerable<UserModelShort>>(new ViewParticipantQuery() { DocumentId = id }).ToList();
			var externalParticipants= ExecuteQuery<FetchExternalMeetingUserQuery, IEnumerable<ExternalMeetingUserModel>>(new FetchExternalMeetingUserQuery() { MeetingId = id , UserId=SessionManager.GetCurrentlyLoggedInUserId().ToString() }).ToList();
			ViewBag.ExternalUser = externalParticipants;
			return PartialView("_ViewParticipantList", model);
		}

		/// <summary>
		/// this is used to get users list based on selection on group for selection of user to join meetting
		/// </summary>
		/// <param name="groupIds"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult ConfirmationEmail(string userIds, string searchText, int pageIndex, int pageSize, int startPage, int endPage, string emails) {

			var emailTo = new List<string>();
			var model = PortalContext.Current.VirtualClassModel;

			var documentId = model.Id.ToString();
			ExecuteCommand(new CreateOrUpdateVirtualClassRoomCommand {
				Id = documentId,
				VirtualClassRoomName = model.VirtualClassRoomName,
				Description = model.Description,
				IsPasswordProtection = model.IsPasswordProtection,
				Password = model.Password,
				IsPublicAccess = model.IsPublicAccess,
				StartDate = DateTime.Parse(model.StartDate).ToUniversalTime(),
				EndDate = DateTime.Parse(model.EndDate).ToUniversalTime(),
				JitsiServerName = model.JitsiServerName
			});
			
			var data = userIds.Split(',').Select(c => new AssignmentViewModel {
				DocumentId = documentId,
				DocumentType = DocumentType.VirtualClassRoom,
				UserId = c,
				AssignedDate = DateTime.UtcNow,
				AdditionalMsg = ""

			}).ToList();

			var response = ExecuteCommand(new AssignDocumentsToUsers {
				AssignedBy = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
				AssignmentViewModels = data,
				AssignedDate = DateTime.UtcNow,
				MultipleAssignedDates = data.FirstOrDefault().MultipleAssignedDates,
			});

			var documentNotifications = new List<DocumentNotificationViewModel>();
			foreach (var virtualModel in data) {
				string additionalMsg = "";
				var notificationModel = new DocumentNotificationViewModel {
					AssignedDate = DateTime.Now,
					IsViewed = false,
					DocId = virtualModel.DocumentId,
					UserId = virtualModel.UserId,
					NotificationType = DocumentNotificationType.MeetingInvite.GetDescription()
				};
				if (virtualModel.AdditionalMsg != "" && virtualModel.AdditionalMsg != null) {
					var additionalMsgList = virtualModel.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
					additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == virtualModel.DocumentId).Msg;
					notificationModel.Message = additionalMsg;
				}				
				documentNotifications.Add(notificationModel);
			}
			ExecuteCommand(documentNotifications);

			foreach (var item in userIds.Split(',').ToList()) {
				var user = ExecuteQuery<FindUserByIdQuery, UserModelShort>(
				new FindUserByIdQuery {
					Id = item
				});
				model.JoinMeetingLink = model.JoinMeetingLink + "/" + model.Id;
				SendEmailConfirmaiton(model, "Dear "+user.UserName, user.Email, user.MobileNumber);
			}

			if (model.IsPublicAccess && !string.IsNullOrEmpty(emails)) {
				model.JoinMeetingLink = model.JoinPublicLink + model.Id;
				var emailList = emails.Split(',').ToList();
				foreach (var item in emailList) {
					var externalUser = ExecuteCommand(new CreateExternalMeetingUserCommand {
						UserId= SessionManager.GetCurrentlyLoggedInUserId().ToString(),
						MeetingId= model.Id.ToString(),
						EmailAddress=item
					});
					SendEmailConfirmaiton(model, "Good day", item, "");

				}
			}

			var meeting = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery() { }).ToList();
			var meetingList = GetMeetingList(meeting, pageIndex, pageSize, startPage, endPage);
			return PartialView("_MeetingList", meetingList);
		}

		[NonAction]
		public void SendEmailConfirmaiton(VirtualClassModel model, string name, string email, string mobileNumber) {

			string _smtpFrom = ConfigurationManager.AppSettings["SMTPFrom"];
			MainContext _mainContext = new MainContext();
			var loginLink = AppSettings.Urls.ResolveUrl("~", PortalContext.Current.UserCompany);
			var domain = string.IsNullOrEmpty(model.JitsiServerName) ? "meet.jit.si" : PortalContext.Current.UserCompany.JitsiServerName;

			var VirtualClassroomMeeting = new VirtualClassroomEvent {
				UserViewModel = new UserViewModel() { FirstName = name, EmailAddress = email },
				CompanyViewModel = PortalContext.Current.UserCompany,
				MeetingName = model.VirtualClassRoomName,
				StartDate = model.StartDate,
				EndDate = model.EndDate,
				Id = model.Id.ToString(),
				JoinMeetingUrl=model.JoinMeetingLink,
				Password = model.Password,
				MeetingUrl = domain,

				Subject = $"You have recived an invitation to join the {model.VirtualClassRoomName} Meeting Room"
			};
			if (PortalContext.Current != null) {
				var fileUploads = _mainContext.CustomerConfigurationSet.Where(c => c.Company.Id == PortalContext.Current.UserCompany.Id && (c.Type == CustomerConfigurationType.NotificationFooterLogo || c.Type == CustomerConfigurationType.NotificationHeaderLogo)).ToList();
				if (fileUploads.Any()) {
					VirtualClassroomMeeting.NotificationHeaderLogo = fileUploads.LastOrDefault(c => c.Type == CustomerConfigurationType.NotificationHeaderLogo).Upload?.Data;
					VirtualClassroomMeeting.NotificationFooterLogo = fileUploads.LastOrDefault(c => c.Type == CustomerConfigurationType.NotificationFooterLogo).Upload?.Data;

				}
			}
			var msg = new SendEmail();
			var body = RenderViewToString("~/Views/Notification/VirtualClassroomMeeting.cshtml", VirtualClassroomMeeting);

			msg.SendMessageWithAttachment(new List<string> { email }, null, null, null, null, null, _smtpFrom, email,
				VirtualClassroomMeeting.Subject, body, null, _smtpFrom, model, loginLink);

			var eventPublisher = new EventPublisher();
			eventPublisher.Publish(new CancelVirtualRoomEvent {
				UserViewModel = new UserViewModel() {
					StartDate = model.StartDate,
					EndDate = model.EndDate,
					FirstName = name, EmailAddress = email, MobileNumber = mobileNumber },
				CompanyViewModel = PortalContext.Current.UserCompany,
				AdditionNote = string.Empty,
				IsConfirmed = true,
				
				MeetingName = VirtualClassroomMeeting.MeetingName,
				Subject = $"{VirtualClassroomMeeting.MeetingName} Virtual Meeting Room has been created."
			});

		}


		[NonAction]
		public MeetingViewModel GetMeetingList(List<VirtualClassModel> model) {
			var meetingList = new MeetingViewModel();
			meetingList.Paginate.IsFirstPage = true;
			meetingList.Paginate.IsLastPage = false;

			var totalPages = (int)Math.Ceiling((decimal)model.Count / (decimal)meetingList.Paginate.PageSize);
			meetingList.Paginate.Page = totalPages;
			meetingList.Paginate.TotalItems = model.Count;
			meetingList.VirtualClassrooms = model.Skip((meetingList.Paginate.PageIndex - 1) * meetingList.Paginate.PageSize).Take(meetingList.Paginate.PageSize).ToList();

			meetingList.Paginate.StartPage = meetingList.Paginate.Page >= 1 ? 1 : 0;
			meetingList.Paginate.EndPage = meetingList.Paginate.Page >= 7 ? 7 : meetingList.Paginate.Page;
			if (meetingList.Paginate.PageIndex == 1) {
				meetingList.Paginate.FirstPage = 1;
				var records = meetingList.Paginate.PageIndex * meetingList.Paginate.PageSize;
				if (records <= meetingList.Paginate.TotalItems) {
					meetingList.Paginate.LastPage = records;
				} else {
					meetingList.Paginate.LastPage = meetingList.Paginate.TotalItems;
				}
			}
			meetingList.VirtualClassrooms = meetingList.VirtualClassrooms.OrderBy(c => c.StatusClass).ToList();
			return meetingList;
		}

		/// <summary>
		/// this is used for search the meeting 
		/// </summary>
		/// <param name="searchText">search meeting based on searchtext</param>
		/// <returns></returns>
		public ActionResult FilterMeeting(string filters, string searchText, int pageIndex, int pageSize, int startPage, int endPage) {
			var model = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery() { Filters=filters,SearchText = searchText }).ToList();
			var virtualModels = new List<VirtualClassModel>();
			foreach (var item in model) {
				TimeSpan duration = DateTime.Now - item.EndDateTime;
				if (duration.TotalDays <= 1) {
					virtualModels.Add(item);
				}
			}
			var meetingList = GetMeetingList(virtualModels, pageIndex, pageSize, startPage, endPage);
			return PartialView("_MeetingList", meetingList);
		}

		[NonAction]
		public MeetingViewModel GetMeetingList(List<VirtualClassModel> model, int pageIndex, int pageSize, int startPage, int endPage) {
			var meetingList = new MeetingViewModel();
			meetingList.Paginate.TotalItems = model.Count;

			var totalPages = (int)Math.Ceiling((decimal)model.Count / (decimal)meetingList.Paginate.PageSize);
			meetingList.Paginate.Page = totalPages;
			meetingList.VirtualClassrooms = model.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

			meetingList.Paginate.PageIndex = pageIndex;
			meetingList.Paginate.IsFirstPage = pageIndex == 1 ? true : false;

			meetingList.Paginate.PageSize = pageSize;
			if (pageIndex > endPage && meetingList.Paginate.Page != pageIndex) {
				meetingList.Paginate.StartPage = startPage + 1;
				meetingList.Paginate.EndPage = endPage + 1;
				meetingList.Paginate.IsLastPage = false;
			} else if (meetingList.Paginate.Page == pageIndex) {
				meetingList.Paginate.StartPage = startPage + 1;
				meetingList.Paginate.EndPage = endPage + 1;
				meetingList.Paginate.IsLastPage = true;
			} else if (pageIndex == startPage && pageSize == 1) {
				meetingList.Paginate.StartPage = startPage;
				meetingList.Paginate.EndPage = endPage;
			} else if (pageIndex < startPage) {
				meetingList.Paginate.StartPage = startPage - 1;
				meetingList.Paginate.EndPage = endPage - 1;
			} else {
				meetingList.Paginate.StartPage = startPage;
				meetingList.Paginate.EndPage = endPage;
				meetingList.Paginate.IsLastPage = false;
				meetingList.Paginate.IsFirstPage = false;
			}
			if (meetingList.Paginate.Page < 7 && meetingList.Paginate.Page > 1) {
				meetingList.Paginate.StartPage = meetingList.Paginate.Page >= 1 ? 1 : 1;
				meetingList.Paginate.EndPage = meetingList.Paginate.Page >= 7 ? 7 : meetingList.Paginate.Page;

			} else if (meetingList.Paginate.Page == 0 || meetingList.Paginate.Page == 1) {
				meetingList.Paginate.StartPage = 1;
				meetingList.Paginate.EndPage = 1;
				meetingList.Paginate.IsLastPage = true;
				meetingList.Paginate.IsFirstPage = true;
			}
			if (meetingList.Paginate.PageIndex == 1) {
				meetingList.Paginate.FirstPage = 1;
				var records = meetingList.Paginate.PageIndex * meetingList.Paginate.PageSize;
				if (records <= meetingList.Paginate.TotalItems) {
					meetingList.Paginate.LastPage = records;
				} else {
					meetingList.Paginate.LastPage = meetingList.Paginate.TotalItems;
				}
			} else {
				var records = meetingList.Paginate.PageIndex * meetingList.Paginate.PageSize;
				meetingList.Paginate.FirstPage = ((meetingList.Paginate.PageIndex - 1) * meetingList.Paginate.PageSize) + 1;
				if (records <= meetingList.Paginate.TotalItems) {
					meetingList.Paginate.LastPage = records;
				} else {
					meetingList.Paginate.LastPage = meetingList.Paginate.TotalItems;
				}
			}
			meetingList.VirtualClassrooms = meetingList.VirtualClassrooms.OrderBy(c => c.StatusClass).ToList();
			return meetingList;
		}

		/// <summary>
		/// this is one is used to show the Create Virtual Classroom page
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CreateOrUpdateVirtualClass(bool check) {
			if (check)
				PortalContext.Current.VirtualClassModel = null;


			if (PortalContext.Current.VirtualClassModel == null) return PartialView("_CreateOrUpdateVirtualClass", new VirtualClassModel());

			var model = PortalContext.Current.VirtualClassModel;

			return PartialView("_CreateOrUpdateVirtualClass", model);
		}
		/// <summary>
		/// this is used to save the Virtual Classroom
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult CreateOrUpdateVirtualClass(VirtualClassModel model) {
			model.JitsiServerName = PortalContext.Current.UserCompany.JitsiServerName;
			model.Id = Guid.NewGuid();
			PortalContext.Current.VirtualClassModel = model;

			return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet };

		}
		/// <summary>
		/// this one is used to share the meeting
		/// </summary>
		/// <param name="emails">emails with comma separation </param>
		/// <param name="link">link that need to share</param>
		/// <param name="documentId">this is the meeting Id</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult SharePublinkByEmail(string emails,string link, string documentId) {

			var model = ExecuteQuery<FetchByIdQuery, VirtualClassModel>(
				new FetchByIdQuery {
					Id = documentId
				});
			string _smtpFrom = ConfigurationManager.AppSettings["SMTPFrom"];
			
			
			model.JoinMeetingLink = link;
			var emailList = emails.Split(',').ToList();
			foreach (var item in emailList) {
				SendEmailConfirmaiton(model, "Good Day", item, "");
				var externalUser = ExecuteCommand(new CreateExternalMeetingUserCommand {
					UserId = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
					MeetingId = model.Id.ToString(),
					EmailAddress = item
				});
			}

			return new JsonResult {Data=true ,JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		private string RenderViewToString(string viewName, dynamic model) {
			this.ViewData.Model = model;
			try {
				using (StringWriter sw = new StringWriter()) {
					var engines = new ViewEngineCollection();
					ViewEngineResult viewResult = ViewEngines.Engines.FindView(this.ControllerContext, viewName, null);
					ViewContext viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, sw);
					viewResult.View.Render(viewContext, sw);
					viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);

					return sw.ToString();
				}
			}
			catch (Exception ex) {
				return ex.ToString();
			}
		}
		#region VirtualClassRoom
		/// <summary>
		/// this is used to get page to select User to join the virtual classroom
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult AssignUserToVirtualClass() {
			ViewBag.Groups =
				ExecuteQuery<GroupsWithUsersQuery, IEnumerable<GroupViewModelShort>>(new GroupsWithUsersQuery());
			return PartialView("_AssignVirtualClassRoom");
		}

		/// <summary>
		/// this is used to get page to select group list
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetGroupList(string documentId, bool check = true) {
			ViewBag.Check = check;
			ViewBag.Groups =
				ExecuteQuery<GroupsWithUsersQuery, IEnumerable<GroupViewModelShort>>(new GroupsWithUsersQuery());
			return PartialView("_GetGroupList", documentId);
		}

		[HttpGet]
		public ActionResult AddParticipantsToMeeting(string userIds, string documentId,string hostname) {
			string _smtpFrom = ConfigurationManager.AppSettings["SMTPFrom"];

			MainContext _mainContext = new MainContext();

			var emailTo = new List<string>();

			var data = userIds.Split(',').Select(c => new AssignmentViewModel {
				DocumentId = documentId,
				DocumentType = DocumentType.VirtualClassRoom,
				UserId = c,
				AssignedDate = DateTime.UtcNow,
				AdditionalMsg = ""

			}).ToList();

			var response = ExecuteCommand(new AssignDocumentsToUsers {
				AssignedBy = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
				AssignmentViewModels = data,
				AssignedDate = DateTime.UtcNow,
				MultipleAssignedDates = data.FirstOrDefault().MultipleAssignedDates,
			});

			var documentNotifications = new List<DocumentNotificationViewModel>();
			foreach (var virtualModel in data) {
				string additionalMsg = "";
				var notificationModel = new DocumentNotificationViewModel {
					AssignedDate = DateTime.Now,
					IsViewed = false,
					DocId = virtualModel.DocumentId,
					UserId = virtualModel.UserId,
					NotificationType = DocumentNotificationType.MeetingInvite.GetDescription()
				};
				if (virtualModel.AdditionalMsg != "" && virtualModel.AdditionalMsg != null) {
					var additionalMsgList = virtualModel.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
					additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == virtualModel.DocumentId).Msg;
					notificationModel.Message = additionalMsg;
				}
				documentNotifications.Add(notificationModel);
			}
			ExecuteCommand(documentNotifications);

			if (response.Validation.Any() || response.ErrorMessage != null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var model = ExecuteQuery<FetchByIdQuery, VirtualClassModel>(new FetchByIdQuery() { Id = documentId });
			model.JoinMeetingLink = hostname + "/" + documentId;
			foreach (var item in userIds.Split(',').ToList()) {
				var user = ExecuteQuery<FindUserByIdQuery, UserModelShort>(
				new FindUserByIdQuery {
					Id = item
				});

				SendEmailConfirmaiton(model, "Dear " + user.UserName, user.Email, user.MobileNumber);

			}
			var meeting = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery() { }).ToList();
			var meetingList = GetMeetingList(meeting);
			return PartialView("_MeetingList", meetingList);

		}


		[HttpGet]
		public ActionResult RemoveParticipantsFromMeeting(string userIds, string documentId) {

			var data = userIds.Split(',').Select(c => new AssignmentViewModel {
				DocumentId = documentId,
				DocumentType = DocumentType.VirtualClassRoom,
				UserId = c,
				AssignedDate = DateTime.UtcNow,
				AdditionalMsg = documentId + "~" + ""
			}).ToList();

			var response = ExecuteCommand(new UnassignDocumentsFromUsers {
				AssignmentViewModels = data,
				CompanyId = PortalContext.Current.UserCompany.Id
			});

			if (response.Validation.Any() || response.ErrorMessage != null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var documentNotifications = new List<DocumentNotificationViewModel>();
			foreach (var model in data) {
				var additionalMsg = "";
				var notificationModel = new DocumentNotificationViewModel {
					AssignedDate = DateTime.Now,
					IsViewed = false,
					DocId = model.DocumentId,
					UserId = model.UserId,
					NotificationType = DocumentNotificationType.MeetingUnassign.GetDescription()
				};
				if (model.AdditionalMsg != "" && model.AdditionalMsg != null) {
					var additionalMsgList = model.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
					additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == model.DocumentId).Msg;
					if(additionalMsg != "") {
						notificationModel.Message = additionalMsg;
					}					
				}
				documentNotifications.Add(notificationModel);
			}
			ExecuteCommand(documentNotifications);

			var virtualClass = ExecuteQuery<FetchByIdQuery, VirtualClassModel>(
				new FetchByIdQuery {
					Id = documentId
				});

			#region send cancelled meeting email/sms notification

			foreach (var item in data) {
				var user = ExecuteQuery<FindUserByIdQuery, UserModelShort>(
				new FindUserByIdQuery {
					Id = item.UserId
				});

				var eventPublisher = new EventPublisher();
				eventPublisher.Publish(new RemoveParticipantEvent {
					UserViewModel = new UserViewModel() { FirstName = user.UserName, EmailAddress = user.Email, MobileNumber = user.MobileNumber },
					MeetingName = virtualClass.VirtualClassRoomName,
					Subject = $"You have been removed from following meeting: {virtualClass.VirtualClassRoomName}."
				});

			}

			#endregion

			var meeting = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery() { }).ToList();
			var meetingList = GetMeetingList(meeting);
			return PartialView("_MeetingList", meetingList);
		}



		[HttpGet]
		public ActionResult AssignDocumentsToUsers(string users) {
			if (string.IsNullOrEmpty(users))
				return new JsonResult { Data = "", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			var isPublicAccess = PortalContext.Current.VirtualClassModel.IsPublicAccess;
			var documentId = PortalContext.Current.VirtualClassModel.Id;
			return new JsonResult { Data = new { users, isPublicAccess, documentId }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}
		/// <summary>
		/// This will used to show the Virtual class room Summary
		/// </summary>
		/// <param name="documentId">this is the virtual class room Id</param>
		/// <param name="users">this contains the users</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetVirtualClassroomSummary(string users) {

			var model = PortalContext.Current.VirtualClassModel;
			ViewData["UserIds"] = users;
			ViewData["participants"] = users.Split(',').Count();
			return PartialView("_VirtualCalssroomSummary", model);
		}

		/// <summary>
		/// this method is used to get user list for Add/Remove Participants based on our requirement raised by user.
		/// </summary>
		/// <param name="groupIds">this is for selected group</param>
		/// <param name="documentId">this is for selected documentId</param>
		/// <param name="check">check true means Add Participants otherwise Remove Participants</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GetUserToAddRemoveParticipants(string groupIds, string documentId, bool check = true) {

			ViewData["ids"] = groupIds.ToString();
			var ids = groupIds.Split(',').ToArray();
			if (check) {
				var users = ExecuteQuery<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>(
								new UsersNotAssignedDocumentQuery {
									DocumentId = documentId,
									DocumentType = DocumentType.VirtualClassRoom,
									GroupIds = ids
								}).ToList();
				return PartialView("_GetUsersToVirtualClassRoom", users);
			} else {

				var users = ExecuteQuery<UsersAssignedDocumentQuery, IEnumerable<UserModelShort>>(
									new UsersAssignedDocumentQuery {
										DocumentId = documentId,
										DocumentType = DocumentType.VirtualClassRoom,
										GroupIds = ids
									}).ToList();
				return PartialView("_GetUsersToVirtualClassRoom", users);
			}

		}

		/// <summary>
		/// this is used to get users list based on selection on group for selection of user to join meetting
		/// </summary>
		/// <param name="groupIds"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserToVirtualClass(string groupIds) {

			ViewData["ids"] = groupIds.ToString();
			var ids = groupIds.Split(',').ToArray();
			var users = ExecuteQuery<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>(
				new UsersNotAssignedDocumentQuery {
					DocumentType = DocumentType.VirtualClassRoom,
					GroupIds = ids
				}).ToList();
			return PartialView("_GetUsersToVirtualClassRoom", users);
		}
		/// <summary>
		/// this is one is used to search the user based on searchtext
		/// </summary>
		/// <param name="groupIds"></param>
		/// <param name="searchText"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GetUserList(string groupIds, string searchText, bool check = true) {

			var ids = groupIds.Split(',').ToArray();

			if (check) {
				var users = ExecuteQuery<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>(
				new UsersNotAssignedDocumentQuery {

					DocumentType = DocumentType.VirtualClassRoom,
					GroupIds = ids
				}).Where(c => c.Name.ToLower().Contains(searchText.ToLower())).ToList();
				return PartialView("_AssignUserList", users);
			} else {
				var users = ExecuteQuery<UsersAssignedDocumentQuery, IEnumerable<UserModelShort>>(
				new UsersAssignedDocumentQuery {

					DocumentType = DocumentType.VirtualClassRoom,
					GroupIds = ids
				}).Where(c => c.Name.ToLower().Contains(searchText.ToLower())).ToList();
				return PartialView("_AssignUserList", users);
			}

		}

		#endregion
	}
}