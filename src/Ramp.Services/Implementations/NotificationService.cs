using Common.Command;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.CorrespondenceManagement;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Ramp.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public NotificationService(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        #region Send email

        private bool PerformRequiredEmailComposeActionsAndSendEmail(string toEmailAddress,
            string encodedHtmlTemplateViewString,
            string subject, IEnumerable<string> filePaths)
        {
            try
            {
                var smtpClient = new SmtpClient();
                var message = new MailMessage();

                if (!string.IsNullOrWhiteSpace(smtpClient.PickupDirectoryLocation) && smtpClient.PickupDirectoryLocation.StartsWith("~"))
                {
                    smtpClient.PickupDirectoryLocation = HostingEnvironment.MapPath(smtpClient.PickupDirectoryLocation);
                    if (!Directory.Exists(smtpClient.PickupDirectoryLocation))
                        Directory.CreateDirectory(smtpClient.PickupDirectoryLocation);
                }

                message.To.Add(toEmailAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;

                AlternateView messageBody = AlternateView.CreateAlternateViewFromString(encodedHtmlTemplateViewString,
                    null,
                    "text/html");

                if (filePaths != null)
                {
                    foreach (string filePath in filePaths)
                    {
                        var attachment = new Attachment(filePath, MediaTypeNames.Application.Octet);
                        message.Attachments.Add(attachment);
                    }
                }

                message.AlternateViews.Add(messageBody);
                smtpClient.Send(message);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion Send email

        #region Resolve view with ViewData, TempData, Model, ControllerContext, ViewName

        private string ReturnEncodedHtmlStringForView(object model,
            ControllerContext controllerContext,
            ViewDataDictionary viewData,
            TempDataDictionary tempData,
            string viewName)
        {
            string viewInEncodedHtmlStringFormat;
            viewData.Model = model;
            using (var stringWriterForEncodingViewToStringFormat = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData,
                    stringWriterForEncodingViewToStringFormat);
                viewResult.View.Render(viewContext, stringWriterForEncodingViewToStringFormat);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                viewInEncodedHtmlStringFormat = stringWriterForEncodingViewToStringFormat.ToString();
            }

            return viewInEncodedHtmlStringFormat;
        }

        #endregion Resolve view with ViewData, TempData, Model, ControllerContext, ViewName

        #region Functions to send email

        public bool ComposeAndSendEmail(EmailTemplate emailTemplate)
        {
            if (emailTemplate.EmailAddress != null)
            {
                string viewName = GetEnumDescription(emailTemplate.Type);
                string viewInEncodedHtmlStringFormat = ReturnEncodedHtmlStringForView(emailTemplate.Model,
                    emailTemplate.ControllerContext,
                    emailTemplate.ViewData,
                    emailTemplate.TempData, viewName);

                return PerformRequiredEmailComposeActionsAndSendEmail(emailTemplate.EmailAddress,
                    viewInEncodedHtmlStringFormat,
                    emailTemplate.Subject, null);
            }

            return false;
        }

        public bool ComposeAndSendEmailWithAttachments(EmailTemplate emailTemplate)
        {
            if (emailTemplate.EmailAddress != null)
            {
                string viewName = GetEnumDescription(emailTemplate.Type);
                string viewInEncodedHtmlStringFormat = ReturnEncodedHtmlStringForView(emailTemplate.Model,
                    emailTemplate.ControllerContext,
                    emailTemplate.ViewData,
                    emailTemplate.TempData, viewName);

                return PerformRequiredEmailComposeActionsAndSendEmail(emailTemplate.EmailAddress,
                    viewInEncodedHtmlStringFormat,
                    emailTemplate.Subject,
                    emailTemplate.FilePaths);
            }

            return false;
        }

        #endregion Functions to send email

        #region Enum description resolver

        private string GetEnumDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])field.GetCustomAttributes
                    (typeof(DescriptionAttribute), false);

            return attributes[0].Description;
        }

        #endregion Enum description resolver

        #region Nested type: EmailTemplate

        public class EmailTemplate
        {
            public object Model { get; set; }
            public ControllerContext ControllerContext { get; set; }
            public ViewDataDictionary ViewData { get; set; }
            public TempDataDictionary TempData { get; set; }
            public string EmailAddress { get; set; }
            public NotificationType Type { get; set; }
            public IList<string> FilePaths { get; set; }
            public string Subject { get; set; }
        }

        #endregion Nested type: EmailTemplate

        #region Nested type: EmailSend

        public class EmailSend
        {
            //public ControllerContext ControllerContext { get; set; }
            //public ViewDataDictionary ViewData { get; set; }
            //public TempDataDictionary TempData { get; set; }

            public string UserName { get; set; }
            public string userAdminName { get; set; }
            public string password { get; set; }
            public string Subject { get; set; }
            public string SubDomain { get; set; }
            public string EmailAddress { get; set; }
            public string sublayer { get; set; }
            public List<string> bulkEmails { get; set; }
            public Guid userId { get; set; }
            public string mobileNumber { get; set; }
            public string hostUrl { get; set; }
            public string CompanyName { get; set; }
        }

        #endregion Nested type: EmailSend

        #region SendEmailLoginSelfProvision

        public bool SendEmailLoginSelfProvisionUser(EmailSend emailTemplate)
        {
            try
            {
                var smtpClient = new SmtpClient();
                var message = new MailMessage();
                var user = new User();
                // message.To.Add(toEmailAddress);

                if (!string.IsNullOrWhiteSpace(smtpClient.PickupDirectoryLocation) && smtpClient.PickupDirectoryLocation.StartsWith("~"))
                {
                    smtpClient.PickupDirectoryLocation = HostingEnvironment.MapPath(smtpClient.PickupDirectoryLocation);
                    if (!Directory.Exists(smtpClient.PickupDirectoryLocation))
                        Directory.CreateDirectory(smtpClient.PickupDirectoryLocation);
                }

                #region Send Email

                //Sending Email
                message.To.Add(emailTemplate.EmailAddress);
                message.Subject = "Email confirmation";
                var path = ConfigurationManager.AppSettings["EmailImagePath"];

                var headerPath = emailTemplate.sublayer + "Content/images/topBg.jpg";
                var footerPath = emailTemplate.sublayer + "Content/images/logoBg.jpg";
                var userProfilepath = emailTemplate.sublayer + "Content/images/manageUsers.png";

                StringBuilder shownAddress = new StringBuilder();
                try
                {
                    var parts = emailTemplate.hostUrl.Split('.');
                    var temp = parts[parts.Length - 1].Split('/');

                    for (int i = 0; i < parts.Length - 1; i++)
                        shownAddress.Append(parts[i]);

                    if (!string.IsNullOrEmpty(temp[0]))
                        shownAddress.Append(temp[0]);
                }
                catch (IndexOutOfRangeException) { }

                StringBuilder body = new StringBuilder();
                body.AppendFormat("<p>Dear {0}</p><BR />", emailTemplate.UserName);
                body.Append("<p>Your account has been approved,");
                body.Append("please click on the following link to sign in with your account details: ");
                if (!shownAddress.ToString().Contains("/Account/Login"))
                    body.AppendFormat("<a href =\"{0}\" title=\"User Email Confirm\">{0}</a></p>", shownAddress.ToString());
                else { body.AppendFormat("<a href =\"{0}\" title=\"User Email Confirm\">{0}</a></p>", emailTemplate.hostUrl); }
                body.Append("<p>Regards,</p><BR />");
                body.Append("<p>The OnRamp Team</p>");

                string a = WebUtility.HtmlEncode("<img src='" + headerPath + "' width='100%' height='35px' /><br /> <br /><p style='margin-top:10px;'> <BR /> " + body.ToString() + " <p style='margin-bottom: 10px;'>Regards</p><br /><img src='" + footerPath + "' width='100%'height='35px' />");
                string b = WebUtility.HtmlDecode(a);
                message.Body = b;
                message.IsBodyHtml = true;
                smtpClient.Send(message);

                #endregion Send Email
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion SendEmailLoginSelfProvision

        public class EmailSendUserExpire
        {
            //public ControllerContext ControllerContext { get; set; }
            //public ViewDataDictionary ViewData { get; set; }
            //public TempDataDictionary TempData { get; set; }

            public string userAdminName { get; set; }
            public string ToEmail { get; set; }
            public string Subject { get; set; }
            public string SubDomain { get; set; }
            public string EmailAddress { get; set; }

            public IEnumerable<StandardUser> UserList { get; set; }

            public string HeaderUrl { get; set; }
        }

        #region SendEmailCustomerSelfSignUp

        public bool SendEmailCustomerSelfSignUp(EmailSend emailTemplate)
        {
            if (emailTemplate.EmailAddress != null)
            {
                return SendEmailSucessCustomerSelfSignUp(emailTemplate);
            }

            return false;
        }

        public bool SendEmailSucessCustomerSelfSignUp(EmailSend emailTemplate)
        {
            try
            {
                var smtpClient = new SmtpClient();
                var message = new MailMessage();
                var user = new User();
                // message.To.Add(toEmailAddress);

                if (!string.IsNullOrWhiteSpace(smtpClient.PickupDirectoryLocation) && smtpClient.PickupDirectoryLocation.StartsWith("~"))
                {
                    smtpClient.PickupDirectoryLocation = HostingEnvironment.MapPath(smtpClient.PickupDirectoryLocation);
                    if (!Directory.Exists(smtpClient.PickupDirectoryLocation))
                        Directory.CreateDirectory(smtpClient.PickupDirectoryLocation);
                }

                #region Send Email

                //Sending Email
                message.To.Add(emailTemplate.EmailAddress);
                message.Subject = "Email confirmation";

                var path = ConfigurationManager.AppSettings["EmailImagePath"];

                var headerPath = emailTemplate.sublayer + "Content/images/topBg.jpg";
                var footerPath = emailTemplate.sublayer + "Content/images/logoBg.jpg";
                var userProfilepath = emailTemplate.sublayer + "Content/images/manageUsers.png";
                StringBuilder body = new StringBuilder();
                body.AppendFormat("<img src='{0}' width='100%' height='35px' />", headerPath);
                body.AppendFormat("<br /> <br /><p>Dear {0},</p>", emailTemplate.UserName);
                body.Append("<br /><p>Your account has been approved, please click on the following link to sign in with your account details:<br />");
                body.AppendFormat("<a href='https://{0}.onramp.training/Account/Login' title='User account'>https://{0}.onramp.training</a></p>", emailTemplate.SubDomain);
                body.Append("<br />Regards,<br />");
                body.Append("The OnRamp Team");
                body.AppendFormat("<br /><img src='{0}'  width='100%' height='35px'/>", footerPath);

                //string a = WebUtility.HtmlEncode("<img src='" + headerPath + "' width='100%' height='35px' /><br /> <br /><p>Dear " +
                //    emailTemplate.userAdminName + " <p>" + emailTemplate .UserName+ ", has signed-up to OnRamp portal.</p>  <p style='margin-bottom: 10px;'>Regards</p> <p>The ONRAMP Team</p><br /><img src='" + footerPath + "'  width='100%' height='35px' style=''/>");

                string b = WebUtility.HtmlDecode(WebUtility.HtmlEncode(body.ToString()));
                message.Body = b;
                message.IsBodyHtml = true;
                smtpClient.Send(message);

                #endregion Send Email
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion SendEmailCustomerSelfSignUp

        #region SendEmailCustomerSelfSignUpApprovedStatus

        public bool SendEmailCustomerSelfSignUpApprovedStatus(EmailSend emailTemplate)
        {
            if (emailTemplate.EmailAddress != null)
            {
                return SendEmailCustomerSelfSignUpApproved(emailTemplate);
            }

            return false;
        }

        public bool SendEmailCustomerSelfSignUpApproved(EmailSend emailTemplate)
        {
            try
            {
                var smtpClient = new SmtpClient();
                var message = new MailMessage();
                var user = new User();
                // message.To.Add(toEmailAddress);

                if (!string.IsNullOrWhiteSpace(smtpClient.PickupDirectoryLocation) && smtpClient.PickupDirectoryLocation.StartsWith("~"))
                {
                    smtpClient.PickupDirectoryLocation = HostingEnvironment.MapPath(smtpClient.PickupDirectoryLocation);
                    if (!Directory.Exists(smtpClient.PickupDirectoryLocation))
                        Directory.CreateDirectory(smtpClient.PickupDirectoryLocation);
                }

                #region Send Email

                //Sending Email
                message.To.Add(emailTemplate.EmailAddress);
                message.Subject = "Account approve confirmation";
                var path = ConfigurationManager.AppSettings["EmailImagePath"];

                var headerPath = emailTemplate.sublayer + "Content/images/topBg.jpg";
                var footerPath = emailTemplate.sublayer + "Content/images/logoBg.jpg";
                var userProfilepath = emailTemplate.sublayer + "Content/images/manageUsers.png";
                string a = WebUtility.HtmlEncode("<img src='" + headerPath + "' width='100%' height='35px' /><br /> <br /><p>Dear " + emailTemplate.userAdminName + " ,</p><p>" + emailTemplate.UserName + ", has signed-up to OnRamp portal and has been automatically approved.</p>  <p style='margin-bottom: 10px;'>Regards</p> <p>The ONRAMP Team</p><br /><img src='" + footerPath + "'  width='100%' height='35px' style=''/>");

                string b = WebUtility.HtmlDecode(a);
                message.Body = b;
                message.IsBodyHtml = true;
                smtpClient.Send(message);

                #endregion Send Email
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        //SendEmailCustomerSelfSignUpApprovedStatus

        #endregion SendEmailCustomerSelfSignUpApprovedStatus

        #region SendEmailCustomerExpire

        public bool SendEmailCustomerExpire(EmailSendUserExpire emailTemplate)
        {
            if (emailTemplate.EmailAddress != null)
            {
                return SendEmailCustomerUserExpiry(emailTemplate);
            }

            return false;
        }

        public bool SendEmailCustomerUserExpiry(EmailSendUserExpire emailTemplate)
        {
            try
            {
                var smtpClient = new SmtpClient();
                var message = new MailMessage();

                if (!string.IsNullOrWhiteSpace(smtpClient.PickupDirectoryLocation) && smtpClient.PickupDirectoryLocation.StartsWith("~"))
                {
                    smtpClient.PickupDirectoryLocation = HostingEnvironment.MapPath(smtpClient.PickupDirectoryLocation);
                    if (!Directory.Exists(smtpClient.PickupDirectoryLocation))
                        Directory.CreateDirectory(smtpClient.PickupDirectoryLocation);
                }

                #region Send Email

                //Sending Email
                message.To.Add(emailTemplate.EmailAddress);
                message.Subject = "User Expiry Notification";
                var path = ConfigurationManager.AppSettings["EmailImagePath"];

                var headerPath = emailTemplate.HeaderUrl + "Content/images/topBg.jpg";
                var footerPath = emailTemplate.HeaderUrl + "Content/images/logoBg.jpg";
                var userProfilepath = emailTemplate.HeaderUrl + "Content/images/manageUsers.png";

                // user list table

                string Table = "<table style='border: 1px solid black; border-collapse: collapse; aline:center;'>";
                //add header row
                Table += "<tr style='border: 1px solid black; background:gray;'>";
                Table += "<td style='border: 1px solid black;'>Sr No</td><td style='border: 1px solid black;'>User Name</td><td style='border: 1px solid black;'> Email Address</td>";
                Table += "</tr>";
                int i = 1;
                foreach (var expUser in emailTemplate.UserList)
                {
                    Table += "<tr style='border: 1px solid black;'>";
                    Table += "<td style='border: 1px solid black;'>" + i + "</td><td style='border: 1px solid black;'>" + expUser.FirstName + "</td><td style='border: 1px solid black;'>" + expUser.EmailAddress + "</td>";
                    Table += "</tr>";
                    i++;
                }
                Table += "</table>";

                //-------------------end----------------

                string a = WebUtility.HtmlEncode("<img src='" + headerPath + "' width='100%' height='35px' /><br /> <br /><p>Dear " + emailTemplate.userAdminName + " ,</p><p>Please note that the below accounts have been automatically locked as per your settings. </p><div>" + Table + "</div><p>Should you need to unlock any of these accounts you can do so  by editing the users account status. </p><p style='margin-bottom: 10px;'>Regards</p> <p>The ONRAMP Team</p><br /><img src='" + footerPath + "'  width='100%' height='35px' style=''/>");

                string b = WebUtility.HtmlDecode(a);
                message.Body = b;
                message.IsBodyHtml = true;
                smtpClient.Send(message);

                #endregion Send Email
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        #endregion SendEmailCustomerExpire

        public void Send(params INotification[] notifications)
        {
            //foreach (var notification in notifications)
            //{
            //    if (notification.Type == NotificationType.TestAssignmentNotification ||
            //            notification.Type == NotificationType.TrainingGuideAssignmentNotification)
            //    {
            //        var smsRecipients = new List<SMSRecipient>();
            //        if (notification.Recipients != null)
            //        {
            //            foreach (var recipient in notification.Recipients)
            //            {
            //                // send off emails
            //                if (!string.IsNullOrEmpty(notification.Title))
            //                {
            //                    SendNotificationEmail(notification.Type, recipient, notification.Reference,
            //                        notification.Id, notification.Title);
            //                    smsRecipients.Add(new SMSRecipient
            //                    {
            //                        ReferenceNumber = notification.Title,
            //                        User = recipient
            //                    });
            //                }
            //                else
            //                {
            //                    SendNotificationEmail(notification.Type, recipient, notification.Reference,
            //                        notification.Id);
            //                    smsRecipients.Add(new SMSRecipient
            //                    {
            //                        ReferenceNumber = notification.Reference,
            //                        User = recipient
            //                    });
            //                }
            //            }
            //        }
            //        if (notification.StandardUserRecipients != null)
            //        {
            //            foreach (var recipient in notification.StandardUserRecipients)
            //            {
            //                // send off emails
            //                if (!string.IsNullOrEmpty(notification.Title))
            //                {
            //                    SendNotificationEmail(notification.Type, recipient, notification.Reference,
            //                        notification.Id, notification.Title);
            //                    smsRecipients.Add(new SMSRecipient
            //                    {
            //                        ReferenceNumber = notification.Title,
            //                        StandardUser = recipient
            //                    });
            //                }
            //                else
            //                {
            //                    SendNotificationEmail(notification.Type, recipient, notification.Reference,
            //                        notification.Id);
            //                    smsRecipients.Add(new SMSRecipient
            //                    {
            //                        ReferenceNumber = notification.Reference,
            //                        StandardUser = recipient
            //                    });
            //                }
            //            }
            //        }
            //        // send off smses
            //        if (smsRecipients.Any())
            //            new SMSService(smsRecipients, notification.Type, _commandDispatcher).Send();
            //    }
            //}
        }

        //public bool SendNotificationEmail(NotificationType type, User user, string referenceId, Guid Id, string title = null)
        //{
        //    AddUserCorrespondenceCommand command;
        //    try
        //    {
        //        var smtpClient = new SmtpClient();
        //        var message = new MailMessage();

        //        message.To.Add(user.EmailAddress);

        //        switch (type)
        //        {
        //            case NotificationType.TestAssignmentNotification:
        //                message.Subject = "Test Notification";
        //                break;

        //            case NotificationType.TrainingGuideAssignmentNotification:
        //                message.Subject = "Playbook Notification";
        //                break;

        //            default:
        //                throw new InvalidEnumArgumentException();
        //        }

        //        message.IsBodyHtml = true;
        //        string viewString;
        //        if (!string.IsNullOrEmpty(title))
        //            viewString = GenerateViewString(type, user, referenceId, Id, title);
        //        else
        //        {
        //            viewString = GenerateViewString(type, user, referenceId, Id);
        //        }
        //        AlternateView messageBody = AlternateView.CreateAlternateViewFromString(viewString,
        //            null,
        //            "text/html");

        //        message.AlternateViews.Add(messageBody);
        //        smtpClient.EnableSsl = false;

        //        smtpClient.Send(message);
        //        command = new AddUserCorrespondenceCommand
        //        {
        //            CurrentUserId = user.Id,
        //            CorrespondenceDescription = GetCorrespondenceDescription(type) + " Reference No: " + referenceId,
        //            CorrespondenceDate = DateTime.Now,
        //            CorrespondenceType = UserCorrespondenceEnum.Email,
        //            Content = viewString
        //        };

        //        _commandDispatcher.Dispatch(command);
        //    }
        //    catch (Exception)
        //    {
        //        command = new AddUserCorrespondenceCommand
        //        {
        //            CurrentUserId = user.Id,
        //            CorrespondenceDescription = "[FAILED] " + GetCorrespondenceDescription(type) + " Reference No: " + referenceId,
        //            CorrespondenceDate = DateTime.Now,
        //            CorrespondenceType = UserCorrespondenceEnum.Email
        //        };

        //        _commandDispatcher.Dispatch(command);
        //        return false;
        //    }

        //    return true;
        //}

        //public bool SendNotificationEmail(NotificationType type, StandardUser user, string referenceId, Guid Id, string title = null)
        //{
        //    AddUserCorrespondenceCommand command;
        //    try
        //    {
        //        var smtpClient = new SmtpClient();
        //        var message = new MailMessage();

        //        message.To.Add(user.EmailAddress);

        //        switch (type)
        //        {
        //            case NotificationType.TestAssignmentNotification:
        //                message.Subject = "Test Notification";
        //                break;

        //            case NotificationType.TrainingGuideAssignmentNotification:
        //                message.Subject = "Playbook Notification";
        //                break;

        //            default:
        //                throw new InvalidEnumArgumentException();
        //        }

        //        message.IsBodyHtml = true;
        //        string viewString;
        //        if (!string.IsNullOrEmpty(title))
        //            viewString = GenerateViewString(type, user, referenceId, Id, title);
        //        else
        //        {
        //            viewString = GenerateViewString(type, user, referenceId, Id);
        //        }
        //        AlternateView messageBody = AlternateView.CreateAlternateViewFromString(viewString,
        //            null,
        //            "text/html");

        //        message.AlternateViews.Add(messageBody);
        //        smtpClient.EnableSsl = false;

        //        smtpClient.Send(message);
        //        command = new AddUserCorrespondenceCommand
        //        {
        //            CurrentUserId = user.Id,
        //            CorrespondenceDescription = GetCorrespondenceDescription(type) + " Reference No: " + referenceId,
        //            CorrespondenceDate = DateTime.Now,
        //            CorrespondenceType = UserCorrespondenceEnum.Email,
        //            Content = viewString
        //        };

        //        _commandDispatcher.Dispatch(command);
        //    }
        //    catch (Exception)
        //    {
        //        command = new AddUserCorrespondenceCommand
        //        {
        //            CurrentUserId = user.Id,
        //            CorrespondenceDescription = "[FAILED] " + GetCorrespondenceDescription(type) + " Reference No: " + referenceId,
        //            CorrespondenceDate = DateTime.Now,
        //            CorrespondenceType = UserCorrespondenceEnum.Email
        //        };

        //        _commandDispatcher.Dispatch(command);
        //        return false;
        //    }

        //    return true;
        //}

        //private string GetCorrespondenceDescription(NotificationType type)
        //{
        //    string description = "";
        //    switch (type)
        //    {
        //        case NotificationType.TestAssignmentNotification:
        //            description = "[EMAIL] Test Assignment Notification Email";
        //            break;

        //        case NotificationType.TrainingGuideAssignmentNotification:
        //            description = "[EMAIL] Playbook Assignment Notification Email";
        //            break;

        //        default:
        //            throw new InvalidEnumArgumentException();
        //    }

        //    return description;
        //}

        //private string GenerateViewString(NotificationType type, User user, string referenceID, Guid id, string title = null)
        //{
        //    var viewPath = "";
        //    AssignmentNotificationViewModel model;
        //    if (!string.IsNullOrEmpty(title))
        //    {
        //        model = new AssignmentNotificationViewModel
        //        {
        //            CustomerUserFirstName = user.FirstName,
        //            ReferenceId = referenceID,
        //            Title = title,
        //            Url = user.Company.LayerSubDomain + ".onramp.training",
        //            ImageUrl = user.Company.LogoImageUrl,
        //            UserID = user.Id,
        //            Id = id
        //        };
        //    }
        //    else
        //    {
        //        model = new AssignmentNotificationViewModel
        //        {
        //            CustomerUserFirstName = user.FirstName,
        //            ReferenceId = referenceID,
        //            Url = user.Company.LayerSubDomain + ".onramp.training",
        //            ImageUrl = user.Company.LogoImageUrl,
        //            UserID = user.Id,
        //            Id = id
        //        };
        //    }

        //    switch (type)
        //    {
        //        case NotificationType.TestAssignmentNotification:
        //            viewPath = ConfigurationManager.AppSettings["TestAssignmentNotificationViewPath"];
        //            break;

        //        case NotificationType.TrainingGuideAssignmentNotification:
        //            viewPath = ConfigurationManager.AppSettings["TrainingGuideAssignmentNotificationViewPath"];
        //            break;

        //        default:
        //            throw new InvalidEnumArgumentException();
        //    }

        //    return ViewToStringConverter.Convert(viewPath, model);
        //}

        //private string GenerateViewString(NotificationType type, StandardUser user, string referenceID, Guid id, string title = null)
        //{
        //    var company =
        //        new QueryExecutor().Execute<ProvisionalCompanyQueryParameter, CompanyViewModelLong>(
        //            new ProvisionalCompanyQueryParameter()
        //            {
        //                Id = user.CompanyId
        //            });
        //    var viewPath = "";
        //    AssignmentNotificationViewModel model;
        //    if (!string.IsNullOrEmpty(title))
        //    {
        //        model = new AssignmentNotificationViewModel
        //        {
        //            CustomerUserFirstName = user.FirstName,
        //            ReferenceId = referenceID,
        //            Title = title,
        //            Url = company.CompanyViewModel.LayerSubDomain + ".onramp.training",
        //            ImageUrl = company.CompanyViewModel.LogoImageUrl,
        //            UserID = user.Id,
        //            Id = id
        //        };
        //    }
        //    else
        //    {
        //        model = new AssignmentNotificationViewModel
        //        {
        //            CustomerUserFirstName = user.FirstName,
        //            ReferenceId = referenceID,
        //            Url = company.CompanyViewModel.LayerSubDomain + ".onramp.training",
        //            ImageUrl = company.CompanyViewModel.LogoImageUrl,
        //            UserID = user.Id,
        //            Id = id
        //        };
        //    }

        //    switch (type)
        //    {
        //        case NotificationType.TestAssignmentNotification:
        //            viewPath = ConfigurationManager.AppSettings["TestAssignmentNotificationViewPath"];
        //            break;

        //        case NotificationType.TrainingGuideAssignmentNotification:
        //            viewPath = ConfigurationManager.AppSettings["TrainingGuideAssignmentNotificationViewPath"];
        //            break;

        //        default:
        //            throw new InvalidEnumArgumentException();
        //    }

        //    return ViewToStringConverter.Convert(viewPath, model);
        //}

        public class Notification : INotification
        {
            public Notification(NotificationType type, string reference, Guid testID, params StandardUser[] recipients)
            {
                Type = type;
                Reference = reference;
                StandardUserRecipients = recipients;
                Id = testID;
            }

            public Notification(NotificationType type, string reference, Guid testID, params User[] recipients)
            {
                Type = type;
                Reference = reference;
                Recipients = recipients;
                Id = testID;
            }

            public Notification(NotificationType type, string reference, params User[] recipients)
            {
                Type = type;
                Recipients = recipients;
                Reference = reference;
            }

            public Notification(NotificationType type, string reference, string title, Guid testID, params User[] recepients)
            {
                Type = type;
                Reference = reference;
                Title = title;
                Recipients = recepients;
                Id = testID;
            }

            public Notification(NotificationType type, string reference, string title, Guid testID, params StandardUser[] recepients)
            {
                Type = type;
                Reference = reference;
                Title = title;
                StandardUserRecipients = recepients;
                Id = testID;
            }

            public NotificationType Type { get; private set; }
            public IEnumerable<User> Recipients { get; private set; }
            public IEnumerable<StandardUser> StandardUserRecipients { get; private set; }
            public string Reference { get; private set; }
            public string Title { get; private set; }
            public Guid Id { get; private set; }
        }
    }
}