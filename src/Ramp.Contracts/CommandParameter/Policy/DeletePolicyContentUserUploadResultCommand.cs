using Common.Command;

namespace Ramp.Contracts.CommandParameter.Policy
{
	public class DeletePolicyContentUserUploadResultCommand : ICommand
	{
		public string AssignedDocumentId { get; set; }
		public string PolicyContentBoxId { get; set; }
		public string DocumentId { get; set; }
		public string UploadId { get; set; }
		public bool IsGlobalAccessed { get; set; }

	}
}
