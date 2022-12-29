using Common.Events;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events.Feedback
{
    public class FeedbackCreatedEvent : 
        IEvent
    {
        public Guid Id { get; set; }
        public TrainingGuideViewModel TrainingGuideViewModel { get; set; }
        public TrainingTestViewModel TrainingTestViewModel { get; set; }
        public FeedbackViewModel FeedbackViewModel { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}
