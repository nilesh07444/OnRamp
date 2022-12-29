using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class BEECertificate : Base.CustomerDomainObject 
    {
        public virtual FileUploads Upload { get; set; }
        public int? Year { get; set; }
    }
}
