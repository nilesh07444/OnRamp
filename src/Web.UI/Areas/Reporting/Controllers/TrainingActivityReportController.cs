using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Common.Report;
using Ramp.Services.QueryHandler;
using Web.UI.Code.ActionFilters;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers
{
    [PortalContextActionFilter]
    public class TrainingActivityReportController : ReportController<TrainingActivityListQuery, TrainingActivityReportModel>
    {
        void RewriteUrls(UploadResultViewModel x)
        {
            x.Url = Url.ActionLink<UploadController>(a => a.Get(x.Id.ToString(), false)).Replace("Reporting/", string.Empty);
            x.DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(x.Id.ToString(), null)).Replace("Reporting/", string.Empty);
            x.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(x.Id.ToString(), 300, 300)).Replace("Reporting/", string.Empty);
        }
        protected override void PostProcess(TrainingActivityReportModel data)
        {
            foreach (var r in data.FilteredResults)
            {
                r.Documents.ToList().ForEach(RewriteUrls);
                switch (r.TrainingActivityType)
                {
                    case TrainingActivityType.Bursary:
                        r.BursaryTrainingActivityDetail.Invoices.ToList().ForEach(RewriteUrls);
                        break;
                    case TrainingActivityType.External:
                        r.ExternalTrainingActivityDetail.Invoices.ToList().ForEach(RewriteUrls);
                        break;
                    default:
                        break;
                }
            }
        }

        public ActionResult ExportEXCEL([FromUri] Guid? id)
        {
            return RedirectToAction("DownloadEXCEL", new {Id = id});
        }
        public ActionResult ExportPDF([FromUri]Guid? id)
        {
            return RedirectToAction("DownloadPDF", new { Id = id });
        }

        public override ActionResult Zip([FromUri] TrainingActivityListQuery query)
        {
            throw new NotImplementedException();
        }
    }
}