using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Ramp.Contracts.QueryParameter.Reporting;
using Ramp.Contracts.ViewModel;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers
{
    public class IndividualDevelopmentRecordReportController : ReportController<IndividualDevelopementRecordReportQuery, IndividualDevelopmentReportModel>
    {
        public override ActionResult Zip([FromUri] IndividualDevelopementRecordReportQuery query)
        {
            throw new NotImplementedException();
        }
    }
}