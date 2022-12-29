using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Web.UI.Code.Jobs
{
    public class NotifyCompanyLockStatusJob : IJob
    {
        #region IJob Members

        public void Execute(IJobExecutionContext context)
        {
            if (MvcApplication.RootPath == null)
                return;

            var path = MvcApplication.RootPath + "CustomerManagement/CustomerMgmt/CustomerCompanyLockSheduler";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();
            response.Close();
        }

        #endregion
    }
}