using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon.Reporting;

namespace Ramp.Contracts.ViewModel
{
    public class ExternalTrainingProviderListModel : IdentityModel<string>
    {
        public string CompanyName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactPerson { get; set; }
        public string MobileNumber { get; set; }
        public string BEEStatusLevel { get; set; }
        public string EmailAddress { get; set; }
		public string CertificateUploadId { get; set; }
		public string Url { get; set; }
	}
    public class ExternalTrainingProviderModel : ExternalTrainingProviderListModel
    {
        public string Address { get; set; }
        public IList<BEECertificateModel> BEECertificates { get; set; } = new List<BEECertificateModel>();

    }
    public class ExternalTrainingProviderReportModel : ContextReportModel<ExternalTrainingProviderModel>
    {
    }
}
