using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class AddSurveyDetailsByCustomerUserCommand : ICommand
    {
        public Guid CurrentUserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string Browser { get; set; }
		public string Category { get; set; }
	}
}