using Common.Web;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Certificate;
using Ramp.Contracts.QueryParameter.Certificates;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web.UI.Code.ActionFilters;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers
{
    [PortalContextActionFilter]
    public class BEECertificateController : KnockoutCRUDController<BEECertificateListQuery,BEECertificateListModel,BEECertificate,BEECertificateModel,CreateOrUpdateBEECertificateCommand>
    {
        public override void Edit_PostProcess(BEECertificateModel model)
        {
            if (model.Upload != null)
            {
                model.Upload.DeleteUrl = Url.ActionLink<UploadController>(m => m.Delete(model.Upload.Id.ToString(), null));
                model.Upload.Url = Url.ActionLink<UploadController>(m => m.Get(model.Upload.Id.ToString(), false));
            }
        }
        [HttpPost]
        public HttpStatusCodeResult RemoveUploadFromCertificate(RemoveUploadFromCertificateCommand command)
        {
            if (!ModelState.IsValid || !base.ValidateAndExecute(command))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ModelState.ModelStateToDictionary().ToString());
            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }
    }
}