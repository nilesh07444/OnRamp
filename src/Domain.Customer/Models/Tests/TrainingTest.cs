using System;
using System.Collections.Generic;

namespace Domain.Customer.Models
{
    public class TrainingTest : Base.CustomerDomainObject
    {
        public string ReferenceId { get; set; }
        public string TestTitle { get; set; }
        public decimal PassMarks { get; set; }
        public int PassPoints { get; set; }
        public virtual Guid? TrainingGuideId { get; set; }
        public virtual Guid ParentTrainingTestId { get; set; }
        public virtual TrainingGuide TrainingGuide { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ActivePublishDate { get; set; }
        public bool ActiveStatus { get; set; }
        public int TestDuration { get; set; }
        public DateTime? DraftEditDate { get; set; }
        public bool DraftStatus { get; set; }
        public Guid CreatedBy { get; set; }
        public string IntroductionContent { get; set; }
        public DateTime? TestExpiryDate { get; set; }
        public DateTime? ExpiryNotificationSentOn { get; set; }
        public byte[] TestTrophy { get; set; }
        public string TrophyName { get; set; }
        public bool AssignMarksToQuestions { get; set; }
        public int? Version { get; set; }
        public virtual List<TrainingQuestion> QuestionList { get; set; }
        public bool TestReview { get; set; }
        public int? MaximumNumberOfRewites { get; set; }
        public bool? Deleted { get; set; }
        public bool DisableQuestionRandomization { get; set; }
        public bool EmailSummaryOnCompletion { get; set; }
        public bool? HighlightAnswersOnSummary { get; set; }
        public TrainingTest()
        {
            QuestionList = new List<TrainingQuestion>();
        }
    }
}