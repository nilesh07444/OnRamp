using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Events;
using Domain.Customer.Models.Test;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Events.TestManagement
{
    public class TestCompletedEvent : IEvent
    {
        public const string DefaultSubject = "Test Completed";
        public UserViewModel UserViewModel { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public TestResultModel TestResultModel { get; set; }
        public string Subject { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}
}
