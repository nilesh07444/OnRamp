using Common.Events;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events.CustomerManagement
{
    public class CustomerSelfProvisionedEvent : IEvent
    {
        public CompanyViewModel CompanyViewModel { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public string Subject { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}