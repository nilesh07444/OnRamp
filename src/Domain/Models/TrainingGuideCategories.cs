using System;

namespace Domain.Models
{
    public class TrainingGuideCategories : DomainObject
    {
        public virtual Guid TraningGuideId { get; set; }
        public virtual Guid CategoryId { get; set; }
    }
}