using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Customer.Models.CheckLists {
	public class CheckList : IdentityModel<string>, IDocument {

	public string ReferenceId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DocumentStatus DocumentStatus { get; set; }
		public DateTime? CreatedOn { get; set; }
		public string CreatedBy { get; set; }

		public int Points { get; set; }
		
		public bool Printable { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public bool IsChecklistTracked { get; set; }
		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public bool Deleted { get; set; }

		public string TrainingLabels { get; set; }
		public DocumentPreviewMode PreviewMode { get; set; }

		public int? CheckListExpiresNumberDaysFromAssignment { get; set; }
		public TestExpiryNotificationInterval NotificationInteval { get; set; }
		public int? NotificationIntevalDaysBeforeExpiry { get; set; }
		public DateTime? ExpiryNotificationSentOn { get; set; }
			   		 
		public string CoverPictureId { get; set; }

		public string DocumentCategoryId { get; set; }
		public virtual DocumentCategory Category { get; set; }

		public virtual Upload CoverPicture { get; set; }
		public virtual ICollection<CheckListChapter> Chapters { get; set; } = new List<CheckListChapter>();
		
		public virtual List<StandardUser> Collaborators { get; set; }

		//added by neeraj
		public DocumentPublishWorkflowStatus? PublishStatus { get; set; }
		public string Approver { get; set; }
		public Guid ApproverId { get; set; }
		public bool? IsCustomDocument { get; set; }
		public Guid CustomDocummentId { get; set; }
	}
}
