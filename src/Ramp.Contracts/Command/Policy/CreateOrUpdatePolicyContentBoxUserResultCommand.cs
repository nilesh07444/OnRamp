using Common.Data;
using System;

namespace Ramp.Contracts.Command.Policy
{
	public class CreateOrUpdatePolicyContentBoxUserResultCommand : IdentityModel<string>
	{

		public string AssignedDocumentId { get; set; }
		public string PolicyContentBoxId { get; set; }
		public bool IsChecked { get; set; }
		public string IssueDiscription { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ChapterTrackedDate { get; set; }

		//added by softude
		public string IsActionNeeded { get; set; }
		


		#region Global accesssed
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion

	}
}
