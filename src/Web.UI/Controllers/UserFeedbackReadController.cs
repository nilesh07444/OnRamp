using Domain.Customer.Models.Feedback;
using Ramp.Contracts.Command.UserFeedbackRead;
using Ramp.Contracts.Query.UserFeedbackRead;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.UI.Controllers
{
    public class UserFeedbackReadController : FeedbackController<UserFeedbackReadListQuery,UserFeedbackReadistModel,UserFeedbackRead,UserFeedbackReadModel,CreateOrUpdateUserFeedbackReadCommand>
    {
    }
}