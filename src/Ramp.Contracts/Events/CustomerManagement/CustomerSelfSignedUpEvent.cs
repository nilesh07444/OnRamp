using Common.Events;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events
{
    public class CustomerSelfSignedUpEvent : IEvent
    {
        public const string DefaultSubject = "Email confirmation";

        public CustomerSelfSignedUpEvent()
        {
            CustomerAdminViewModelsList = new List<UserViewModel>();
        }

        public string Subject { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public List<UserViewModel> CustomerAdminViewModelsList { get; set; }
        public UserViewModel CurrentAdminViewModel { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}