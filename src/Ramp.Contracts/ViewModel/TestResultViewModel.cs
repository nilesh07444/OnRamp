using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class TestResultViewModel : IViewModel
    {
        public TestResultViewModel()
        {
            QuestionResults = new List<QuestionResultViewModel>();
        }

        public string CertificateUrl { get; set; }
        public Guid TestResultId { get; set; }
        public int TotalMarksScored { get; set; }
        public int NumberOfWrongAnswers { get; set; }
        public int NumberOfRightAnswers { get; set; }
        public int NumberOfUnattemptedQuestions { get; set; }
        public string TestTitle { get; set; }
        public string TrainingGuideReferenceId { get; set; }
        public string TrainingGuideTitle { get; set; }
        public Guid TrainingGuideId { get; set; }
        public bool TestResult { get; set; }
        public decimal TestResultPers { get; set; }
        public Guid TestId { get; set; }

        // these are used for trophy cabinet
        public bool IsTrophyPic { get; set; }

        public string TrophyPicName { get; set; }
        public int MarkScored { get; set; }
        public int MarksOutOff { get; set; }
        public string Result { get; set; }
        public double persentage { get; set; }
        public TrainingTestViewModel TestSnapshot { get; set; }
        public bool TestReview { get; set; }
        public List<QuestionResultViewModel> QuestionResults { get; set; }
        public bool DisableQuestionRandomization { get; set; }
        public int PassPoints { get; set; }
        public bool EmailSummaryOnCompletion { get; set; }
        public bool? HighlightAnswersOnSummary { get; set; }
    }
}