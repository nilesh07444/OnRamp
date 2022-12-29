using Common.Enums;
using Common.Query;
using Common.Report;
using Domain.Customer;
using Domain.Customer.Models.ScheduleReport;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentAudit;
using Ramp.Contracts.Query.Report;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers {
	public class DocumentAuditController : RampController {
		// GET: Reporting/DocumentAudit
		public ActionResult Index()
		{
			var model = new DocumentAuditReportModel();

			var documentType = PortalContext.Current.UserCompany.EnableChecklistDocument ? EnumUtilityExtensions.GetEnumFriendlyNamesDictionary(typeof(DocumentType)).Select(c => new { Id = c.Value.Replace(" ", ""), Type = c.Value }).ToList() : EnumUtilityExtensions.GetEnumFriendlyNamesDictionary(typeof(DocumentWithoutType)).Select(c => new { Id = c.Value.Replace(" ", ""), Type = c.Value }).ToList();

			model.DocumentType = new SelectList(documentType.Where(c => !c.Id.Contains("Unknown") && !c.Id.Contains("VirtualClassRoom") && !c.Id.Contains("Certificate")), "Id", "Type", "--select--");

			var document = new List<DocumentListModel> { new DocumentListModel { Id = "0", Title = "--Select--" } };

			model.DocumentList = new SelectList(document, "Value", "Text");

			return View(model);
		}

		public ActionResult DocumentsByType(string documentTypes) {
			if (string.IsNullOrEmpty(documentTypes)) {
				return new JsonResult { Data = new object[] { new object() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}

			var documentFilters = new List<string>
			{
				"Status:Published",
				"Status:Draft",
				"Status:Recalled"
			};

			foreach (var type in documentTypes.Split(',').ToList()) {
				documentFilters.Add($"Type:{type.ToString()}");
			}

			var documents = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery {
				DocumentFilters = documentFilters,
				EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument
			});

			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public ActionResult GetDocumentAudits(string documents, DateTime? startDate, DateTime? endDate) {
			var audits = ExecuteQuery<DocumentAuditFilterQuery, AuditReportModel>(new DocumentAuditFilterQuery() {
				Documents = documents,
				StartDate = startDate,
				EndDate = endDate

			});
			return PartialView("_DocumentAuditList", audits);
		}

		/// <summary>
		/// this is used to export document audit
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>     
		[System.Web.Mvc.HttpGet]
		[AllowAnonymous]
		public ActionResult ExportDocumentAudit(DocumentAuditFilterQuery query)
		{
			query.PortalContext = PortalContext.Current;
			var model = ExecuteQuery<DocumentAuditFilterQuery, IExportModel>(query);
			var stream = new MemoryStream();
			IReportDocumentWriter publisher = new ExcelReportPublisher();
			var title = "DocumentAudits.xls";
			publisher.Write(model.Document, stream);
			stream.Position = 0;
			string filePaths = null;
			var t = stream.ToArray();
			if (query.PortalContext != null)
			{
				Response.AddHeader("filename", title);
				filePaths = Server.MapPath(Path.Combine("~/Download/", title));

				System.IO.File.WriteAllBytes(filePaths, stream.ToArray());
			}
			new Ramp.Services.Helpers.SendEmail().addAttachmentSendEmail(stream, query.Recepients, title, "application/octet-stream", filePaths);
			if (!string.IsNullOrEmpty(filePaths))
			{
				FileInfo file = new FileInfo(filePaths);
				if (file.Exists)
				{
					file.Delete();
				}
			}
			return new FileStreamResult(stream, "application/octet-stream");

		}



		[HttpGet]
		[AllowAnonymous]
		public void ExportDocumentAuditPass(string Occurance)
		{
			var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "10").ToList();
			foreach (var d in getAllReport)
			{
				var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
				{
					Id = d.Id
				});

				DocumentAuditFilterQuery query = new DocumentAuditFilterQuery
				{
					DocumentList = ReportParam.ReturnParams(data.Params, "DocumentType"),
					ScheduleName = data.ScheduleName,
					Recepients = data.RecipientsList,
					StartDate = ReportParam.FromDate,
					EndDate= ReportParam.ToDate,
				};

				ExportDocumentAudit(query);
			}
		}

	 
	}
}