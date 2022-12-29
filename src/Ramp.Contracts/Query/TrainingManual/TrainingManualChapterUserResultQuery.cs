namespace Ramp.Contracts.Query.TrainingManual {
	public	class TrainingManualChapterUserResultQuery {

		public string AssignedDocumentId { get; set; }
		public string TrainingManualChapterId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}

	public class TrainingManualUserResultQuery
	{
		public string AssignedDocumentId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}
	
}
