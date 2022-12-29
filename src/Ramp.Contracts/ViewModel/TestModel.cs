using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TestListModel : DocumentListModel
    {

    }
    public class TestModel : TestListModel
    {
		//public List<DocumentUrlViewModel> DocLinks { get; set; }
		public int? TestExpiresNumberDaysFromAssignment { get; set; }
        public TestExpiryNotificationInterval NotificationInteval { get; set; }
        public int? NotificationIntevalDaysBeforeExpiry { get; set; }
        public decimal PassMarks { get; set; }
        public int Duration { get; set; }
        public int MaximumAttempts { get; set; }
        public string IntroductionContent { get; set; }
        public bool AssignMarksToQuestions { get; set; }
        public bool TestReview { get; set; }
        public bool RandomizeQuestions { get; set; }
        public bool EmailSummary { get; set; }
        public bool? HighlightAnswersOnSummary { get; set; }
        public bool OpenTest { get; set; }
        public bool EnableTimer { get; set; } = true;
        public DocumentPreviewMode PreviewMode { get; set; }
        public object CoverPictureUpload { get; set; }
        public string CertificateId { get; set; }
        public string CertificateThumbnailId { get; set; }
        public UploadResultViewModel Certificate { get; set; }
        public IEnumerable<TestQuestionModel> ContentModels { get; set; } = new List<TestQuestionModel>();
		public bool IsGlobalAccessed { get; set; }

        //added by softude
        public string Title { get; set; }
    }
}
