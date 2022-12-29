using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class TestUserAnswer : Base.CustomerDomainObject
    {
        public virtual TestAnswer Answer { get; set; }
        public virtual TestResult Result { get; set; }
    }
}
