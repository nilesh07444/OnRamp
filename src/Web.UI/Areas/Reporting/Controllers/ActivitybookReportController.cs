using Common.Query;
using Common.Report;
using Domain.Customer.Models;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using VirtuaCon;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers {
	public class ActivitybookReportController :ExportController<CheckListExportReportQuery> {

		
		[System.Web.Mvc.HttpGet]
		public ActionResult DownloadPrintPDF([FromUri] CheckListExportReportQuery query) {

			if (!string.IsNullOrEmpty(query.CompanyId))
				PortalContext.Override(query.CompanyId.AsGuid());
			query.AddOnrampBranding = true;
			query.PortalContext = PortalContext.Current;
			query.UserId = query.UserId;
				

			var checkList = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = query.ResultId });

			#region for pdf checklist

			var model = ExecuteQuery<CheckListExportReportQuery, IExportModel>(query);

			model.Title = checkList.Title + ".xls";
			var stream = new MemoryStream();
			stream.Position = 0;
			IReportDocumentWriter publisher = new ExcelReportPublisher();
			publisher.Write(model.Document, stream);
			
			var t = stream.ToArray();

			var memoryStream = new MemoryStream();
			var zipStream = new ZipOutputStream(memoryStream);
			zipStream.SetLevel(3);
			SaveFile(zipStream, model.Title, t);

			#endregion

			var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = Convert.ToString(query.UserId), DocumentId = query.ResultId });
			if (checkList != null && checkList.ContentModels.Any()) {
				var chapters = checkList.ContentModels;

				foreach (var item in chapters) {
					if (item.Attachments.Any()) {
						foreach (var attachment in item.Attachments) {

							ZipFile(attachment.Id, zipStream, attachment.Name);

						}
					}
					var checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id });

					if (checkListChapterUserUpload.Any()) {

						foreach (var attachment in checkListChapterUserUpload) {

							ZipFile(attachment.Id, zipStream, attachment.Name);

						}
					}
				}
			}

			zipStream.IsStreamOwner = false;
			zipStream.Close();
			memoryStream.Position = 0;
			var fileName = checkList.Title + ".zip";
			Response.AddHeader("filename", fileName);
			return new FileStreamResult(memoryStream, "application/octet-stream");
		}
		#region written by ashok to save file in zip and download zip
		public void ZipFile(string id, ZipOutputStream zipStream, string name) {
			var upload = ExecuteQuery<FetchUploadByIdQuery, Upload>(new FetchUploadByIdQuery { Id = id });

			if (upload != null && upload.Data.Any()) {
				SaveFile(zipStream, name,upload.Data);
			}

		}

		public void SaveFile(ZipOutputStream zipStream, string name, byte[] data) {
			using (var stream = new MemoryStream(data)) {
				var attachmentEntry = new ZipEntry(ZipEntry.CleanName(name)) {
					Size = stream.Length
				};
				zipStream.PutNextEntry(attachmentEntry);
				byte[] buffer = new byte[4096];
				int count = stream.Read(buffer, 0, buffer.Length);
				while (count > 0) {
					zipStream.Write(buffer, 0, count);
					count = stream.Read(buffer, 0, buffer.Length);
					if (!Response.IsClientConnected) {
						break;
					}
				}
				zipStream.CloseEntry();
			};
		}
		#endregion


		public override ActionResult Zip([FromUri] CheckListExportReportQuery query) {
			throw new NotImplementedException();
		}
	}
}