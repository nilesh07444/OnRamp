using System;

namespace Domain.Customer.Models
{
    public class TestAnswer : Base.CustomerDomainObject
    {
        public string Option { get; set; }
        public bool Correct { get; set; }
        public virtual TrainingQuestion TrainingQuestion { get; set; }
        public Guid TrainingQuestionId { get; set; }
        public int? Position { get; set; }
    }
}