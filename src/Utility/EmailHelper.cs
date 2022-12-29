using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.Configuration;
using System.Net;

namespace Utility
{
    /// <summary>
    /// Summary description for EmailHelper
    /// </summary>
    public class EmailHelper
    {
        public EmailHelper()
        {
            //
            // TODO: Add constructor logic here
            //
        }



        public static int SendEmail(string from, List<string> toSend, string subject, string body)
        {


            MailMessage mail;
            //List<Attachment> Attachments=new List<Attachment>();
            //foreach (var file in files)
            //{
            //Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
            //ContentDisposition disposition = data.ContentDisposition;
            //disposition.CreationDate = System.IO.File.GetCreationTime(file);
            //disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            //disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
            //Attachments.Add(data);
            //}

            //  Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);

            foreach (string recepient in toSend) // Loop through List with foreach
            {


                mail = new MailMessage();

                SmtpClient smtpServer = new SmtpClient(ConfigurationManager.AppSettings["SMTPCLIENT"].ToString());
                smtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUsername"].ToString(), ConfigurationManager.AppSettings["SMTPPassword"].ToString());
                smtpServer.Port = int.Parse(ConfigurationManager.AppSettings["SMTPPort"].ToString()); // Gmail works on this port
                //mail.Attachments

                mail.From = new MailAddress("Donotreply.ShopingCart@gmail.com");
                mail.To.Add(recepient);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                smtpServer.EnableSsl = true;

                smtpServer.Send(mail);
            }

            return 1;

        }


        /// <summary>
        /// Sends the Mail
        /// </summary>
        /// <param name="sendto"></param>
        /// <param name="from"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="bodyHTML"></param>
        /// <returns></returns>
        public static bool SendMail(string sendto, string cc, string bcc, string from, string subject, string body,
            bool bodyHTML)
        {
            try
            {
                StringBuilder strSentMail = new StringBuilder();
                MailMessage eMsg = new MailMessage();

                if (!sendto.Equals(""))
                {
                    string[] argsTo = sendto.Split(',');
                    for (int i = 0; i <= argsTo.Length - 1; i++)
                    {
                        eMsg.To.Add(new MailAddress(argsTo[i]));
                        strSentMail.Append(argsTo[i] + "\n");
                    }
                }

                if (!cc.Equals(""))
                {
                    string[] argsCC = cc.Split(',');
                    for (int i = 0; i <= argsCC.Length - 1; i++)
                    {
                        eMsg.To.Add(new MailAddress(argsCC[i]));
                        strSentMail.Append(argsCC[i] + "\n");
                    }
                }

                if (!bcc.Equals(""))
                {
                    string[] argsBCC = bcc.Split(',');
                    for (int i = 0; i <= argsBCC.Length - 1; i++)
                    {
                        eMsg.Bcc.Add(new MailAddress(argsBCC[i]));
                        strSentMail.Append(argsBCC[i] + "\n");
                    }
                }
                eMsg.Subject = subject;
                eMsg.Body = body;
                eMsg.IsBodyHtml = bodyHTML;

                if (string.IsNullOrEmpty(from))
                    eMsg.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUsername"]);
                else
                    eMsg.From = new MailAddress(from);

                SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["SMTPCLIENT"]);
                bool bEnableSSL = false;
                if (ConfigurationManager.AppSettings["EnableSSL"] != null)
                {
                    bEnableSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSSL"].ToString());
                }
                client.EnableSsl = bEnableSSL;
                client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUsername"],
                    ConfigurationManager.AppSettings["SMTPPassword"]);
                client.Send(eMsg);
                return true;
            }
            catch (System.Exception ex)
            {
                Utility.LogManager.Fatal(ex, Utility.LogManager.LoggerFileName.Utility);
                return false;
            }
        }
    }
}