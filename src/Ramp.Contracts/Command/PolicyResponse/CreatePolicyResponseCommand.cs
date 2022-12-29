namespace Ramp.Contracts.Command.PolicyResponse
{
    public class CreatePolicyResponseCommand
    {
        public string UserId { get; set; }
        public string PolicyId { get; set; }
        public bool? Response { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}