using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class BursaryTrainingActivityDetail : Base.CustomerDomainObject
    {
        public virtual IList<Upload> Invoices { get; set; } = new List<Upload>();
        public virtual IList<StandardUser> ConductedBy { get; set; } = new List<StandardUser>();
        public string BursaryType { get; set; }
    }
}
