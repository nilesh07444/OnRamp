using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ramp.Contracts.CommandParameter.Settings
{
    public class SaveCustomConfigurationCommand : ICommand
    {
        public string CompanyId { get; set; }
        public HttpPostedFileBase DashboardLogo { get; set; }
        public HttpPostedFileBase LoginLogo { get; set; }
        public HttpPostedFileBase FooterLogo { get; set; }
		public HttpPostedFileBase NotificationHeaderLogo { get; set; }
        public HttpPostedFileBase NotificationFooterLogo { get; set; }
        public HttpPostedFileBase CSS { get; set; }
        public HttpPostedFileBase Certificate { get; set; }
        public HttpPostedFileBase Tropy { get; set; }
        public bool? DeleteDashboardLogo { get; set; }
        public bool? DeleteLoginLogo { get; set; }
        public bool? DeleteFooterLogo { get; set; }
        public bool? DeleteNotificationFooterLogo { get; set; }
        public bool? DeleteNotificationHeaderLogo { get; set; }
    }
}
