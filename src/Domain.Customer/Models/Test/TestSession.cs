using System;
using Common.Data;

namespace Domain.Customer.Models.Test
{
    public class TestSession : IdentityModel<string>
    {
        public string UserId { get; set; }
        public string CurrentTestId { get; set; }
        public DateTime? StartTime { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}