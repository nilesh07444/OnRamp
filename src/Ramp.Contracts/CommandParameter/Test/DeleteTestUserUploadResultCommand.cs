using Common.Command;


namespace Ramp.Contracts.CommandParameter.Test {
	public class DeleteTestUserUploadResultCommand : ICommand {

		public string AssignedDocumentId { get; set; }
		public string TestChapterId { get; set; }
		public string DocumentId { get; set; }
		public string UploadId { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}
