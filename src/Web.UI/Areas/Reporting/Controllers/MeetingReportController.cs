using Common.Query;
using Common.Report;
using Domain.Customer;
using Domain.Customer.Models.ScheduleReport;
using Ramp.Contracts.Query.Report;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Controllers;


namespace Web.UI.Areas.Reporting.Controllers
{
    public class MeetingReportController : RampController
    {
        // GET: Reporting/MeetingReport
        public ActionResult Index()
        {
            var model = new MeetingReportModel();

            var meetings = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery()).ToList();

            model.MeetingRoom = new SelectList(meetings, "Id", "VirtualClassRoomName", "--select--");

            return View(model);
        }
        [HttpPost]
        public ActionResult GetMeeting(string meetingIds, string status, DateTime? startDate, DateTime? endDate)
        {
            var meetings = ExecuteQuery<MeetingReportQuery, IEnumerable<MeetingReportUsageModel>>(new MeetingReportQuery()
            {
                MeetingIds = meetingIds,
                Status = status,
                StartDate = startDate,
                EndDate = endDate

            }).ToList();
            return PartialView("_MeetingReport", meetings);
        }

        [HttpGet]
        [AllowAnonymous]
        public void ExportMeetingPass(string Occurance)
        {
            var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "9").ToList();
            foreach (var d in getAllReport)
            {
                var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
                {
                    Id = d.Id
                });

                DocumentType[] documentsType = null;
                ReportParam.ReturnDocumentType(data.Params, out documentsType);
                MeetingReportQuery query = new MeetingReportQuery
                {
                    MeetingIds = ReportParam.ReturnParams(data.Params, "MeetingRoom").FirstOrDefault(),
                    ToggleFilter = "User Name,Attendance,Date Joined",
                    ScheduleName = data.ScheduleName,
                    Recepients = data.RecipientsList,
                    StartDate = ReportParam.FromDate,
                    EndDate= ReportParam.ToDate,
                };

                ExportMeeting(query);
            }
        }

        /// <summary>
        /// this is used to export meeting
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>     
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ExportMeeting(MeetingReportQuery query)
        {
            query.PortalContext = PortalContext.Current;
            var model = ExecuteQuery<MeetingReportQuery, IExportModel>(query);
            var stream = new MemoryStream();
            string filePaths = null;
            IReportDocumentWriter publisher = new ExcelReportPublisher();
            var title = "Meeting.xls";
            publisher.Write(model.Document, stream);

            stream.Position = 0;

            var t = stream.ToArray();
            if (!string.IsNullOrEmpty(query.Recepients))
            {
                if (query.PortalContext != null)
                {
                    Response.AddHeader("filename", title);
                    filePaths = Server.MapPath(Path.Combine("~/Download/", title));
                    System.IO.File.WriteAllBytes(filePaths, stream.ToArray());
                }

                if (!string.IsNullOrEmpty(query.Recepients))
                {
                    new SendEmail().addAttachmentSendEmail(stream, query.Recepients, title, "application/octet-stream", filePaths);
                    if (!string.IsNullOrEmpty(filePaths))
                    {
                        FileInfo file = new FileInfo(filePaths);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                }
            }
            return new FileStreamResult(stream, "application/octet-stream");

        }

    }
}