using Common.Command;


namespace Ramp.Contracts.CommandParameter.AcrobatField {
	public class DeleteAcrobatFieldUserUploadResultCommand : ICommand {

		public string AssignedDocumentId { get; set; }
		public string AcrobatFieldChapterId { get; set; }
		public string DocumentId { get; set; }
		public string UploadId { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}
