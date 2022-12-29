using Common.Command;


namespace Ramp.Contracts.CommandParameter.CheckList {
	public class DeleteCheckListUserUploadResultCommand : ICommand {

		public string AssignedDocumentId { get; set; }
		public string CheckListChapterId { get; set; }
		public string DocumentId { get; set; }
		public string UploadId { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}
