using Common.Events;
using Domain.Customer.Models.Feedback;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Events.Feedback
{
    public class UserFeedbackCreatedEvent : IEvent
    {
        public string Id { get; set; }
        public UserFeedbackViewModel UserFeedbackViewModel { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}