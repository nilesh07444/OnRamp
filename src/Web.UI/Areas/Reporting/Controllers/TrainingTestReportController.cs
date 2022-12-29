using Common.Report;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Code.ActionFilters;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers
{
    [PortalContextActionFilter]
    public class TrainingTestReportController : ExportController<TrainingTestExportReportQuery>
    {
        [System.Web.Mvc.AllowAnonymous]
        public override ActionResult DownloadPDF([FromUri] TrainingTestExportReportQuery query)
        {
            if (query.CompanyId.HasValue)
                PortalContext.Override(query.CompanyId);
            query.AddOnrampBranding = true;
            query.PortalContext = PortalContext.Current;
            return base.DownloadPDF(query);
        }
        public override ActionResult Zip([FromUri] TrainingTestExportReportQuery query)
        {
            query.PortalContext = PortalContext.Current;
            query.AddOnrampBranding = true;
            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(3);

            var model = ExecuteQuery<TrainingTestExportReportQuery, IExportModel>(query);
            model.Title = model.Title.RemoveSpecialCharacters() + ".pdf";
            var entry = new ZipEntry(ZipEntry.CleanName(model.Title));
            entry.DateTime = DateTime.Now;
            zipStream.PutNextEntry(entry);
            new PdfReportPublisher().Write(model.Document, zipStream);
            zipStream.CloseEntry();

			var attachments = ExecuteQuery<TrainingTestExportReportQuery, IEnumerable<AttachmentModel>>(query);
            attachments.ToList().ForEach(delegate (AttachmentModel attachment)
            {

                using (var stream = new MemoryStream(attachment.Data))
                {
                    var attachmentEntry = new ZipEntry(ZipEntry.CleanName(attachment.Name));
                    attachmentEntry.Size = stream.Length;
                    zipStream.PutNextEntry(attachmentEntry);
                    byte[] buffer = new byte[4096];
                    int count = stream.Read(buffer, 0, buffer.Length);
                    while (count > 0)
                    {
                        zipStream.Write(buffer, 0, count);
                        count = stream.Read(buffer, 0, buffer.Length);
                        if (!Response.IsClientConnected)
                        {
                            break;
                        }
                    }
                    zipStream.CloseEntry();
                };
            });
            zipStream.IsStreamOwner = false;
            zipStream.Close();
            memoryStream.Position = 0;
            Response.AddHeader("filename", $"{model.Title.Replace(".pdf", "")}.zip");
            return new FileStreamResult(memoryStream, "application/octet-stream");
        }
    }
}