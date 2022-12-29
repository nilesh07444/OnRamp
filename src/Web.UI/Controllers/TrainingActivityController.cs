using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TrainingActivity;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Web.UI.Code.ActionFilters;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers
{
    [PortalContextActionFilter]
    public class TrainingActivityController : KnockoutCRUDController<TrainingActivityListQuery,TrainingActivityListModel,StandardUserTrainingActivityLog,TrainingActivityModel,CreateOrUpdateTrainingActivityCommand>
    {
        public override void Edit_PostProcess(TrainingActivityModel model)
        {
           Action<UploadResultViewModel> rewriteUrls = x =>
           {
               x.Url = Url.ActionLink<UploadController>(a => a.Get(x.Id.ToString(), false));
               x.DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(x.Id.ToString(), null));
               x.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(x.Id.ToString(), 300, 300));
           };
            model.Documents.ToList().ForEach(rewriteUrls);
            switch (model.TrainingActivityType)
            {
                case TrainingActivityType.Bursary:
                    model.BursaryTrainingActivityDetail.Invoices.ToList().ForEach(rewriteUrls);
                    break;
                case TrainingActivityType.External:
                    model.ExternalTrainingActivityDetail.Invoices.ToList().ForEach(rewriteUrls);
                    break;
                default:
                    break;
            }
        }
        public override ActionResult Save(CreateOrUpdateTrainingActivityCommand command)
        {
            command.CreatedBy = new UserModelShort { Id = SessionManager.GetCurrentlyLoggedInUserId() };
            command.EditedBy = new UserModelShort { Id = SessionManager.GetCurrentlyLoggedInUserId() };
            return base.Save(command);
        }
    }
}