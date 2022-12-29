namespace Ramp.Contracts.Query.CheckListUserResult {
	public class CheckListUserResultQuery {
		public string AssignedDocumentId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}
}
