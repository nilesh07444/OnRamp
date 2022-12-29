namespace Ramp.Contracts.Query.CheckListChapterUserResult {
	public	class CheckListChapterUserResultQuery {

		public string AssignedDocumentId { get; set; }
		public string CheckListChapterId { get; set; }
		public string DocumentId { get; set; }
		public bool isSignOff { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}
}
