using System.Collections.Generic;

namespace Domain.Models
{
    public class Role : DomainObject
    {
        public const string Admin = "ADMIN", Reseller = "RESELLER";
        public string RoleName { get; set; }
        public string Description { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}