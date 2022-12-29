using ASPSnippets;
using ASPSnippets.SmsAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public  static class SmsHelper
    {
        public static bool SmsSend(string recipientNumbers, string message)
        {
            try
            {
                SMS.APIType = SMSGateway.Site2SMS;

                SMS.MashapeKey = ConfigurationManager.AppSettings["MashapeKey"].ToString();

                SMS.Username = ConfigurationManager.AppSettings["SmsUsername"].ToString();
                SMS.Password = ConfigurationManager.AppSettings["SmsPassword"].ToString();

                if (recipientNumbers.Trim().IndexOf(",") == -1)
                {
                    //Single SMS
                    SMS.SendSms(recipientNumbers.Trim(), message);
                }
                else
                {
                    //Multiple SMS
                    List<string> numbers = recipientNumbers.Trim().Split(',').ToList();
                    SMS.SendSms(numbers, message);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                LogManager.Fatal(ex, Utility.LogManager.LoggerFileName.Utility);
                return false;
            }
        }
    }
}
