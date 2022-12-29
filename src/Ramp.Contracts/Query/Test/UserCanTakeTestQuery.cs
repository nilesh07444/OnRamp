using System;

namespace Ramp.Contracts.Query.Test
{
    public class UserCanTakeTestQuery
    {
        public Guid UserId { get; set; }
        public string TestId { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}