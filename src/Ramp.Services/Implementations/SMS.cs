using Common.Events;
using Domain.Models;
using javax.naming;
using Ramp.Contracts.Events.ActivityManagement;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Ramp.Services.Implementations
{
    public class SMS
    {
        private string _body;
        private readonly string _domain, _reference;
        private readonly IEnumerable<DocumentTitlesAndTypeQuery> _documentTitles;
        private readonly TrainingTestViewModel _trainingTestViewModel;
        private readonly TrainingGuideViewModel _trainingGuideViewModel;
        private readonly UserViewModel _userViewModel;
        private readonly NotificationType _notificationType;

        private const string SmsPickupDirectory = "SMSPickupDirectory",
            RequestContentType = "application/x-www-form-urlencoded";

        public static SMS SendCustomerLoginDetails(UserViewModel userViewModel, string domain)
        {
            return new SMS(NotificationType.SendUserCredentials, userViewModel, null, null, domain);
        }

        public static SMS SendTrainingGuideAndTestAssignmentNotification(UserViewModel userViewModel,
            TrainingTestViewModel trainingTestViewModel, TrainingGuideViewModel trainingGuideViewModel, string domain)
        {
            return new SMS(NotificationType.TrainingGuideAndTestAssignmentNotification, userViewModel,
                trainingTestViewModel, trainingGuideViewModel, domain);
        }

        public static SMS SendTestAssignmentNotification(UserViewModel userViewModel,
            TrainingTestViewModel trainingTestViewModel, string domain)
        {
            return new SMS(NotificationType.TestAssignmentNotification, userViewModel, trainingTestViewModel, null, domain);
        }

        public static SMS SendTrainingGuideAssignmentNotification(UserViewModel userViewModel,
            TrainingGuideViewModel trainingGuideViewModel, string domain)
        {
            return new SMS(NotificationType.TrainingGuideAssignmentNotification, userViewModel, null, trainingGuideViewModel, domain);
        }

        public static SMS SendDocumentsAssignmentNotification(UserViewModel userViewModel,
            IEnumerable<DocumentTitlesAndTypeQuery> documentTitles, string domain)
        {
            return new SMS(NotificationType.DocumentAssignmentNotification, userViewModel, documentTitles, domain);
        }
		public static SMS SendDocumentsUnassignmentNotification(UserViewModel userViewModel,
			IEnumerable<DocumentTitlesAndTypeQuery> documentTitles, string domain) {
			return new SMS(NotificationType.DocumentUnAssignmentNotification, userViewModel, documentTitles, domain );
		}

		public static SMS SendCancelMeetingNotification(UserViewModel userViewModel,
			IEnumerable<DocumentTitlesAndTypeQuery> documentTitles, string domain) {
			return new SMS(NotificationType.CancelMeetingNotification, userViewModel, documentTitles, domain);
		}

		public static SMS SendConfirmedMeetingNotification(UserViewModel userViewModel,
			IEnumerable<DocumentTitlesAndTypeQuery> documentTitles, string domain) {
			return new SMS(NotificationType.ConfirmedMeetingNotification, userViewModel, documentTitles, domain);
		}

		public static SMS SendTestExpiryNotification(UserViewModel userViewModel,
          TrainingTestViewModel trainingTestViewModel, string domain)
        {
            return new SMS(NotificationType.SendTestExpiryNotification, userViewModel, trainingTestViewModel, null, domain);
        }
        private SMS(NotificationType notificationType, UserViewModel userViewModel, TrainingTestViewModel trainingTestViewModel, TrainingGuideViewModel trainingGuideViewModel, string domain)
        {
            if (string.IsNullOrWhiteSpace(userViewModel.MobileNumber))
                throw new NullReferenceException();

            _userViewModel = userViewModel;
            _domain = domain;
            _trainingTestViewModel = trainingTestViewModel;
            _trainingGuideViewModel = trainingGuideViewModel;
            _notificationType = notificationType;
            switch (notificationType)
            {
                case NotificationType.NotifyAboutTestExpiryToAllAdmins:
                    CreateNotifyAboutTestExpiryToAllAdminsSms();
                    break;

                case NotificationType.RetakeTest:
                    CreateRetakeTestSms();
                    break;

                case NotificationType.SendUserCredentials:
                    CreateSendUserCredentialsSms();
                    break;

                case NotificationType.TestAssignmentNotification:
                    if (_trainingTestViewModel == null)
                        throw new NullReferenceException();

                    _reference = _trainingTestViewModel.TestTitle;
                    CreateTestAssigmentNotificationSms();
                    break;
                case NotificationType.SendTestExpiryNotification:
                    if (_trainingTestViewModel == null)
                        throw new NullReferenceException();

                    _reference = _trainingTestViewModel.TestTitle;
                    CreateSendTestExpiryNotificationSms();
                    break;

                case NotificationType.TrainingGuideAssignmentNotification:
                    if (_trainingGuideViewModel == null)
                        throw new NullReferenceException();

                    _reference = _trainingGuideViewModel.Title;
                    CreateTrainingGuideAssignmentNotification();
                    break;

                case NotificationType.TrainingGuideAndTestAssignmentNotification:
                    if (_trainingGuideViewModel == null || _trainingTestViewModel == null)
                        throw new NullReferenceException();
                    _reference = "New Playbook & Test Notification - " + _trainingGuideViewModel.Title;
                    CreateTrainingGuideAndTestAssignmentNotification();
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private SMS(NotificationType notificationType, UserViewModel userViewModel, IEnumerable<DocumentTitlesAndTypeQuery> documentTitles,
            string domain)
        {
            _userViewModel = userViewModel;
            _documentTitles = documentTitles != null ? documentTitles.ToList() : new List<DocumentTitlesAndTypeQuery>();
            _domain = domain;
            _notificationType = notificationType;

            if (string.IsNullOrWhiteSpace(userViewModel.MobileNumber))
                throw new NullReferenceException();

            switch (notificationType)
            {
                case NotificationType.DocumentAssignmentNotification:
                    if (_documentTitles != null && !documentTitles.Any())
                        throw new ArgumentException();
                    CreateDocumentsAssignmentNotification();
                    break;
				case NotificationType.DocumentUnAssignmentNotification:
					if (_documentTitles != null &&  !documentTitles.Any())
						throw new ArgumentException();
					CreateDocumentsUnAssignmentNotification();
					break;
				case NotificationType.CancelMeetingNotification:
					
					CreateCancelMeetingNotification();
					break;
				case NotificationType.ConfirmedMeetingNotification:

					CreateConfirmedMeetingNotification();
					break;

				default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private void CreateNotifyAboutTestExpiryToAllAdminsSms()
        {
        }

        private void CreateRetakeTestSms()
        {
        }

        private void CreateSendUserCredentialsSms()
        {
            var body = new StringBuilder();
            body.AppendFormat("Hi {0}, \r\n", _userViewModel.FirstName);
            body.Append("Your login was created successfully.\n\r");
            body.AppendFormat(" Username: {0} \r\n", _userViewModel.EmailAddress);
            body.AppendFormat(" Password: {0} \r\n", _userViewModel.Password);
            body.AppendFormat(" Your SubDomain: {0} \r\n", _domain);
            body.Append("For more details please check your email.");

            _body = body.ToString();
        }

        private void CreateTestAssigmentNotificationSms()
        {
            var body = new StringBuilder();
            body.AppendFormat("Hi {0}! \r\n", _userViewModel.FirstName);
            body.AppendFormat(" You have been assigned a test on playbook: {0}. \r\n", _trainingTestViewModel.ReferenceId);
            body.AppendFormat(" Please login to {0} to take your test.", _domain);
            _body = body.ToString();
        }
        private void CreateSendTestExpiryNotificationSms()
        {
            var body = new StringBuilder();
            body.AppendFormat("Hi {0}! \r\n", _userViewModel.FirstName);
            body.AppendFormat(" Your Test {0} is exiring on {1}. \r\n", _trainingTestViewModel.ReferenceId,!_trainingTestViewModel.TestExpiryDate.HasValue ? string.Empty : _trainingTestViewModel.TestExpiryDate.Value.ToShortDateString());
            body.AppendFormat(" Please login to {0} to complete the test.", _domain);
            _body = body.ToString();
        }
        private void CreateTrainingGuideAssignmentNotification()
        {
            var body = new StringBuilder();
            body.AppendFormat("Hi {0}! \r\n", _userViewModel.FirstName);
            body.AppendFormat(" You have been assigned the following playbook: {0}. \r\n", _trainingGuideViewModel.ReferenceId);
            body.AppendFormat(" Login to {0} to view the playbook.", _domain);
            _body = body.ToString();
        }

        private void CreateTrainingGuideAndTestAssignmentNotification()
        {
            var body = new StringBuilder();
            body.AppendFormat("Dear {0} {1},\r\n", _userViewModel.FirstName, _userViewModel.LastName);
            body.AppendFormat("Please note that the following playbook has been assigned to you to complete: {0}.\r\n",
                _trainingGuideViewModel.Title);
            body.Append("There is also a test on the content in this playbook.\r\n");
            if (_trainingTestViewModel.TestExpiryDate.HasValue)
            {
                body.AppendFormat("Please note you have until {0} to complete the playbook and test.\r\n", _trainingTestViewModel.TestExpiryDate.Value.ToShortDateString());
            }
            body.AppendFormat("Login to {0} to view the playbook and test.", _domain);
            _body = body.ToString();
        }

        private void CreateDocumentsAssignmentNotification()
        {
            var body = new StringBuilder();
            body.AppendFormat("Dear {0} {1},\r\n", _userViewModel.FirstName, _userViewModel.LastName);
            body.AppendFormat("Please note that the following document{0} has been assigned to you:\r\n",
                _documentTitles.Count() > 1 ? "s" : String.Empty);
			if (!string.IsNullOrEmpty(_userViewModel.AdditionalNote))	
			foreach (var documentTitle in _documentTitles)
            {
                body.AppendFormat("{0}\r\n", documentTitle.DocumentTitle);
            }
            body.AppendFormat("\r\nAdditional Note:- {0} \r\n", _userViewModel.AdditionalNote + "\r\n");
            _body = body.ToString();
        }
		private void CreateDocumentsUnAssignmentNotification() {
			var body = new StringBuilder();
			body.AppendFormat("Dear {0} {1},\r\n", _userViewModel.FirstName, _userViewModel.LastName);
			body.AppendFormat("Please note that the following document{0} has been unassigned from you:\r\n",
				_documentTitles.Count() > 1 ? "s" : String.Empty);
			if (!string.IsNullOrEmpty(_userViewModel.AdditionalNote))
			
			foreach (var documentTitle in _documentTitles) {
				body.AppendFormat("{0}\r\n", documentTitle.DocumentTitle);
			}
            body.AppendFormat("\r\nAdditional Note:- {0} \r\n", _userViewModel.AdditionalNote + "\r\n");
            _body = body.ToString();
		}

		private void CreateCancelMeetingNotification() {
			var body = new StringBuilder();
			body.AppendFormat("Dear {0},\r\n", _userViewModel.FirstName.Trim());
			body.AppendFormat("Please note that the following meeting {0} has been cancelled from you:\r\n",
				_userViewModel.VirtualClassroom);
			if (!string.IsNullOrEmpty(_userViewModel.AdditionalNote))
				body.AppendFormat("\r\nAdditional Note:- {0} \r\n", _userViewModel.AdditionalNote + "\r\n");

			body.AppendFormat("\r\nVirtual Meeting Room Name: {0}\r\n", _userViewModel.VirtualClassroom);
			body.AppendFormat("Start Date: {0}\r\n", _userViewModel.StartDate);
			body.AppendFormat("End Date: {0}\r\n", _userViewModel.EndDate);

			_body = body.ToString();
		}
		private void CreateConfirmedMeetingNotification() {
			var body = new StringBuilder();
			body.AppendFormat("{0},\r\n", _userViewModel.FirstName.Trim());
			body.AppendFormat("Please note that you have been sent a Virtual Meeting Invite:\r\n",
				_userViewModel.VirtualClassroom);
			
			body.AppendFormat("\r\nVirtual Meeting Room Name: {0}\r\n", _userViewModel.VirtualClassroom);
			body.AppendFormat("Start Date: {0}\r\n", _userViewModel.StartDate);
			body.AppendFormat("End Date: {0}\r\n", _userViewModel.EndDate);
			 

			_body = body.ToString();
		}

		private string CreateSmsUrl()
        {
            var smsUrl = ConfigurationManager.AppSettings["SmsUrl"];

            smsUrl += "&username=" + ConfigurationManager.AppSettings["SmsUsername"];

            smsUrl += "&password=" + ConfigurationManager.AppSettings["SmsPassword"];

            smsUrl += "&numto=" + _userViewModel.MobileNumber;

            smsUrl += "&data1=" + _body;

            return smsUrl;
        }
			
        public void Send()
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[SmsPickupDirectory].Trim()))
                {
                    var request = WebRequest.Create(CreateSmsUrl());
                    request.ContentType = RequestContentType;

                    using (var response = request.GetResponse())
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            var doc = XDocument.Load(stream);
                            var docString = doc.ToString();
                        }
                    }
                }
                else
                {
                    var dropPath = ConfigurationManager.AppSettings[SmsPickupDirectory].Replace("~", HttpRuntime.AppDomainAppPath).Trim();
                    if (!Directory.Exists(dropPath))
                        Directory.CreateDirectory(dropPath);

                    using (
                        var fs =
                            File.CreateText(Path.Combine(dropPath,
                                Guid.NewGuid() + ".txt")))
                    {
                        if (!string.IsNullOrWhiteSpace(_reference))
                        {
                            fs.WriteLine(_reference);
                            fs.WriteLine("---");
                        }
                        fs.WriteLine(_body);
                    }
                }
                new EventPublisher().Publish(new UserCorrespondaceOccuredEvent
                {
                    UserCorrespondenceLogViewModel = new UserCorrespondenceLogViewModel
                    {
                        CorrespondenceType = UserCorrespondenceEnum.Sms,
                        CorrespondenceDate = DateTime.Now,
                        Description =
                              GetCorrespondenceDescription(_notificationType) + " Reference No: " + _reference,
                        MessageContent = _body,
                        UserViewModel = _userViewModel
                    }
                });
            }
            catch (WebException)
            {
                new EventPublisher().Publish(new UserCorrespondaceOccuredEvent
                {
                    UserCorrespondenceLogViewModel = new UserCorrespondenceLogViewModel
                    {
                        CorrespondenceType = UserCorrespondenceEnum.Sms,
                        CorrespondenceDate = DateTime.Now,
                        Description = "[FAILED] " +
                             GetCorrespondenceDescription(_notificationType) + " Reference No: " + _reference,
                        MessageContent = _body,
                        UserViewModel = _userViewModel
                    }
                });
            }
        }

        private string GetCorrespondenceDescription(NotificationType type)
        {
            string description = "";
            switch (type)
            {
                case NotificationType.TestAssignmentNotification:
                    description = "[SMS] Test Assignment Notification";
                    break;
                case NotificationType.SendTestExpiryNotification:
                    description = "[SMS] Test Expiry Notification";
                    break;

                case NotificationType.TrainingGuideAssignmentNotification:
                    description = "[SMS] Playbook Assignment Notification";
                    break;

                case NotificationType.SendUserCredentials:
                    description = "[SMS] Send User Credentials SMS";
                    break;

                case NotificationType.TrainingGuideAndTestAssignmentNotification:
                    description = "[SMS] Playbook and Test Assignment Notification";
                    break;
                case NotificationType.DocumentAssignmentNotification:
                    description = "[SMS] Document Assignment Notification";
                    break;
				case NotificationType.DocumentUnAssignmentNotification:
					description = "[SMS] Document Unassignment Notification";
					break;
				case NotificationType.CancelMeetingNotification:
					description = "[SMS] Cancelled Meeting Notification";
					break;
				case NotificationType.ConfirmedMeetingNotification:
					description = "[SMS] Confirmed Meeting Notification";
					break;

				default:
                    throw new InvalidEnumArgumentException();
            }

            return description;
        }
    }

    public enum NotificationType
    {
        TestAssignmentNotification,
        TrainingGuideAssignmentNotification,
        TrainingGuideAndTestAssignmentNotification,
        DocumentAssignmentNotification,
        DocumentUnAssignmentNotification,
        SendTestExpiryNotification,
		CancelMeetingNotification,
		ConfirmedMeetingNotification,
		[Description("RetakeTestInvite")]
        RetakeTest,

        [Description("NotifyAboutTestExpiryToAllAdmins")]
        NotifyAboutTestExpiryToAllAdmins,

        [Description("SendUserCredentialsSMS")]
        SendUserCredentials
    }
}