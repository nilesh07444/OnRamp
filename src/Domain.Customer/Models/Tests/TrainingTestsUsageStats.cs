using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class TrainingTestUsageStats : Base.CustomerDomainObject
    {
        public Guid TrainingTestId { get; set; }
        public Guid UserId { get; set; }
        public DateTime? DateTaken { get; set; }
        public bool Unassigned { get; set; }
    }
}
