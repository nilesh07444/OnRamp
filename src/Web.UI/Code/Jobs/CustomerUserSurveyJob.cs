using System.Configuration;
using System.IO;
using System.Net;
using Quartz;

namespace Web.UI.Code.Jobs
{
    public class CustomerUserSurveyJob : IJob
    {
        #region IJob Members

        public void Execute(IJobExecutionContext context)
        {
            if (MvcApplication.RootPath == null)
                return;

            var path = MvcApplication.RootPath + "CustomerManagement/CustomerMgmt/CustomerUserSurvey";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();
            response.Close();
        }
        #endregion
    }
}