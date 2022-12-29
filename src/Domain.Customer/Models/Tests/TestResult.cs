using System;
using System.Collections.Generic;

namespace Domain.Customer.Models
{
    public class TestResult : Base.CustomerDomainObject
    {
        public virtual Guid TestTakenByUserId { get; set; }
        public virtual Guid? TrainingGuideId { get; set; }
        public virtual Guid? TrainingTestId { get; set; }
        public virtual int CorrectAnswers { get; set; }
        public virtual int WrongAnswers { get; set; }
        public virtual int TestScore { get; set; }
        public virtual bool TestResultStatus { get; set; }
        public virtual int Points { get; set; }
        public virtual int Total { get; set; }
        public virtual DateTime TestDate { get; set; }
        public virtual int Version { get; set; }
        public virtual string TestTitle { get; set; }
        public virtual string TrainingGuideTitle { get; set; }
        public virtual string TrainingGuideCategory { get; set; }
        public virtual Guid? TrainingGuideCategoryId { get; set; }
        public virtual string TrophyName { get; set; }
        public virtual byte[] TrophyData { get; set; }
        public bool MaximumTestRewritesReached { get; set; }
    }
}