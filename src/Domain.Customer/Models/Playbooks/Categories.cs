using Common.Data;
using System;
using System.Collections.Generic;
namespace Domain.Customer.Models
{
    public class Categories : Base.CustomerDomainObject
    {
        public string CategoryTitle { get; set; }
        public string Description { get; set; }
        public Guid CreatedUnderCompanyId { get; set; }
        public Guid? ParentCategoryId { get; set; } 
        public virtual ICollection<TrainingGuide> TrainingGuides { get; set; }

    }
    public class DocumentCategory : IdentityModel<string>
    {
        public string Title { get; set; }
        public string ParentId { get; set; }
    }
}