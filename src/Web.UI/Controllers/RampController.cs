using Common.Command;
using Common.Query;
using Ramp.Security.Attributes;
using Ramp.Security.Authorization;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Web.UI.Code.ActionFilters;

namespace Web.UI.Controllers
{
    [RampSecurity]
    [PortalContextActionFilter]
    public class RampController : Controller
    {
        protected CommandDispatcher CommandDispatcher { get; set; }
        protected IQueryExecutor QueryExecutor { get; set; }

        public RampController()
        {
            CommandDispatcher = DependencyResolver.Current.GetService<CommandDispatcher>();
            QueryExecutor = DependencyResolver.Current.GetService<IQueryExecutor>();
        }

      
        protected TResult ExecuteQuery<TParameters, TResult>(TParameters parameters)
            where TParameters : class
        {
            return QueryExecutor.Execute<TParameters, TResult>(parameters);
        }

        
        protected CommandResponse ExecuteCommand<TCommand>(TCommand command)
        {
            return CommandDispatcher.Dispatch(command);
        }

        #region Notifications

        private IList<Notification> Notifications
        {
            get
            {
                var notifications = Session["Notifications"];

                if (notifications == null)
                    Session["Notifications"] = new List<Notification>();

                return (IList<Notification>)Session["Notifications"];
            }
        }

        protected void NotifySuccess(string message, string context = "")
        {
            Notify(message, Notification.Success, context);
        }

        protected void NotifyError(string message, string context = "")
        {
            Notify(message, Notification.Error, context);
        }

        protected void NotifyInfo(string message, string context = "")
        {
            Notify(message, Notification.Info, context);
        }

        protected void Notify(string message, string type = Notification.Info, string context = "")
        {
            if (!ControllerContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                Notifications.Add(new Notification { Message = message, Type = type, Context = context });
        }

        public class Notification
        {
            public string Message { get; set; }
            public string Type { get; set; }
            public string Context { get; set; }

            public const string Success = "success";
            public const string Error = "error";
            public const string Info = "info";
        }

        #endregion Notifications
    }
}