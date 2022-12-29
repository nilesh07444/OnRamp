using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class BEECertificateListModel : IdentityModel<string>
    {
        public int? Year { get; set; }
        public string ExternalTrainingProviderId { get; set; }
    }
    public class BEECertificateModel : BEECertificateListModel
    {
        public FileUploadResultViewModel Upload { get; set; }
    }

}
