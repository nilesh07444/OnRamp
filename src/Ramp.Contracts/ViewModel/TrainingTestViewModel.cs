using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingTestViewModel : IViewModel
    {
        public List<TrainingTestQuestionViewModel> QuestionsList { get; set; } = new List<TrainingTestQuestionViewModel>();

        public UserViewModel User { get; set; }

        public IEnumerable<SerializableSelectListItem> TrainingGuideList { get; set; }

        public string ReferenceId { get; set; }

        public string TrainingGuideName { get; set; }

        public Guid ParentTrainingTestId { get; set; }

        public Guid CreatedBy { get; set; }

        public string CompanyName { get; set; }

        public Guid TrainingTestId { get; set; }
        public int Version { get; set; }

        [Required(ErrorMessage = "Please enter title")]
        [Remote("CheckTrainingGuideTestTitle", "ManageTrainingTest", "ManageTrainingTest", HttpMethod = "CheckTrainingGuideTestTitle", ErrorMessage = "Test name already exist")]
        public string TestTitle { get; set; }

        [Required(ErrorMessage = "Please enter percentage")]
        [RegularExpression(@"(?!^0*$)(?!^0*\.0*$)^\d{1,2}(\.\d{1,2})|([0-9]{1,2}|[0-9]{1,2}\.0|[0-9]{1,2}\.00)?(100|100\.0|100\.00)?$", ErrorMessage = "Please enter Pass Mark %")]
        public decimal PassMarks { get; set; }

        [Required(ErrorMessage = "Please enter points")]
        [RegularExpression(@"^([0-9][0-9]{0,2}|1000)$", ErrorMessage = "Please enter Pass Points (0 - 1000)")]
        public int PassPoints { get; set; }

        [Required(ErrorMessage = "Please Select Playbook")]
        public Guid? SelectedTrainingGuideId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ActivePublishDate { get; set; }
        public bool ActiveStatus { get; set; }

        [Required(ErrorMessage = "Please enter test duration")]
        [RegularExpression("^[1-9][0-9]*$",
         ErrorMessage = "Please enter a Test Duration")]
        public int TestDuration { get; set; }
        public DateTime? DraftEditDate { get; set; }
        public bool DraftStatus { get; set; }
        [Required(ErrorMessage = "Please specify Expiry Date")]
        public DateTime? TestExpiryDate { get; set; }
        public string IntroductionContent { get; set; }
        public bool IsUserEligibleToTakeTest { get; set; }
        public bool TestStatus { get; set; }
        public bool IsTestExpiryDate { get; set; }
        public bool AssignMarksToQuestions { get; set; }
        public byte[] TestTrophy { get; set; }

        [Required(ErrorMessage = "Please select the Trophy")]
        public string TrophyName { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? DateTaken { get; set; }
        public string CertificateUrl { get; set; }
        public bool TestReview { get; set; }
        public ViewMode ViewMode { get; set; }
        public FunctionalMode Mode { get; set; }
        public string PageTitle { get; set; }
        public int UnreadFeedback { get; set; }
        public IEnumerable<FeedbackViewModel> Feedback { get; set; }
        public bool EnableMaximumTestRewriteFunction { get; set; }
        public int? MaximumRewrites { get; set; }
        public Guid? LastPublishedVersionId { get; set; }
        public Guid? DraftVersionId { get; set; }
        public bool DisableQuestionRandomization { get; set; }
        public bool EmailSummaryOnCompletion { get; set; }
        public string TrophyDataBase64Encoded { get; set; }
        public bool? HighlightAnswersOnSummary { get; set; }
    }
    public static partial class Extensions
    {
        public static TrainingTest ToDomainModel(this TrainingTestViewModel x)
        {
            return new TrainingTest
            {
                ActivePublishDate = x.ActivePublishDate,
                ActiveStatus = x.ActiveStatus,
                AssignMarksToQuestions = x.AssignMarksToQuestions,
                CreateDate = x.CreateDate.HasValue ? x.CreateDate.Value : DateTime.MinValue,
                CreatedBy = x.CreatedBy,
                DraftEditDate = x.DraftEditDate,
                DraftStatus = x.DraftStatus,
                Id = x.TrainingTestId,
                IntroductionContent = x.IntroductionContent,
                MaximumNumberOfRewites = x.MaximumRewrites,
                PassMarks = x.PassMarks,
                PassPoints = x.PassPoints,
                ReferenceId = x.ReferenceId,
                TestDuration = x.TestDuration,
                TestExpiryDate = x.TestExpiryDate,
                TestReview = x.TestReview,
                TestTitle = x.TestTitle,
                DisableQuestionRandomization = x.DisableQuestionRandomization,
                EmailSummaryOnCompletion = x.EmailSummaryOnCompletion,
                HighlightAnswersOnSummary = x.HighlightAnswersOnSummary
            };
        }
    }
}