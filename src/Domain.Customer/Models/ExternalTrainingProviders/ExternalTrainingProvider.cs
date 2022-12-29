using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class ExternalTrainingProvider : Base.CustomerDomainObject
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string ContactPerson { get; set; }
        public string EmialAddress { get; set; }
        public string MobileNumber { get; set; }
        public string BEEStatusLevel { get; set; }
		public string CertificateUploadId { get; set; }
		public virtual IList<BEECertificate> BEECertificates { get; set; } = new List<BEECertificate>();
    }
}
