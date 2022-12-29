using System;
using Common.Data;

namespace Domain.Customer.Models.PolicyResponse
{
    public class PolicyResponse: IdentityModel<string>
    {
        public string PolicyId { get; set; }
        public string UserId { get; set; }
        public DateTime Created { get; set; }
        public bool? Response { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}