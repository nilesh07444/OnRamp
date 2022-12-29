namespace Ramp.Contracts.Query.Memo {
	public	class MemoChapterUserResultQuery {

		public string AssignedDocumentId { get; set; }
		public string MemoChapterId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}

	public class MemoUserResultQuery
	{
		public string AssignedDocumentId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}
	
}
