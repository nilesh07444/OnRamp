using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class StandardUserLoginStats : Base.CustomerDomainObject
    {
        public virtual StandardUser LoggedInUser { get; set; }
        public DateTime LogInTime { get; set; }
        public DateTime? LogOutTime { get; set; }
    }
}