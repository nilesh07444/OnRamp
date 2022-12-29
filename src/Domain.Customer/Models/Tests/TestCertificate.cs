using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class TestCertificate : Base.CustomerDomainObject
    {
        public virtual StandardUser User { get; set; }
        public Guid TestId { get; set; }
        public virtual FileUploads Upload { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual bool Passed { get; set; }
    }
}