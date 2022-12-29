using Common.Events;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events.GuideManagement
{
    public class TrainingGuideAssignedEvent : IEvent
    {
        public const string DefaultSubject = "Playbook Notification";
        public UserViewModel UserViewModel { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public string Subject { get; set; }
        public TrainingGuideViewModel TrainingGuideViewModel { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}