using System.Configuration;
using System.IO;
using System.Net;
using Quartz;

namespace Web.UI.Code.Jobs
{
    public class NotifyTestReportJob : IJob
    {
        #region IJob Members

        public void Execute(IJobExecutionContext context)
        {
            if (MvcApplication.RootPath == null)
                return;

            var path = MvcApplication.RootPath + "ManageTrainingTest/ManageTrainingTest/SendMailToAllAdminsAndAuthorOfTest";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();
            response.Close();
        }

        #endregion
    }
}