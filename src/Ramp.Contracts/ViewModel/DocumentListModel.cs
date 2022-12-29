using Common.Data;
using Domain.Customer;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel {
	public class DocumentListModel : IdentityModel<string>
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
		public DocumentStatus DocumentStatus { get; set; } 
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
		public string DocumentUrl { get; set; }

		public UserModelShort CreatedByModel { get; set; } 
        public string LastEditedBy { get; set; }
		public UserModelShort LastEditedByModel { get; set; }
		public bool Printable { get; set; } = false;
		public bool IsGlobalAccessed { get; set; } = false;

		public bool IsMannualReview { get; set; } = false;
		public bool IsChecklistTracked { get; set; } = false;

		public int Points { get; set; } = 0;
		public bool Deleted { get; set; } = false;
		public CategoryViewModelShort Category { get; set; }
        public string CategoryId { get; set; }
        public DateTime? LastEditDate { get; set; }
		public DocumentType DocumentType { get; set; }
		public UploadResultViewModel CoverPicture { get; set; }
        public string CoverPictureId { get; set; }
		public IEnumerable<UserModelShort> Collaborators { get; set; }
        public string TrainingLabels { get; set; } = string.Empty;
		public string LabelIds { get; set; }
		public AssignedDocumentStatus Status { get; set; }
		public DateTime? LastViewedDate { get; set; }
		public string Author { get; set; }
		public string DeclineMessage { get; set; }
		public List<DeclineMessages> DeclineMessages { get; set; }
		public bool IsCertificate { get; set; }
		public decimal PassMarks { get; set; }
		public int TestExpiresNumberDaysFromAssignment { get; set; }
		public bool? EmailSummary { get; set; }
		public bool? HighlightAnswersOnSummary { get; set; }
		public string CertificateUrl { get; set; }
		public int Duration { get; set; }
		public UploadResultViewModel Certificate { get; set; }
		public int Type { get; set; }

		//added by neeraj
		public DocumentPublishWorkflowStatus? PublishStatus { get; set; }
		public string Approver { get; set; }
		public Guid? ApproverId { get; set; }
		public string ApproversName { get; set; }

	}
    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public class DeclineMessages
    {
        public string messages { get; set; }
        public string CreatedOn { get; set; } /*added by softude*/
		public byte type { get; set; } /*added by softude*/

	}
}
