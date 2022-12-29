using Quartz;
using Ramp.Contracts.Query.Report;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Web.UI.Helpers;

namespace Web.UI.Code.Jobs
{
    public class NotifyReportSchedular : IJob
    {
        private string SchedularOccurence = string.Empty;

        public async void Execute(IJobExecutionContext context)
        {
             SchedularOccurence = context.JobDetail.Key.Name;
            ReportParam reportParam = new ReportParam();
            if (SchedularOccurence == "Daily")
            {
                reportParam.AssignDateTime(DateTime.Now.Date, DateTime.Now.Date);
                
            }
            else if (SchedularOccurence == "Monthly")
            {
                reportParam.AssignDateTime(ExtensionMethods.FirstDayOfMonth_NewMethod(DateTime.Now.Date), ExtensionMethods.LastDayOfMonth_NewMethod(DateTime.Now.Date));           
            }


            if (MvcApplication.RootPath == null)
                return;
            await callFeedBackReport();
            await InteractionReport();
            await PointStatementReport();
            await UserActivityAndPerformanceUserListReport();
            await ActivityBookSubmissionReport();
            await ExportVirtualMeetingReport();
            await ExportAuditReport();
            await TrainingActivitylogReport();

            //   await ExportActivitybookSubmissionReport();



        }

        public async Task UserActivityAndPerformanceUserListReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/UserActivityAndPerformanceReport/ScheduleUserActivity?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task PointStatementReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/PointsStatement/DownloadEXCELZip/SchedulePointStatement?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task TrainingActivitylogReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/TrainingActivitylogReport/DownloadExcelLog?Occurance=" + SchedularOccurence;

            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task ExportAuditReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/DocumentAudit/ExportDocumentAuditPass?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task ExportActivitybookSubmissionReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/ActivitybookSubmissionReport/DownloadSummaryEXCELPass?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task ExportVirtualMeetingReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/MeetingReport/ExportMeetingPass?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task ActivityBookSubmissionReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/ActivitybookSubmissionReport/DownloadSummaryEXCELPass?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task InteractionReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/InteractionReport/DownloadExcelLog?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }
        public async Task callFeedBackReport()
        {
            var path = "https://boxer.qa.onramp.training/Reporting/Feedback/DownloadEXCELPass?Occurance=" + SchedularOccurence;
            await Task.Run(() =>
            {
                CallWebMethods(path);
            });
        }

        private void CallWebMethods(string url)
        {
            var path = url;

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();
            response.Close();
        }







    }
}
