using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.Serialization;
using System.Text;
using System.IO;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.Helpers
{
    public class SendEmail
    {
        private string _emailAdd = ConfigurationManager.AppSettings["emailAdd"];
        private string _smtpFrom = ConfigurationManager.AppSettings["SMTPFrom"];
        private string _smtpLogin = ConfigurationManager.AppSettings["SMTPLogin"];

        public void SendMessage(
            List<string> emailTo,       // 1
            List<string> nameTo,        // 2
            List<string> emailCcTo,     // 3
            List<string> nameCcTo,      // 4
            List<string> emailBccTo,    // 5
            List<string> nameBccTo,     // 6
            string nameReplyTo,         // 7
            string emailReplyTo,        // 8
            string subject,             // 9
            string body,                // 10
            List<string> attachments,   // 11
            string from)                // 12
        {
            MailMessage mail = null;
            try
            {
                mail = new MailMessage();
                foreach (var t in emailTo.Where(t => !string.IsNullOrEmpty(t)))
                {
                    mail.To.Add(new MailAddress(t));
                }

                if (emailCcTo != null && emailCcTo.Count > 0)
                {
                    foreach (var t in emailCcTo.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        mail.CC.Add(new MailAddress(t));
                    }
                }

                if (emailBccTo != null && emailBccTo.Count > 0)
                {
                    foreach (var t in emailBccTo.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        mail.Bcc.Add(new MailAddress(t));
                    }
                }
                if (nameReplyTo != null && emailReplyTo != null)
                {
                    mail.ReplyToList.Add(new MailAddress(emailReplyTo, nameReplyTo));
                }
                if (mail.To.Any())
                {
                    mail.From = new MailAddress(from, _smtpFrom);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    var smtpserver = ConfigurationManager.AppSettings["SMTPServer"];
                    var smtpport = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                    var smtplogin = _smtpLogin;

                    var smtppass = ConfigurationManager.AppSettings["SMTPPassword"];
                    var smtp = new SmtpClient(smtpserver, smtpport)
                    {
                        Credentials = new NetworkCredential(smtplogin, smtppass)
                    };


                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                    smtp.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error sending email", ex);
            }
            finally
            {
                if ((mail != null)) mail.Dispose();
            }
        }


        public void SendMessageWithAttachment(
            List<string> emailTo,       // 1
            List<string> nameTo,        // 2
            List<string> emailCcTo,     // 3
            List<string> nameCcTo,      // 4
            List<string> emailBccTo,    // 5
            List<string> nameBccTo,     // 6
            string nameReplyTo,         // 7
            string emailReplyTo,        // 8
            string subject,             // 9
            string body,                // 10
            List<string> attachments,   // 11
            string from, VirtualClassModel model, string domain)                // 12
        {
            MailMessage mail = null;
            try
            {
                mail = new MailMessage();
                foreach (var t in emailTo.Where(t => !string.IsNullOrEmpty(t)))
                {
                    mail.To.Add(new MailAddress(t));
                }

                if (emailCcTo != null && emailCcTo.Count > 0)
                {
                    foreach (var t in emailCcTo.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        mail.CC.Add(new MailAddress(t));
                    }
                }

                if (emailBccTo != null && emailBccTo.Count > 0)
                {
                    foreach (var t in emailBccTo.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        mail.Bcc.Add(new MailAddress(t));
                    }
                }
                if (nameReplyTo != null && emailReplyTo != null)
                {
                    mail.ReplyToList.Add(new MailAddress(emailReplyTo, nameReplyTo));
                }
                if (mail.To.Any())
                {
                    mail.From = new MailAddress(from, _smtpFrom);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    var smtpserver = ConfigurationManager.AppSettings["SMTPServer"];
                    var smtpport = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                    var smtplogin = _smtpLogin;

                    var smtppass = ConfigurationManager.AppSettings["SMTPPassword"];
                    var smtp = new SmtpClient(smtpserver, smtpport)
                    {
                        Credentials = new NetworkCredential(smtplogin, smtppass)
                    };


                    #region ICal
                    var calendar = new Calendar();

                    calendar.Events.Add(new Event
                    {

                        Class = "PUBLIC",

                        Summary = model.VirtualClassRoomName,

                        Created = new CalDateTime(DateTime.Now),

                        Description = model.Description + Environment.NewLine + Environment.NewLine +
                        "Meeting Details:" + Environment.NewLine + Environment.NewLine +
                        "Virtual Meeting Name:" + model.VirtualClassRoomName + Environment.NewLine +
                        "Start Date/Time:" + model.StartDate + Environment.NewLine +
                        "End Date/Time:" + model.EndDate + Environment.NewLine + Environment.NewLine +

                         "Please visit " + model.JoinMeetingLink + "  to login",

                        Start = new CalDateTime(DateTime.Parse(model.StartDate)),

                        End = new CalDateTime(DateTime.Parse(model.EndDate)),

                        Sequence = 0,

                        Uid = Guid.NewGuid().ToString(),

                        Location = domain,

                    });
                    var serializer = new CalendarSerializer(new SerializationContext());

                    var serializedCalendar = serializer.SerializeToString(calendar);

                    var bytesCalendar = Encoding.UTF8.GetBytes(serializedCalendar);

                    MemoryStream ms = new MemoryStream(bytesCalendar);

                    System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(ms, "event.ics", "text/calendar");

                    mail.Attachments.Add(attachment);

                    #endregion


                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(mail);
                    smtp.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error sending email", ex);
            }
            finally
            {
                if ((mail != null)) mail.Dispose();
            }
        }

        public void addAttachmentSendEmail(MemoryStream stream,string Recepients, string Title, string mediaType,string filepaths= null)
        {
            //var writer = new StreamWriter(stream);
            //writer.Flush();
            //stream.Position = 0;
            //var msg = new SendEmail();
            List<string> strRecepients = new List<string>();
            var recepientarray = Recepients.Split(',');
            foreach(var re in recepientarray)
            {
                strRecepients.Add(re);
            }
            SendMessageWithAttachment(strRecepients, null, null, null, null, "Report Submission", "Please see the report attached", null,
                ConfigurationManager.AppSettings["SMTPFrom"].ToString(), stream, Title, mediaType,filepaths);
        }

        public void SendMessageWithAttachment(
                          List<string> emailTo,
                          List<string> emailCcTo,
                          List<string> emailBccTo,
                          string nameReplyTo,
                          string emailReplyTo,
                          string subject,
                          string body,
                          List<string> attachments,
                          string from, MemoryStream memory = null, string filename = null, string mediaType = null,string filepaths= null)
        {
            MailMessage mail = null;
            try
            {
                mail = new MailMessage();
                foreach (var t in emailTo.Where(t => !string.IsNullOrEmpty(t)))
                {
                    mail.To.Add(new MailAddress(t));
                }

                if (emailCcTo != null && emailCcTo.Count > 0)
                {
                    foreach (var t in emailCcTo.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        mail.CC.Add(new MailAddress(t));
                    }
                }

                if (emailBccTo != null && emailBccTo.Count > 0)
                {
                    foreach (var t in emailBccTo.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        mail.Bcc.Add(new MailAddress(t));
                    }
                }
                if (nameReplyTo != null && emailReplyTo != null)
                {
                    mail.ReplyToList.Add(new MailAddress(emailReplyTo, nameReplyTo));
                }
                if (mail.To.Any())
                {
                    mail.From = new MailAddress(from, _smtpFrom);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    var smtpserver = ConfigurationManager.AppSettings["SMTPServer"];
                    var smtpport = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                    var smtplogin = _smtpLogin;

                    var smtppass = ConfigurationManager.AppSettings["SMTPPassword"];
                    var smtp = new SmtpClient(smtpserver, smtpport)
                    {
                        Credentials = new NetworkCredential(smtplogin, smtppass)
                    };
                    if (filepaths != null && filename != null && mediaType != null)
                    {
                        mail.Attachments.Add(new System.Net.Mail.Attachment(filepaths));
                        //   
                    }
                    else if (memory != null && memory.Length > 0)
                    {
                        mail.Attachments.Add(new System.Net.Mail.Attachment(memory, filename, mediaType));
                    }

                    //smtp.EnableSsl = true;
                    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //smtp.UseDefaultCredentials = true;
                    smtp.Timeout = 200000000;
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                      smtp.Send(mail);
                    smtp.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error sending email", ex);
            }
            finally
            {
                if ((mail != null)) mail.Dispose();
            }


           
        }

    }
}
