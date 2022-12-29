using Common.Enums;
using Domain.Customer;
using Domain.Customer.Models.Feedback;
using Ramp.Contracts.Command.UserFeedback;
using Ramp.Contracts.Query.UserFeedback;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtuaCon;

namespace Web.UI.Controllers
{
    public class UserFeedbackController : FeedbackController<UserFeedbackListQuery, UserFeedbackListModel, UserFeedback, UserFeedbackModel, CreateOrUpdateUserFeedbackCommand>
    {
        [HttpGet]
        public JsonResult Types()
        {
            var r = EnumUtilityExtensions.GetEnumFriendlyNamesDictionary(typeof(UserFeedbackType));
            return new JsonResult { Data = r.Select(x => new { Id = x.Key, Name = x.Value }).OrderBy(x => x.Name).ToArray(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpGet]
        public JsonResult ContentTypes()
        {
            var r = EnumUtilityExtensions.GetEnumFriendlyNamesDictionary(typeof(UserFeedbackContentType));
            return new JsonResult { Data = r.Select(x => new { Id = x.Key, Name = x.Value }).OrderBy(x => x.Name).ToArray(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}