using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class TrainingLabel : Base.CustomerDomainObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class Label : IdentityModel<string>
    {
        public bool Deleted { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
