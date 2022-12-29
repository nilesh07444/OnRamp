using Ramp.Contracts.QueryParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.UI.Controllers;
using System.Web.Http;
using Ramp.Security.Authorization;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.QueryParameter.TestManagement;
using Common.Report;
using Ramp.Services.Helpers;
using VirtuaCon.Reporting.Publishers;

namespace Web.UI.Areas.Reporting.Controllers
{
    public class TrainingGuideExportController : ExportController<TrainingGuideExportQuery>
    {
        public override ActionResult DownloadPDF([FromUri] TrainingGuideExportQuery query)
        {
            query.UserId = SessionManager.GetCurrentlyLoggedInUserId();
            query.AddOnrampBranding = true;
            query.PortalContext = PortalContext.Current;
            return base.DownloadPDF(query);
        }
        public override ActionResult Zip([FromUri] TrainingGuideExportQuery query)
        {
            query.UserId = SessionManager.GetCurrentlyLoggedInUserId();
            query.PortalContext = PortalContext.Current;
            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(3);

            var model = ExecuteQuery<TrainingGuideExportQuery, IExportModel>(query);
            model.Title = model.Title.RemoveSpecialCharacters() + ".pdf";
            var entry = new ZipEntry(ZipEntry.CleanName(model.Title));
            entry.DateTime = DateTime.Now;
            zipStream.PutNextEntry(entry);
            new PdfReportPublisher().Write(model.Document, zipStream);
            zipStream.CloseEntry();
            var attachments = ExecuteQuery<TrainingGuideExportQuery, IEnumerable<AttachmentModel>>(query);
            attachments.Where(x => x.Data != null).ToList().ForEach(delegate (AttachmentModel attachment)
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