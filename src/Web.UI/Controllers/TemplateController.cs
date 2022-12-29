using Common.Command;
using Domain.Customer;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Common;
using Web.UI.Code.Extensions;
using Domain.Customer.Models.CheckLists;

namespace Web.UI.Controllers
{
    public class TemplateController : KnockoutListController<DocumentListQuery, DocumentListModel>
    {
        public override ActionResult Index([FromUri] DocumentListQuery query)
        {
            ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = ConfigurationManager.AppSettings["TemplatePortalId"].ToString() });
            ExecuteCommand(new UpdateConnectionStringCommand());
            query.TemplatePortal = true;
            return base.Index(query);
        }
        public override void Index_PostProcess(IEnumerable<DocumentListModel> listModel)
        {
            listModel.ToList().ForEach(m =>
            {
                if (m.CoverPicture != null)
                {
                    m.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.Get(m.CoverPicture.Id, false));
                    m.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(m.CoverPicture.Id, 500, 500));
                }
            });
        }
        [System.Web.Mvc.HttpGet]
        public JsonResult TemplateList([FromUri] DocumentListQuery query)
        {
            ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = ConfigurationManager.AppSettings["TemplatePortalId"] });

            query.DocumentFilters = new string[]
            {
                "Status:Published"
            };
            query.TemplatePortal = true;
            query.EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument;

            ExecuteCommand(new UpdateConnectionStringCommand());

            return new JsonResult { Data = null, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult List([FromUri] DocumentListQuery query)
        {
            ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = ConfigurationManager.AppSettings["TemplatePortalId"] });

            query.DocumentFilters = new string[]
            {
                "Status:Published"
            };
            query.TemplatePortal = true;
            query.EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument;

            //var data = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(query);

            //data.ToList().ForEach(m =>
            //{
            //    if (m.CoverPicture != null)
            //    {
            //        m.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.Get(m.CoverPicture.Id, false));
            //        m.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(m.CoverPicture.Id, 500, 500));
            //    }
            //});

            ExecuteCommand(new UpdateConnectionStringCommand());
            return new JsonResult { Data = null, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //  return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [System.Web.Mvc.HttpPost]
        public JsonResult Clone([FromBody] AuthorizeCloneCommand command)
        {
            command.TargetCompanyId = PortalContext.Current?.UserCompany?.Id.ToString();
            command.SourceCompanyId = ConfigurationManager.AppSettings["TemplatePortalId"].ToString();
            var result = GetResultAndRedirectLocation(command);
            if (result.Item1 != null && !result.Item1.Validation.Any())
                return new JsonResult { Data = new { HttpStatusCode = HttpStatusCode.OK, Url = result.Item2 }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult { Data = new { HttpStatusCode = HttpStatusCode.BadRequest, Url = result.Item2 }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        private Tuple<CommandResponse, string> GetResultAndRedirectLocation(AuthorizeCloneCommand command)
        {
            CommandResponse response = null;
            var path = string.Empty;
            if (Enum.TryParse<DocumentType>(command.Type, out var type))
            {
                switch (type)
                {
                    case DocumentType.Memo:
                        var cm = new CloneCommand<Memo> { Id = command.Id, SourceCompanyId = command.SourceCompanyId, TargetCompanyId = command.TargetCompanyId };
                        response = ExecuteCommand(cm);
                        path = Url.Action("Edit", "Memo", new { Area = "", Id = cm.Id });
                        break;
                    case DocumentType.Policy:
                        var cp = new CloneCommand<Policy> { Id = command.Id, SourceCompanyId = command.SourceCompanyId, TargetCompanyId = command.TargetCompanyId };
                        response = ExecuteCommand(cp);
                        path = Url.Action("Edit", "Policy", new { Area = "", Id = cp.Id });
                        break;
                    case DocumentType.Test:
                        var ct = new CloneCommand<Test> { Id = command.Id, SourceCompanyId = command.SourceCompanyId, TargetCompanyId = command.TargetCompanyId };
                        response = ExecuteCommand(ct);
                        path = Url.Action("Edit", "Test", new { Area = "", Id = ct.Id });
                        break;
                    case DocumentType.TrainingManual:
                        var ctr = new CloneCommand<TrainingManual> { Id = command.Id, SourceCompanyId = command.SourceCompanyId, TargetCompanyId = command.TargetCompanyId };
                        response = ExecuteCommand(ctr);
                        path = Url.Action("Edit", "TrainingManual", new { Area = "", Id = ctr.Id });
                        break;
                    case DocumentType.Checklist:
                        var ch = new CloneCommand<CheckList> { Id = command.Id, SourceCompanyId = command.SourceCompanyId, TargetCompanyId = command.TargetCompanyId };
                        response = ExecuteCommand(ch);
                        path = Url.Action("Edit", "CheckList", new { Area = "", Id = ch.Id });
                        break;
                    default: break;
                }
            }
            return new Tuple<CommandResponse, string>(response, path);
        }
    }
}