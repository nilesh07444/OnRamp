using System;

namespace Ramp.Contracts.ViewModel
{
    public class CustomerSurveyDetailViewModel : IViewModel
    {
        public string Comment { get; set; }
        public Guid CustomerSurveyId { get; set; }
        public int Rating { get; set; }
        public DateTime RatedOn { get; set; }
        public UserViewModel User { get; set; }
        public string Browser { get; set; }
        public Guid UserId { get; set; }
		public string Category { get; set; }
	}
}