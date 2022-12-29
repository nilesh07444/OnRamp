using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Web.Mvc;
using Common.Report;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.Query.Test;
using VirtuaCon;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Code.ActionFilters;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers {
	[PortalContextActionFilter]
    public class TestReportController : ExportController<TestExportReportQuery>
    {
        [System.Web.Mvc.AllowAnonymous]
        public override ActionResult DownloadPDF([FromUri] TestExportReportQuery query)
        {
            if (!string.IsNullOrEmpty(query.CompanyId))
                PortalContext.Override(query.CompanyId.AsGuid());
			query.AddOnrampBranding = true;
            query.PortalContext = PortalContext.Current;

			var memoryStream = new MemoryStream();
			var zipStream = new ZipOutputStream(memoryStream);
			zipStream.SetLevel(9);

			var export = ExecuteQuery<TestExportReportQuery, IExportModel>(query);
			export.Title = export.Title.Replace("/", " ") + ".pdf";

			var stream = new MemoryStream();
			IReportDocumentWriter pdfPublisher = new PdfReportPublisher();
			pdfPublisher.Write(export.Document, stream);
			stream.Position = 0;
			SaveFile(zipStream, export.Title, stream.ToArray());

			zipStream.IsStreamOwner = false;
			zipStream.Close();
			memoryStream.Position = 0;
			var fileName = "TestTranscript.zip";
			Response.AddHeader("filename", fileName);
			return File(memoryStream, "application/pdf", fileName);
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

		public override ActionResult Zip([FromUri] TestExportReportQuery query)
        {
            throw new NotImplementedException();
        }
    }
}