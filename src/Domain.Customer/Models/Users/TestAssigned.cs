using System;

namespace Domain.Customer.Models
{
    public class TestAssigned : Base.CustomerDomainObject
    {
        public virtual Guid? UserId { get; set; }
        public virtual Guid? GroupId { get; set; }
        public virtual TrainingTest Test { get; set; }
        public Guid TestId { get; set; }
        public Guid AssignedBy { get; set; }
        public DateTime? AssignedDate { get; set; }
    }
}