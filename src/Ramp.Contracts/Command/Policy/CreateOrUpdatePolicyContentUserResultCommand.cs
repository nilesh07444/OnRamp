using Common.Data;
using System;

namespace Ramp.Contracts.Command.Policy
{
    public class CreateOrUpdatePolicyContentUserResultCommand : IdentityModel<string>
    {
		public string AssignedDocumentId { get; set; }

		public bool Status { get; set; }
		public bool IsGlobalAccessed { get; set; }

		public DateTime SubmittedDate { get; set; }
		public string DocumentId { get; set; }
		public string UserId { get; set; }

	}
}
