using System;
using System.Collections.Generic;

namespace Domain.Customer.Models
{
    public class TrainingQuestion : Base.CustomerDomainObject
    {
        public virtual TrainingTest TrainingTest { get; set; }
        public virtual Guid TrainingTestId { get; set; }
        public string TestQuestion { get; set; }
        public int TestQuestionNumber { get; set; }
        public int AnswerWeightage { get; set; }
        public string CorrectAnswer { get; set; }
        public string ImageName { get; set; }
        public string VideoName { get; set; }
        public virtual QuestionUpload Image { get; set; }
        public virtual QuestionUpload Video { get; set; }
        public virtual QuestionUpload Audio { get; set; }
        public virtual List<TestAnswer> TestAnswerList { get; set; }

        public TrainingQuestion()
        {
            TestAnswerList = new List<TestAnswer>();
        }
    }
}