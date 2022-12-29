namespace Ramp.Contracts.Query.Test {
	public	class TestChapterUserResultQuery {

		public string AssignedDocumentId { get; set; }
		public string TestChapterId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}

	public class TestUserResultQuery
	{
		public string AssignedDocumentId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}
	
}
