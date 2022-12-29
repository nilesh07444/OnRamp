using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Events;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Events.GuideAndTestManagement
{
    public class TrainingGuideAndTestAssignedEvent : IEvent
    {
        public const string DefaultSubject = "New Playbook & Test Notification";
        public UserViewModel User { get; set; }
        public CompanyViewModel Company { get; set; }
        public string Subject { get; set; }
        public TrainingGuideViewModel TrainingGuide { get; set; }
        public TrainingTestViewModel TrainingTest { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}
