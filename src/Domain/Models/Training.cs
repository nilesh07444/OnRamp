using System;

namespace Domain.Models
{
    public class Training : DomainObject
    {
        public int AssignedTo { get; set; }
        //public int PackageId { get; set; }
        public virtual Package Package { get; set; }
        public DateTime AssignedOn { get; set; }
        public string AssignedBy { get; set; }
        public DateTime CompletedOn { get; set; }
        public string Description { get; set; }

    }
}
