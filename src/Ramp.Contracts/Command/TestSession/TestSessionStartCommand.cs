namespace Ramp.Contracts.Command.TestSession
{
    public class TestSessionStartCommand
    {
        public string UserId { get; set; }
        public string CurrentTestId { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}