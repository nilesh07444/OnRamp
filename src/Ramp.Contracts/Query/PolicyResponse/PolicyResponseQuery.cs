using System;

namespace Ramp.Contracts.Query.PolicyResponse
{
    public class PolicyResponseQuery
    {
        public string PolicyId { get; set; }
        public string UserId { get; set; }
		public bool IsGlobalAccess { get; set; }
	}
}