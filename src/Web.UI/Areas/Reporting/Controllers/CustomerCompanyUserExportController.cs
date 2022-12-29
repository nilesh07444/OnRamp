using Ramp.Contracts.QueryParameter.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers
{
    public class CustomerCompanyUserExportController : ExportController<CustomerCompanyUserExportQuery>
    {
        public override ActionResult DownloadEXCEL([FromUri] CustomerCompanyUserExportQuery query)
        {
            PortalContext.Override(query.CompanyId);
            query.PortalContext = PortalContext.Current;
            query.AddOnrampBranding = false;
            return base.DownloadEXCEL(query);
        }
        public override ActionResult DownloadPDF([FromUri] CustomerCompanyUserExportQuery query)
        {
            throw new NotImplementedException();
        }

        public override ActionResult Zip([FromUri] CustomerCompanyUserExportQuery query)
        {
            throw new NotImplementedException();
        }
    }
}