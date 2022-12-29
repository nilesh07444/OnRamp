using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Certificates
{
    public class BEECertificateListQuery:IPagedQuery
    {
        public string ExternalTrainingProviderId { get; set; }
		public int Page { get; set; }
		public int? PageSize { get; set; }
		public bool? EnableChecklistDocument { get; set; }
	}
}
