using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class QuestionsViewModel : IViewModel
    {
        public QuestionsViewModel()
        {
            TestAnswerList = new List<TestAnswerViewModel>();
        }

        public List<TestAnswerViewModel> TestAnswerList { get; set; }

        public Guid TrainingTestQuestionId { get; set; }

        public virtual TrainingTestViewModel TrainingTest { get; set; }

        public virtual Guid TrainingTestId { get; set; }
        public string TestQuestion { get; set; }

        public int TestQuestionNumber { get; set; }

        public string CorrectAnswer { get; set; }

        public bool ViewLaterFlag { get; set; }

        public IEnumerable<SerializableSelectListItem> AnswerList { get; set; }

        public string ImageName { get; set; }
        public string VideoName { get; set; }
        public string AudioName { get; set; }

        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public string AudioUrl { get; set; }
        public string AudioType { get; set; }
        public ChapterUploadViewModel Image { get; set; }
        public ChapterUploadViewModel Video { get; set; }
        public ChapterUploadViewModel Audio { get; set; }
    }
}