using Quartz;
using System.IO;
using System.Net;

namespace Web.UI.Code.Jobs
{
    public class testreport : IJob
    {

        #region IJob Members

        public void Execute(IJobExecutionContext context)
        {
            if (MvcApplication.RootPath == null)
                return;

            var path = MvcApplication.RootPath + "Reporting/InteractionReport/DownloadEXCEL?groupIds=f4453a03-1f9a-43c2-b6fe-091c651c4e65%2C1276b591-d1ed-4451-8a61-0dcd007df39d%2C7555702e-2dbf-4f71-bb44-16996a808c91%2C82e14eb1-be53-4bef-9ff1-31f5ab546940&categoryIds=16a21b83-5656-44a1-bf48-d987037f1d44%2C3f27f0f0-8880-4bb6-af4c-b547fb10c0e2%2Cf6b76b5e-ad81-4b85-9e6b-597b16a35d83&documentTypes=9%2C6%2C4%2C3&fromDate=2022-05-06T14%3A02%3A17.980Z&toDate=2022-05-06T14%3A02%3A17.980Z&ScheduleName=Interaction&Recepients=pershanthenm%40onramp.co.za";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();
            response.Close();
        }
    }

    #endregion
}