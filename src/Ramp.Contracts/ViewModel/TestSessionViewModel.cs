using Domain.Customer;
using System;

namespace Ramp.Contracts.ViewModel
{
    public class TestSessionViewModel
    {
        public TimeSpan TimeLeft { get; set; }
        public bool OpenTest { get; set; }
        public bool EnableTimer { get; set; }
        public DocumentStatus? DocumentStatus { get; set; }
        public string CurrentTestId { get; set; }
		public string Title { get; set; }
		public bool IsGloballyAccessed { get; set; }
	}
}