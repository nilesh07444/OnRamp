using Common.Events;
using Domain.Models;
using Ramp.Contracts.Events;
using Ramp.Contracts.Events.Account;
using Ramp.Contracts.Events.ActivityManagement;
using Ramp.Contracts.Events.CustomerManagement;
using Ramp.Contracts.Events.Feedback;
using Ramp.Contracts.Events.GuideAndTestManagement;
using Ramp.Contracts.Events.GuideManagement;
using Ramp.Contracts.Events.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Implementations;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Domain.Customer;
using Ramp.Contracts.Events.Document;
using Web.UI.Code.ActionFilters;
using Data.EF;
using Ramp.Contracts.Events.VirtualClassroom;
using Ramp.Contracts.Events.DocumentWorkflow;

namespace Web.UI.Controllers {
	[PortalContextActionFilter]
	public class NotificationController : Controller,
		IEventHandler<CustomerSelfSignedUpEvent>,
		IEventHandler<CustomerSelfProvisionedEvent>,
		IEventHandler<UserCreatedEvent>,
		IEventHandler<TrainingTestAssignedEvent>,
		IEventHandler<TrainingGuideAssignedEvent>,
		IEventHandler<CustomerUserSelfSignedUpApprovedEvent>,
		IEventHandler<TrainingGuideAndTestAssignedEvent>,
		IEventHandler<LostPasswordEvent>,
		IEventHandler<FeedbackCreatedEvent>,
		IEventHandler<UserFeedbackCreatedEvent>,
		IEventHandler<SendTestExpiryNotificationEvent>,
		IEventHandler<TrainingTestCompletedEvent>,
		IEventHandler<DocumentsAssignedEvent>,
		IEventHandler<DocumentWorkflowEvent>,
		IEventHandler<VirtualClassroomEvent>,
		IEventHandler<VirtualMeetingReminderEvent>,
		IEventHandler<CancelVirtualRoomEvent>,
		IEventHandler<RemoveParticipantEvent>,
		IEventHandler<TestCompletedEvent> {
		public const string CustomerSelfSignedUpCustomerCopyView = "CustomerSelfSignedUpCustomerCopy",
			CustomerSelfSignedUpCustomerAdminCopyView = "CustomerSelfSignedUpCustomerAdminCopy",
			CustomerSelfProvisionedCustomerCopyView = "CustomerSelfProvisionedCustomerCopy",
			UserCreatedEventUserCopyView = "UserCreatedEventUserCopy",
			TrainingTestAssignedUserCopyView = "TrainingTestAssignedUserCopy",
			TrainingGuideAssignedUserCopyView = "TrainingGuideAssignedUserCopy",
			DocumentWorkflowEventView = "DocumentWorkflowEventView",
			DocumentsAssignedUserCopyView = "DocumentsAssignedUserCopyView",
			CustomerUserSelfSignedUpApprovedCustomerCustomerCopyView = "CustomerUserSelfSignedUpApprovedCustomerCopy",
			TrainingGuideAndTestAssignmentCustomerCopyView = "TrainingGuideAndTestAssignmentCustomerCopy",
			LostPasswordUserCopyView = "LostPasswordUserCopy",
			Feedback = "NotifyFeedback",
			UserFeedback = "NotifyUserFeedback",
			SendTestExpiryNotificationUserCopyView = "SendTestExpiryNotificationUserCopy",
			TrainingTestCompletedEventUserCopyView = "TrainingTestCompletedEventUserCopy",
				VirtualClassroomMeeting = "VirtualClassroom",
			CancelVirtualMeetingEvent = "CancelVirtualMeetingEvent",
			VirtualMeetingReminderEvent = "VirtualMeetingReminder",
			RemoveParticipantEvent = "RemoveParticipantMeeting",
		TestCompletedEventUserCopyView = "TestCompletedEventUserCopy";

		private readonly MainContext _mainContext;

		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
		public NotificationController() {
			_mainContext = new MainContext();
			if (PortalContext.Current != null) {
				var fileUploads = _mainContext.CustomerConfigurationSet.Where(c => c.Company.Id == PortalContext.Current.UserCompany.Id && (c.Type == CustomerConfigurationType.NotificationFooterLogo || c.Type == CustomerConfigurationType.NotificationHeaderLogo)).ToList();
				if (fileUploads.Any()) {
					NotificationHeaderLogo = fileUploads.LastOrDefault(c => c.Type == CustomerConfigurationType.NotificationHeaderLogo).Upload?.Data;
					NotificationFooterLogo = fileUploads.LastOrDefault(c => c.Type == CustomerConfigurationType.NotificationFooterLogo).Upload?.Data;

				}
			}
		}

		private void EnsureContext() {
			if (ControllerContext == null) {
				RouteData routeData = null;
				HttpContextBase wrapper = null;

				if (System.Web.HttpContext.Current != null)
					wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
				else
					throw new InvalidOperationException(
						"Can't create Controller Context if no active HttpContext instance is available.");

				if (routeData == null)
					routeData = new RouteData();

				// add the controller routing if not existing
				if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
					routeData.Values.Add("controller", this.GetType().Name.ToLower().Replace("controller", ""));

				this.ControllerContext = new ControllerContext(wrapper, routeData, this);
			}
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

		public ActionResult CustomerSelfSignedUpCustomerCopy(CustomerSelfSignedUpEvent eventArgs) {
			if (Request.HttpMethod == "EMAIL")
				Handle(eventArgs);

			eventArgs.NotificationFooterLogo = NotificationFooterLogo;
			eventArgs.NotificationHeaderLogo = NotificationHeaderLogo;

			return View(CustomerSelfSignedUpCustomerCopyView, eventArgs);
		}

		public ActionResult CustomerSelfSignedUpAdminCopy(CustomerSelfSignedUpEvent eventArgs) {
			if (Request.HttpMethod == "EMAIL")
				Handle(eventArgs);
			eventArgs.CurrentAdminViewModel = eventArgs.CustomerAdminViewModelsList[0];
			eventArgs.NotificationFooterLogo = NotificationFooterLogo;
			eventArgs.NotificationHeaderLogo = NotificationHeaderLogo;
			return View(CustomerSelfSignedUpCustomerAdminCopyView, eventArgs);
		}

		[NonAction]
		public void Handle(CustomerSelfSignedUpEvent eventArgs) {
			EnsureContext();
			if (eventArgs.CompanyViewModel.IsSelfSignUpApprove) {
				var body = RenderViewToString(CustomerSelfSignedUpCustomerCopyView, eventArgs);
				SendMail(eventArgs.UserViewModel, body, eventArgs.Subject);
			} else {
				foreach (var userViewModel in eventArgs.CustomerAdminViewModelsList) {
					eventArgs.NotificationFooterLogo = NotificationFooterLogo;
					eventArgs.NotificationHeaderLogo = NotificationHeaderLogo;
					eventArgs.CurrentAdminViewModel = userViewModel;
					var body = RenderViewToString(CustomerSelfSignedUpCustomerAdminCopyView, eventArgs);
					SendMail(eventArgs.CurrentAdminViewModel, body,
						eventArgs.CompanyViewModel.CompanyName +
						" OnRamp Portal Signup Notification - please approve / deny");
				}
			}
		}

		public ActionResult CustomerSelfProvisionedCustomerCopy(CustomerSelfProvisionedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			if (Request.HttpMethod == "EMAIL")
				Handle(@event);
			return View(CustomerSelfProvisionedCustomerCopyView, @event);
		}

		[NonAction]
		public void Handle(CustomerSelfProvisionedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			EnsureContext();
			var body = RenderViewToString(CustomerSelfProvisionedCustomerCopyView, @event);
			SendMail(@event.UserViewModel, body, @event.Subject);
		}

		public ActionResult UserCreatedEventUserCopy(UserCreatedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			if (Request.HttpMethod == "EMAIL")
				Handle(@event);
			return View(UserCreatedEventUserCopyView, @event);
		}

		[NonAction]
		public void Handle(UserCreatedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			EnsureContext();
			var body = RenderViewToString(UserCreatedEventUserCopyView, @event);
			SendMail(@event.UserViewModel, body, @event.Subject);
			if (@event.CompanyViewModel.IsSendWelcomeSMS && !string.IsNullOrWhiteSpace(@event.UserViewModel.MobileNumber)) {
				var sms = SMS.SendCustomerLoginDetails(@event.UserViewModel, AppSettings.Urls.ResolveUrl("~", @event.CompanyViewModel));
				sms.Send();
			}
		}

		public ActionResult TrainingTestAssignedUserCopy(TrainingTestAssignedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			if (Request.HttpMethod == "EMAIL")
				Handle(@event);
			return View(TrainingTestAssignedUserCopyView, @event);
		}

		[NonAction]
		public void Handle(TrainingTestAssignedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			EnsureContext();
			var body = RenderViewToString(TrainingTestAssignedUserCopyView, @event);
			SendMail(@event.UserViewModel, body, @event.Subject);
			if (@event.CompanyViewModel.IsSendWelcomeSMS) {
				var sms = SMS.SendTestAssignmentNotification(@event.UserViewModel, @event.TrainingTestViewModel, AppSettings.Urls.Main);
				sms.Send();
			}
		}

		public ActionResult TrainingGuideAssignedUserCopy(TrainingGuideAssignedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			if (Request.HttpMethod == "EMAIL")
				Handle(@event);
			return View(TrainingGuideAssignedUserCopyView, @event);
		}

		[NonAction]
		public void Handle(TrainingGuideAssignedEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			EnsureContext();
			var body = RenderViewToString(TrainingGuideAssignedUserCopyView, @event);
			SendMail(@event.UserViewModel, body, @event.Subject);
			if (@event.CompanyViewModel.IsSendWelcomeSMS) {
				var sms = SMS.SendTrainingGuideAssignmentNotification(@event.UserViewModel, @event.TrainingGuideViewModel, AppSettings.Urls.Main);
				sms.Send();
			}
		}

		[NonAction]
		public void Handle(CustomerUserSelfSignedUpApprovedEvent @event) {
			EnsureContext();
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			@event.UserViewModel.Password = new EncryptionHelper().Decrypt(@event.UserViewModel.Password);
			var body = RenderViewToString(CustomerUserSelfSignedUpApprovedCustomerCustomerCopyView, @event);
			SendMail(@event.UserViewModel, body, @event.Subject);
			if (@event.CompanyViewModel.IsSendWelcomeSMS && !string.IsNullOrEmpty(@event.UserViewModel.MobileNumber)) {
				var sms = SMS.SendCustomerLoginDetails(@event.UserViewModel, AppSettings.Urls.ResolveUrl("~", @event.CompanyViewModel));
				sms.Send();
			}
		}

		public ActionResult TrainingGuideAndTestAssignmentEvent(TrainingGuideAndTestAssignedEvent @event) {
			if (Request.HttpMethod == "EMAIL") {
				@event.NotificationFooterLogo = NotificationFooterLogo;
				@event.NotificationHeaderLogo = NotificationHeaderLogo;
				Handle(@event);

			}
			return View(TrainingGuideAndTestAssignmentCustomerCopyView, @event);
		}

		[NonAction]
		public void Handle(TrainingGuideAndTestAssignedEvent @event) {
			EnsureContext();
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			var body = RenderViewToString(TrainingGuideAndTestAssignmentCustomerCopyView, @event);
			SendMail(@event.User, body, @event.Subject);
			if (@event.Company.IsSendWelcomeSMS && !string.IsNullOrEmpty(@event.User.MobileNumber)) {
				var sms = SMS.SendTrainingGuideAndTestAssignmentNotification(@event.User, @event.TrainingTest,
				@event.TrainingGuide, AppSettings.Urls.Main);
				sms.Send();
			}
		}

		public ActionResult LostPasswordEvent(LostPasswordEvent @event) {
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			if (Request.HttpMethod == "EMAIL") {
				Handle(@event);
			}
			return View(TrainingGuideAndTestAssignmentCustomerCopyView, @event);
		}

		[NonAction]
		public void Handle(LostPasswordEvent @event) {
			EnsureContext();
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			var body = RenderViewToString(LostPasswordUserCopyView, @event);
			SendMail(@event.User, body, @event.Subject + " -" + @event.User.FullName);
		}

		private void SendMail(UserModelShort user, string body, string subject) {
			SendMail(new UserViewModel { Id = user.Id, FirstName = user.Name, EmailAddress = user.Email }, body, subject);
		}

		private void SendMail(UserViewModel user, string body, string subject) {
			var message = new MailMessage {
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			message.To.Add(new MailAddress(user.EmailAddress, user.FirstName ?? user.EmailAddress));

			var cl = new SmtpClient();
			if (!string.IsNullOrWhiteSpace(cl.PickupDirectoryLocation) && cl.PickupDirectoryLocation.StartsWith("~")) {
				cl.PickupDirectoryLocation = Server.MapPath(cl.PickupDirectoryLocation);
				if (!Directory.Exists(cl.PickupDirectoryLocation))
					Directory.CreateDirectory(cl.PickupDirectoryLocation);
			}

			cl.Send(message);

			var correspondaceLogEntry = new UserCorrespondaceOccuredEvent() {
				UserCorrespondenceLogViewModel = new UserCorrespondenceLogViewModel {
					CorrespondenceDate = DateTime.Now,
					CorrespondenceType = UserCorrespondenceEnum.Email,
					Description = "[EMAIL] " + subject,
					MessageContent = body,
					UserViewModel = user
				}
			};

			new EventPublisher().Publish(correspondaceLogEntry);
		}

		[NonAction]
		public void Handle(FeedbackCreatedEvent @event) {
			EnsureContext();
			if (@event.FeedbackViewModel.Type == Domain.Customer.Models.Feedback.FeedbackType.Playbook) {
				@event.FeedbackViewModel.CompanyViewModel = PortalContext.Current.UserCompany;
				@event.NotificationFooterLogo = NotificationFooterLogo;
				@event.NotificationHeaderLogo = NotificationHeaderLogo;
				@event.FeedbackViewModel.SubjectName = @event.TrainingGuideViewModel.Title;
				foreach (var collaborator in @event.TrainingGuideViewModel.Collaborators) {
					@event.FeedbackViewModel.Collaborator = collaborator;
					var body = RenderViewToString(Feedback, @event.FeedbackViewModel);
					SendMail(collaborator,
								body,
								$"Playbook Feedback - { @event.FeedbackViewModel.Type.ToString() } from { @event.FeedbackViewModel.User.FullName }");

				}
			} else if (@event.FeedbackViewModel.Type == Domain.Customer.Models.Feedback.FeedbackType.Test) {
				@event.FeedbackViewModel.CompanyViewModel = PortalContext.Current.UserCompany;
				@event.FeedbackViewModel.SubjectName = @event.TrainingTestViewModel.TrainingGuideName;
				foreach (var collaborator in @event.TrainingGuideViewModel.Collaborators) {
					@event.FeedbackViewModel.Collaborator = collaborator;
					var body = RenderViewToString(Feedback, @event.FeedbackViewModel);
					SendMail(collaborator,
								body,
								$"Test Feedback - { @event.FeedbackViewModel.Type.ToString() } from { @event.FeedbackViewModel.User.FullName }");

				}
			}
		}

		public void Handle(UserFeedbackCreatedEvent @event) {
			EnsureContext();
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			@event.UserFeedbackViewModel.CompanyViewModel = PortalContext.Current.UserCompany;
			var subject = $"{VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(@event.UserFeedbackViewModel.DocumentListModel.DocumentType)} Feedback - {@event.UserFeedbackViewModel.UserFeedback.ContentType.ToString()} from { @event.UserFeedbackViewModel.UserViewModel.FullName }";
			foreach (var collaborator in @event.UserFeedbackViewModel.DocumentListModel.Collaborators) {
				@event.UserFeedbackViewModel.Collaborator = collaborator;
				var body = RenderViewToString(UserFeedback, @event.UserFeedbackViewModel);
				SendMail(collaborator, body, subject);
			}
		}

		public void Handle(SendTestExpiryNotificationEvent @event) {
			EnsureContext();
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			var body = RenderViewToString(SendTestExpiryNotificationUserCopyView, @event);
			SendMail(@event.UserViewModel, body, @event.Subject);
			if (@event.CompanyViewModel.IsSendWelcomeSMS  && !string.IsNullOrEmpty(@event.UserViewModel.MobileNumber)) {
				var sms = SMS.SendTestExpiryNotification(@event.UserViewModel, @event.TrainingTestViewModel, AppSettings.Urls.Main);
				sms.Send();
			}
		}
		public void Handle(TrainingTestCompletedEvent @event) {
			if (@event.TestResultViewModel != null && @event.TestResultViewModel.EmailSummaryOnCompletion) {
				EnsureContext();
				@event.NotificationFooterLogo = NotificationFooterLogo;
				@event.NotificationHeaderLogo = NotificationHeaderLogo;
				var body = RenderViewToString(TrainingTestCompletedEventUserCopyView, @event);
				SendMail(@event.UserViewModel, body, @event.Subject);
			}
		}



		[NonAction]
		public void Handle(VirtualMeetingReminderEvent @event) {
			EnsureContext();
			var body = RenderViewToString(VirtualMeetingReminderEvent, @event);
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			SendMail(@event.UserViewModel, body, @event.Subject);
		}


		[NonAction]
		public void Handle(VirtualClassroomEvent @event) {
			EnsureContext();
			var body = RenderViewToString(VirtualClassroomMeeting, @event);
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			SendMail(@event.UserViewModel, body, @event.Subject);
			//if (@event.CompanyViewModel != null ? @event.CompanyViewModel.IsSendWelcomeSMS : false && !string.IsNullOrWhiteSpace(@event.UserViewModel.MobileNumber)) {
			//	var sms = SMS.SendDocumentsAssignmentNotification(@event.UserViewModel, @event.DocumentTitles, AppSettings.Urls.Main);
			//	sms.Send();
			//}
		}

		[NonAction]
		public void Handle(CancelVirtualRoomEvent @event)
		{
			EnsureContext();
			@event.UserViewModel.VirtualClassroom = @event.MeetingName;

			if (!@event.IsConfirmed) {
				var body = RenderViewToString(CancelVirtualMeetingEvent, @event);
				@event.NotificationFooterLogo = NotificationFooterLogo;
				@event.NotificationHeaderLogo = NotificationHeaderLogo;
				@event.UserViewModel.AdditionalNote = @event.AdditionNote;
				SendMail(@event.UserViewModel, body, @event.Subject);
			}
			if (@event.UserViewModel != null && !string.IsNullOrEmpty(@event.UserViewModel.MobileNumber))
			{
				if (@event.CompanyViewModel != null ? @event.CompanyViewModel.IsSendWelcomeSMS : false && !string.IsNullOrEmpty(@event.UserViewModel.MobileNumber))
				{
					if (@event.IsConfirmed)
					{
						var sms = SMS.SendConfirmedMeetingNotification(@event.UserViewModel, @event.DocumentTitles, AppSettings.Urls.Main);
						sms.Send();
					}
					else
					{
						var sms = SMS.SendCancelMeetingNotification(@event.UserViewModel, @event.DocumentTitles, AppSettings.Urls.Main);
						sms.Send();
					}
				}
			}
		}

		[NonAction]
		public void Handle(DocumentsAssignedEvent @event) {
			EnsureContext();
			
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
		
				
			if (@event.UserViewModel != null) {
				var body = RenderViewToString(DocumentsAssignedUserCopyView, @event);
				@event.UserViewModel.AdditionalNote = @event.AdditionalMessage;
				SendMail(@event.UserViewModel, body, @event.Subject);
				if (@event.CompanyViewModel != null ? @event.CompanyViewModel.IsSendWelcomeSMS : false && !string.IsNullOrWhiteSpace(@event.UserViewModel.MobileNumber)) {
					if (@event.IsAssigned) {
						var sms = SMS.SendDocumentsAssignmentNotification(@event.UserViewModel, @event.DocumentTitles, AppSettings.Urls.Main);
						sms.Send();
					} else {
						var sms = SMS.SendDocumentsUnassignmentNotification(@event.UserViewModel, @event.DocumentTitles, AppSettings.Urls.Main);
						sms.Send();
					}
				}
			}
		}

		[NonAction]
		public void Handle(DocumentWorkflowEvent @event)
		{
			EnsureContext();
			var body = RenderViewToString(DocumentWorkflowEventView, @event);
			
			@event.NotificationFooterLogo = NotificationFooterLogo;
			@event.NotificationHeaderLogo = NotificationHeaderLogo;
			@event.UserViewModel.AdditionalNote = @event.AdditionalMessage;
			SendMail(@event.UserViewModel, body, @event.Subject);

			//if (@event.UserViewModel != null && !string.IsNullOrEmpty(@event.UserViewModel.MobileNumber))
			//{
			//	if (@event.CompanyViewModel != null ? @event.CompanyViewModel.IsSendWelcomeSMS : false && !string.IsNullOrWhiteSpace(@event.UserViewModel.MobileNumber))
			//	{
			//		if (@event.IsAssigned)
			//		{
			//			var sms = SMS.SendDocumentsAssignmentNotification(@event.UserViewModel, @event.DocumentTitles, AppSettings.Urls.Main);
			//			sms.Send();
			//		}
			//		else
			//		{
			//			var sms = SMS.SendDocumentsUnassignmentNotification(@event.UserViewModel, @event.DocumentTitles, AppSettings.Urls.Main);
			//			sms.Send();
			//		}
			//	}
			//}
		}

		public void Handle(TestCompletedEvent @event) {
			if (@event.TestResultModel != null) {
				EnsureContext();
				@event.NotificationFooterLogo = NotificationFooterLogo;
				@event.NotificationHeaderLogo = NotificationHeaderLogo;
				var body = RenderViewToString(TestCompletedEventUserCopyView, @event);
				SendMail(@event.UserViewModel, body, @event.Subject);
			}
		}
		public void Handle(RemoveParticipantEvent @event) {
			if (@event != null) {
				EnsureContext();
				@event.NotificationFooterLogo = NotificationFooterLogo;
				@event.NotificationHeaderLogo = NotificationHeaderLogo;
				var body = RenderViewToString(RemoveParticipantEvent, @event);
				SendMail(@event.UserViewModel, body, @event.Subject);
			}
		}
	}
}