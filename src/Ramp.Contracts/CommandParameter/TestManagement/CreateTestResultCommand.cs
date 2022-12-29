using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement {
	public class CreateTestResultCommand : TestResultModel
    {
        public string ResultId { get; set; }
        public string UserId { get; set; }
        public TimeSpan TimeLeft { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
		public bool IsGloballyAccessed { get; set; }
	}
}
