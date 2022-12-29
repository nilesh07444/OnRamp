using Ramp.Contracts.CommandParameter.Icon;
using Ramp.Contracts.QueryParameter.IconSet;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers
{
    public class IconSetController : RampController
    {
        // GET: IconSet
        [HttpGet]
        public ActionResult Manage()
        {
            return View(ExecuteQuery<IconSetListQuery, IEnumerable<IconSetModel>>(new IconSetListQuery()));
        }
        [HttpGet]
        public ActionResult Edit(string id,bool? create)
        {
            if (create.HasValue && create.Value)
            {
                ViewBag.Action = "Create";
                return View(new IconSetModel { AvailableIcons = ExecuteQuery<AvaiableIconTypesQuery, IEnumerable<SelectListItem>>(new AvaiableIconTypesQuery()) });
            }
            ViewBag.Action = "Edit";
            var model = ExecuteQuery<FindIconSetQuery, IconSetModel>(new FindIconSetQuery { Id = id });
            foreach(var icon in model.Icons.Where(x => x.UploadModel != null).ToList())
            {
                icon.UploadModel.Url = Url.ActionLink<UploadController>(a => a.Get(icon.UploadModel.Id.ToString(), true));
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(SaveIconSetCommand command)
        {
            if (ExecuteCommand(command).Validation.Any())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }
        public ActionResult Delete(string id)
        {
            if (ExecuteCommand(new DeleteIconSetCommand { Id = id }).Validation.Any())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        
    }
}