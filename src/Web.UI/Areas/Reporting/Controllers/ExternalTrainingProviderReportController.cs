using Ramp.Contracts.QueryParameter.ExternalTrainingProvider;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Report;
using Ramp.Services.QueryHandler;
using Web.UI.Code.ActionFilters;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;
using System.Web.Http;

namespace Web.UI.Areas.Reporting.Controllers
{
    [PortalContextActionFilter]
    public class ExternalTrainingProviderReportController : ReportController<ExternalTrainingProviderListQuery,ExternalTrainingProviderReportModel>
    {
        void RewriteUrls(FileUploadResultViewModel x)
        {
            x.Url = Url.ActionLink<UploadController>(a => a.Get(x.Id.ToString(), false)).Replace("Reporting/", string.Empty);
            x.DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(x.Id.ToString(), null)).Replace("Reporting/", string.Empty);
            x.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(x.Id.ToString(), 300, 300)).Replace("Reporting/", string.Empty);
        }
        protected override void PostProcess(ExternalTrainingProviderReportModel data)
        {
            foreach (var externalTrainingProviderModel in data.FilteredResults)
            {
                foreach (var beeCertificateModel in externalTrainingProviderModel.BEECertificates)
                {
                    RewriteUrls(beeCertificateModel.Upload);
                }
            }
        }

        public override ActionResult Zip([FromUri] ExternalTrainingProviderListQuery query)
        {
            throw new NotImplementedException();
        }
    }
}