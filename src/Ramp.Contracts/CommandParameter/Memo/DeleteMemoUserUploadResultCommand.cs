using Common.Command;


namespace Ramp.Contracts.CommandParameter.Memo {
	public class DeleteMemoUserUploadResultCommand : ICommand {

		public string AssignedDocumentId { get; set; }
		public string MemoChapterId { get; set; }
		public string DocumentId { get; set; }
		public string UploadId { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}
