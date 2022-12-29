using Ramp.Contracts.CommandParameter.Feedback;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Common;
using Common.Report;
using Domain.Customer;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.UserFeedback;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Controllers;
using Domain.Customer.Models.ScheduleReport;
using Common.Query;
using Ramp.Contracts.Query.Report;

namespace Web.UI.Areas.Feedback.Controllers
{
    public class FeedbackController : ExportController<UserFeedbackExportQuery>
    {
        [System.Web.Mvc.HttpPost]
        public ActionResult CloseFeedbackModal()
        {
            Session["ShowTestFeedbackModal"] = false;
            Session["FeedbackSent"] = null;
            Session["FeedbackSentInvalid"] = null;
            return new HttpStatusCodeResult(200);
        }


        public ActionResult ContentFeedbackReport()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult ContentFeedbackData(DateTime? fromDate, DateTime? toDate,
            DocumentType[] documentTypes = null, UserFeedbackContentType[] feedbackTypes = null, DocumentIdentifier[] documents = null, string search = "")
        {
            var feedback = ExecuteQuery<FilteredUserFeedbackQuery, IEnumerable<UserFeedbackViewModelShort>>(
                new FilteredUserFeedbackQuery
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    DocumentTypes = documentTypes,
                    FeedbackTypes = feedbackTypes,
                    Documents = documents,
                    Text = search,
					IsGlobalAccess=false
                });

			var feedbackGlobal = ExecuteQuery<FilteredUserFeedbackQuery, IEnumerable<UserFeedbackViewModelShort>>(
				new FilteredUserFeedbackQuery {
					FromDate = fromDate,
					ToDate = toDate,
					DocumentTypes = documentTypes,
					FeedbackTypes = feedbackTypes,
					Documents = documents,
					Text = search,
					IsGlobalAccess = true
				});

			return new JsonResult {Data = new { feedback, feedbackGlobal }, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.AllowAnonymous]
        public void DownloadEXCELPass(string Occurance)
        {
            var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences.ToLower() == Occurance.ToLower() && z.ReportAssignedId == "1").ToList();
            foreach (var d in getAllReport)
            {
                var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
                {
                    Id = d.Id
                });
                DocumentType[] documentsType = null;
                ReportParam.ReturnDocumentType(data.Params, out documentsType);
                UserFeedbackExportQuery query = new UserFeedbackExportQuery
                {
                    FromDate = DateTime.Now  , //ReportParam.FromDate,
                    ToDate = DateTime.Now, //ReportParam.ToDate,
                    DocumentTypes = documentsType,
                    ScheduleName = data.ScheduleName,
                    Recepients = data.RecipientsList
                };
                DownloadEXCEL(query);
            }
        }

        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult DownloadEXCEL([FromBody] UserFeedbackExportQuery query)
        {
            query.PortalContext = PortalContext.Current;
			query.IsGlobalAccess = false;
            return Publish(ExecuteQuery<UserFeedbackExportQuery, IExportModel>(query), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
        }
		[System.Web.Mvc.HttpPost]
		public ActionResult DownloadGlobalEXCEL([FromBody] UserFeedbackExportQuery query) {
			query.PortalContext = PortalContext.Current;
			query.IsGlobalAccess = true;
			return Publish(ExecuteQuery<UserFeedbackExportQuery, IExportModel>(query), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
		}

		[System.Web.Mvc.HttpPost]
        public ActionResult SaveFeedback(SaveFeedbackCommand command)
        {
            command.UserId = SessionManager.GetCurrentlyLoggedInUserId();
            var commandResponse = ExecuteCommand(command);

            if (commandResponse.Validation.Any())
            {
                return new JsonResult()
                {
                    Data = new
                    {
                        success = false,
                        error = commandResponse.ErrorMessage
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return new JsonResult()
            {
                Data = new {success = true},
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult SaveFeedbackMvc(SaveFeedbackCommand command)
        {
            if (ModelState.IsValid)
            {
                SaveFeedback(command);
                Session["FeedbackSent"] = true;
                Session["FeedbackSentInvalid"] = null;
            }
            else
            {
                Session["FeedbackSentInvalid"] = true;
            }

            Response.Redirect(new System.Security.Policy.Url(Request.UrlReferrer.ToString()).Value);
            return null;
        }

        public override ActionResult Zip(UserFeedbackExportQuery query)
        {
            throw new NotImplementedException();
        }
    }
}