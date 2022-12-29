using System;

namespace Ramp.Contracts.ViewModel
{
    public class TestAnswerViewModel : IViewModel
    {
        public Guid TestAnswerId { get; set; }
        public virtual string Option { get; set; }
        public bool Correct { get; set; }
        public virtual TrainingTestQuestionViewModel TrainingQuestion { get; set; }
        public virtual Guid TrainingQuestionId { get; set; }
        public int? Position { get; set; }
    }
}