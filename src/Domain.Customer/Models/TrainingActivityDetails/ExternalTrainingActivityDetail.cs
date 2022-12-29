using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class ExternalTrainingActivityDetail : Base.CustomerDomainObject
    {
        public virtual IList<Upload> Invoices { get; set; } = new List<Upload>();
        public virtual IList<ExternalTrainingProvider> ConductedBy { get; set; } = new List<ExternalTrainingProvider>();
    }
}
