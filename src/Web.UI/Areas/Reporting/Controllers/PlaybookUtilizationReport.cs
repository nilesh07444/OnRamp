using Ramp.Contracts.QueryParameter.Reporting;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.UI.Controllers;
using System.Web.Http;
using System.Web.Mvc;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.CustomerManagement;

namespace Web.UI.Areas.Reporting.Controllers
{
    public class PlaybookUtilizationReportController : ReportController<PlaybookUtilizationReportQuery, PlaybookUtilizationReportModel>
    {
        public override ActionResult DownloadEXCEL([FromUri] PlaybookUtilizationReportQuery query)
        {
            query.Excel = true;
            return base.DownloadEXCEL(query);
        }
        public override ActionResult Zip([FromUri] PlaybookUtilizationReportQuery query)
        {
            throw new NotImplementedException();
        }
        public override ActionResult Index(PlaybookUtilizationReportQuery query)
        {
            ViewBag.Groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter()).OrderBy(x => x.Title).ToList();
            ViewBag.Guides = ExecuteQuery<AllTrainingGuideQueryParameter, List<TrainingGuideViewModel>>(new AllTrainingGuideQueryParameter()).OrderBy(x => x.Title).ToList();
            var users = ExecuteQuery<AllStandardUserQueryParameter, List<UserViewModel>>(new AllStandardUserQueryParameter());
            if (query.GroupId.HasValue)
                users = users.Where(x => x.SelectedGroupId == query.GroupId.Value).ToList();
            ViewBag.Users = users.OrderBy(x => x.FullName).ToList();
            return base.Index(query);
        }
    }
}