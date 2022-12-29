using Quartz;
using System.IO;
using System.Net;

namespace Web.UI.Code.Jobs
{
    public class SendEmails : IJob
    {

        #region IJob Members

        public void Execute(IJobExecutionContext context)
        {
            if (MvcApplication.RootPath == null)
                return;

            var path = MvcApplication.RootPath + "Send/sendEmails?sendTo=pershanthenm@onramp.co.za";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();
            response.Close();
        }
    }

    #endregion
}