using Common.Command;


namespace Ramp.Contracts.CommandParameter.TrainingMannual {
	public class DeleteTrainingMannualUserUploadResultCommand : ICommand {

		public string AssignedDocumentId { get; set; }
		public string TrainingMannualChapterId { get; set; }
		public string DocumentId { get; set; }
		public string UploadId { get; set; }
		public bool IsGlobalAccessed { get; set; }
	}
}
