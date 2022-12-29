using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Custom_Fields
{
    public class Fields : Base.CustomerDomainObject
    {
        public string FieldName { get; set; }
        public string UserId { get; set; }
        public string FieldType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateEdited { get; set; }
        public DateTime? DateDeleted { get; set; }

    }
}
