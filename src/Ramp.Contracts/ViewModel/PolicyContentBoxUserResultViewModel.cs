using System;

namespace Ramp.Contracts.ViewModel
{
	public class PolicyContentBoxUserResultViewModel
	{

		public string AssignedDocumentId { get; set; }
		public string PolicyContentBoxId { get; set; }
		public bool IsChecked { get; set; }
		public DateTime CreatedDate { get; set; }
		public string IssueDiscription { get; set; }

		//added by softude
		public string IsActionNeeded { get; set; }
		
	}

	public class PolicyContentResultViewModel
	{
		public string AssignedDocumentId { get; set; }
		public bool Status { get; set; }
		public DateTime SubmittedDate { get; set; }
		public string Id { get; set; }
	}
}
