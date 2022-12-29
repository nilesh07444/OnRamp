using System;

namespace Domain.Customer.Models
{
    public class AssignedTrainingGuides : Base.CustomerDomainObject
    {
        public virtual Guid? UserId { get; set; }
        public virtual Guid? GroupId { get; set; }
        public virtual Guid TrainingGuideId { get; set; }
        public virtual TrainingGuide TrainingGuide { get; set; }
        public Guid AssignedBy { get; set; }
        public DateTime? AssignedDate { get; set; }
    }
}