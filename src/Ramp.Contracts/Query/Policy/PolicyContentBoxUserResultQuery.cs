
namespace Ramp.Contracts.Query.Policy
{
	public class PolicyContentBoxUserResultQuery
	{
		public string AssignedDocumentId { get; set; }
		public string PolicyContentBoxId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }

		public string CreatedDate { get; set; }

	}

	public class PolicyContentUserResultQuery
	{
		public string AssignedDocumentId { get; set; }
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
	}
}
