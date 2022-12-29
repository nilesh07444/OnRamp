using Common.Events;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events.Account
{
    public class LostPasswordEvent : IEvent
    {
        public const string DefaultSubject = "OnRamp - Reset your password";
        public UserViewModel User { get; set; }
        public CompanyViewModel Company { get; set; }
        public string ResetToken { get; set; }
        public string Subject { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}