using Common.Data;
using Domain.Customer;
using Domain.Enums;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.Test
{
    public class CreateOrUpdateTestCommand : IdentityModel<string>
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
		//public string DocumentUrl { get; set; }
		public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public UserModelShort CreatedByModel { get; set; }
        public bool Printable { get; set; }
		public bool IsGlobalAccessed { get; set; }
        public int Points { get; set; }
        public bool Deleted { get; set; }
        public CategoryViewModelShort Category { get; set; }
        public DateTime? LastEditDate { get; set; }
        public DocumentType DocumentType { get; set; }
        public DocumentPreviewMode PreviewMode { get; set; }
        public string TrainingLabels { get; set; } = string.Empty;
        public object CoverPictureUpload { get; set; }
        public UploadResultViewModel CoverPicture { get; set; }
        public UploadResultViewModel Certificate { get; set; }

        public decimal PassMarks { get; set; }
        public int? Duration { get; set; }
        public int MaximumAttempts { get; set; }
        public string IntroductionContent { get; set; }
        public DateTime? TestExpiryDate { get; set; }
        public bool AssignMarksToQuestions { get; set; }
        public bool TestReview { get; set; }
        public bool RandomizeQuestions { get; set; }
        public bool EmailSummary { get; set; }
        public bool? HighlightAnswersOnSummary { get; set; }
        public bool OpenTest { get; set; }
        public bool EnableTimer { get; set; }
        public int? TestExpiresNumberDaysFromAssignment { get; set; }
        public TestExpiryNotificationInterval NotificationInteval { get; set; }
        public int? NotificationIntevalDaysBeforeExpiry { get; set; }

        public IEnumerable<TestQuestionModel> ContentModels { get; set; } = new List<TestQuestionModel>();
        public IEnumerable<Domain.Customer.Models.StandardUser> Collaborators { get; set; }

		//added by neeraj
		public DocumentPublishWorkflowStatus PublishStatus { get; set; }
		public string Approver { get; set; }
		public Guid? ApproverId { get; set; }
	}
}
