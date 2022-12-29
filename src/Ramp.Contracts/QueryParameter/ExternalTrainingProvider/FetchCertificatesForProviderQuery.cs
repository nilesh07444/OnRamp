using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.ExternalTrainingProvider
{
    public class FetchCertificatesForProviderQuery
    {
        public string ProviderId { get; set; }
        public int? Year { get; set; }
    }
}
