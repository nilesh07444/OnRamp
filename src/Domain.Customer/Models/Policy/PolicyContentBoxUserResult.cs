using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Policy;
using System;

namespace Domain.Customer.Models.Policy
{
	public class PolicyContentBoxUserResult : IdentityModel<string>
	{
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string PolicyContentBoxId { get; set; }
		public virtual PolicyContentBox PolicyContentBox { get; set; }
		public bool IsChecked { get; set; }
		public string IssueDiscription { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime DateCompleted { get; set; }
		public DateTime? ChapterTrackedDate { get; set; }


		//added by softude
		
		public string IsActionNeeded { get; set; }



		#region For Global access
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion
	
	}
}
