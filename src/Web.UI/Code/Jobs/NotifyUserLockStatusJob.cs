using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Web.UI.Code.Jobs
{
    public class NotifyUserLockStatusJob : IJob
    {
        #region IJob Members

        public void Execute(IJobExecutionContext context)
        {
            if (MvcApplication.RootPath == null)
                return;

            var path = $"{MvcApplication.RootPath}CustomerManagement/CustomerMgmt/CustomerUserLockScheduler";


            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();
            response.Close();
        }

        #endregion

    }
}