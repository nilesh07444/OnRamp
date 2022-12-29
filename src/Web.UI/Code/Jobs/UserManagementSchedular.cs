using Common.Command;
using Common.Query;
using Data.EF;
using Data.EF.Customer;
using Quartz;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Web.UI.Code.Jobs
{
    public class UserManagementSchedular : IJob
    {
        public async void Execute(IJobExecutionContext context)
        {
            if (MvcApplication.RootPath == null)
                return;

            await ImportGroups();

            await ImportUsers();


        }

        public async Task ImportGroups()
        {
            string url = "https://boxer.qa.onramp.training/UserManagement/UserMgmt/ImportGroups";
            await Task.Run(() =>
            {
                WRequestobj(url, "GET", null);
            });
        }

        public async Task ImportUsers()
        {
            string url = "https://boxer.qa.onramp.training/UserManagement/UserMgmt/ImportUsers";
            await Task.Run(() =>
            {
                WRequestobj(url, "GET", null);
            });
        }

        public void WRequestobj(string UrlId, string method, string postData)
        {
            string URL = UrlId;
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);

                httpWebRequest.ContentType = "application/json;charset=UTF-8";
                httpWebRequest.Method = method;
                httpWebRequest.KeepAlive = false;
                httpWebRequest.Timeout = 600000;

                if (httpWebRequest.Method == "POST")
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(postData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string results;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    results = streamReader.ReadToEnd();
                }
            }

            catch (WebException ex)
            {
                Console.WriteLine(ex);
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                }
            }

        }

    }
}
