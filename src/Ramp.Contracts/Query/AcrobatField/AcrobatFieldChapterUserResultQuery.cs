namespace Ramp.Contracts.Query.AcrobatField {
	public	class AcrobatFieldChapterUserResultQuery {

		public string AssignedDocumentId { get; set; }
		public string AcrobatFieldChapterId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}

	public class AcrobatFieldUserResultQuery
	{
		public string AssignedDocumentId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}
	
}
