using Common.Data;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models {
	public class CustomDocument : IdentityModel<string>, IDocument {
		
		public string ReferenceId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DocumentStatus DocumentStatus { get; set; }
		public DateTime? CreatedOn { get; set; }
		public string CreatedBy { get; set; }
		public virtual List<StandardUser> Collaborators { get; set; }
		public bool Printable { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public bool IsMannualReview { get; set; }
		public bool IsResourceCentre { get; set; }
		public int Points { get; set; }
		public bool Deleted { get; set; }
		public DocumentPreviewMode PreviewMode { get; set; }
		public virtual Upload CoverPicture { get; set; }
		public string CoverPictureId { get; set; }
		public virtual DocumentCategory Category { get; set; }
		public string DocumentCategoryId { get; set; }
		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public string TrainingLabels { get; set; }
		public Guid? TrainingMannualId { get; set; }
		public Guid? CheckListId { get; set; }
		public Guid? MemoId { get; set; }
		public Guid? AcrobatFieldId { get; set; }
		public Guid? TestId { get; set; }
		public Guid? PolicyId { get; set; }
		public virtual ICollection<TrainingManualChapter> TMContentModels { get; set; } = new List<TrainingManualChapter>();
		public virtual ICollection<TestQuestion> TestContentModels { get; set; } = new List<TestQuestion>();
		public virtual ICollection<PolicyContentBox> PolicyContentModels { get; set; } = new List<PolicyContentBox>();
        public virtual ICollection<MemoContentBox> MemoContentModels { get; set; } = new List<MemoContentBox>();
		public virtual ICollection<AcrobatFieldContentBox> AcrobatFieldContentModels { get; set; } = new List<AcrobatFieldContentBox>();		
		public virtual ICollection<CheckListChapter> CLContentModels { get; set; } = new List<CheckListChapter>();
		public DocumentPublishWorkflowStatus? PublishStatus { get; set; }
		public string Approver { get; set; }
		public Guid ApproverId { get; set; }
		public string CertificateId { get; set; }
		public virtual Certificate Certificate { get; set; }
		public int? TestExpiresNumberDaysFromAssignment { get; set; }
		public TestExpiryNotificationInterval NotificationInteval { get; set; }
		public int? NotificationIntevalDaysBeforeExpiry { get; set; }
	}
}
