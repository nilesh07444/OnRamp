using Common.Events;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events.TestManagement
{
    public class TrainingTestAssignedEvent : IEvent
    {
        public const string DefaultSubject = "Test Notification";
        public UserViewModel UserViewModel { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public TrainingTestViewModel TrainingTestViewModel { get; set; }
        public string Subject { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}