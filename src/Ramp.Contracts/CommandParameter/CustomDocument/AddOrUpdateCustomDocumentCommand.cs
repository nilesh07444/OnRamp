using Common.Command;
using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Enums;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomDocument {
	public class AddOrUpdateCustomDocumentCommand: IdentityModel<string> {

		//public Guid Id { get; set; }
		public string ReferenceId { get; set; }
		[Required]
		public string Title { get; set; }
		[Required]
		public string Description { get; set; }
		public DocumentStatus DocumentStatus { get; set; }
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; }
		public virtual List<StandardUser> Collaborators { get; set; }
		public bool Printable { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public bool IsMannualReview { get; set; }

		//added by softude
		public bool IsResourceCentre { get; set; }

		public int Points { get; set; }
		public bool Deleted { get; set; }
		public DocumentPreviewMode PreviewMode { get; set; }
		//public virtual Upload CoverPicture { get; set; }
		public string CoverPictureId { get; set; }
		public virtual DocumentCategory Category { get; set; }
		public string DocumentCategoryId { get; set; }
		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public string TrainingLabels { get; set; }
		public bool CallToAction { get; set; } = true;
		public string CallToActionMessage { get; set; }
		public bool TrainingMannual { get; set; }
		public bool CheckList { get; set; }
		public bool Memo { get; set; }
		public bool AcrobatField { get; set; }
		public bool Test { get; set; }
		public bool Policy { get; set; }

		//added by softude
		public bool Form { get; set; }

		public IEnumerable<TrainingManualChapterModel> TMContentModels { get; set; } = new List<TrainingManualChapterModel>();

		public virtual IEnumerable<TestQuestionModel> TestContentModels { get; set; } = new List<TestQuestionModel>();

		//added by softude
		public virtual IEnumerable<FormChapterModel> FormContentModels { get; set; } = new List<FormChapterModel>();


		public virtual IEnumerable<PolicyContentBoxModel> PolicyContentModels { get; set; } = new List<PolicyContentBoxModel>();

		public virtual IEnumerable<MemoContentBoxModel> MemoContentModels { get; set; } = new List<MemoContentBoxModel>();
	public virtual IEnumerable<AcrobatFieldContentBoxModel> AcrobatFieldContentModels { get; set; } = new List<AcrobatFieldContentBoxModel>();

		public virtual IEnumerable<CheckListChapterModel> CLContentModels { get; set; } = new List<CheckListChapterModel>();
		public PolicyModel PolicyContent { get; set; } = new PolicyModel();

		//added by neeraj
		public DocumentPublishWorkflowStatus? PublishStatus { get; set; }
		public string Approver { get; set; }
		public Guid? ApproverId { get; set; }

		//added by softude
		public UploadResultViewModel Certificate { get; set; }
		public object CoverPictureUpload { get; set; }
		public UploadResultViewModel CoverPicture { get; set; }

		public int? TestExpiresNumberDaysFromAssignment { get; set; }
		public TestExpiryNotificationInterval NotificationInteval { get; set; }
		public int? NotificationIntevalDaysBeforeExpiry { get; set; }

	}
}
