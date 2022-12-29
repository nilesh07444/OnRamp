using Common.Command;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
   public class UpdateCustomerCompanyAdminSettingCommand : ICommand
    {

        public Guid Id { get; set; } 
        public int DefaultUserExpireDays { get; set; }
        public bool ShowCompanyNameOnDashboard { get; set; }
        public bool HideDashboardLogo { get; set; }
        public LegalDisclaimerActivationType? LegalDisclaimerActivationType { get; set; }
        public string LegalDisclaimer { get; set; }
        public string DashboardVideoFileId { get; set; }
        public string DashboardVideoTitle { get; set; }
        public string DashboardVideoDescription { get; set; }
        public string DashboardQuoteAuthor { get; set; }
        public string DashboardQuoteText { get; set; }
		public bool IsForSelfSignUp { get; set; }
		public bool IsSelfSignUpApprove { get; set; }
		public bool IsEmployeeCodeReq { get; set; }
		public bool IsEnabledEmployeeCode { get; set; }
		public string CompanySiteTitle { get; set; }
		public bool IsCompanySiteTitleReset { get; set; } = false;
	}
}
