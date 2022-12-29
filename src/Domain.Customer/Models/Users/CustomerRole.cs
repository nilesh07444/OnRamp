using System.Collections.Generic;

namespace Domain.Customer.Models
{
    public class CustomerRole : Base.CustomerDomainObject
    {
        public string RoleName { get; set; }
        public string Description { get; set; }
    }
}