using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
 

    public class TestViewModel : IViewModel
    {
        public TestViewModel()
        {
            QuestionsList = new List<QuestionsViewModel>();
        }

        public List<QuestionsViewModel> QuestionsList { get; set; }

        public string ReferenceId { get; set; }

        public string TrainingGuideName { get; set; }

        public virtual Guid ParentTrainingTestId { get; set; }

        public string CompanyName { get; set; }

        public Guid TrainingTestId { get; set; }

        public string TestTitle { get; set; }

        public decimal PassMarks { get; set; }

        public Guid? SelectedTrainingGuideId { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? ActivePublishDate { get; set; }

        public bool ActiveStatus { get; set; }

        public int TestDuration { get; set; }

        public DateTime? DraftEditDate { get; set; }

        public bool DraftStatus { get; set; }

        public string IntroductionContent { get; set; }
        public  int Version { get; set; }
    }
}