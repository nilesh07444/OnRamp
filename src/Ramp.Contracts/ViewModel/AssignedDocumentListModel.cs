using System;
using System.Collections.Generic;
using Common.Data;
using Domain.Customer;

namespace Ramp.Contracts.ViewModel {
	public class AssignedDocumentListModel : IdentityModel<string> {
		public DocumentType DocumentType { get; set; }
		public string ReferenceId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string CategoryId { get; set; }
		public DateTime AssignedDate { get; set; }
		public DateTime? LastViewedDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public bool Printable { get; set; }
		public string CertificateUrl { get; set; }
		public int AttemptsRemaining { get; set; }
		public AssignedDocumentStatus Status { get; set; }
		public string DocStatus { get; set; }
		public string AssignedDocumentId { get; set; }
		public string TrainingLabels { get; set; }
		public string Author { get; set; }
		public bool? EmailSummary { get; set; }
		public bool? HighlightAnswersOnSummary { get; set; }
		public bool? IsViewed { get; set; }
		public string DateAssigned { get; set; }
		public string Message { get; set; }
		public string NotificationType { get; set; }
		public string VirtualMeetingStartDate { get; set; }
		public string VirtualMeetingEndDate { get; set; }
		public bool IsJoinMeeting { get; set; } = false;
		public decimal PassMarks { get; set; }
		public int Duration { get; set; }
		public string DocumentHref { get; set; }
		public string ChecklistHref { get; set; }		
		public bool Deleted { get; set; }
		public string DeclineMessage { get; set; }
		public List<DeclineMessages> DeclineMessages { get; set; }
		public DateTime? CreatedOn { get; set; }
	}


	public enum AssignedDocumentStatus {

        #region Old Status 
        //Pending,
        //Viewed,
        //Failed,
        //Passed,
        //Incomplete,
        //Complete,
        //CheckList,
        #endregion

        //Status Updated By Softude
        Pending,
		InProgress,
		UnderReview,
		ActionRequired,
		Complete,		
		Passed,
		CheckList,

	}
}
