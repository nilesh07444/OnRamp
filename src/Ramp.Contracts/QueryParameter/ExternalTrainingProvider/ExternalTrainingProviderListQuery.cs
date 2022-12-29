using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Query;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.QueryParameter.ExternalTrainingProvider
{
    public class ExternalTrainingProviderListQuery :  IContextQuery, IPagedQuery {
        public bool? ShowOnlyCompaniesWithMissingCertificates { get; set; }
        public IEnumerable<string> CompanyNames { get; set; } = new List<string>();
        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }
		public int Page { get ; set ; }
		public int? PageSize { get ; set ; }
		public IEnumerable<string> ExternalTrainingFilter { get; set; }
		public bool? EnableChecklistDocument { get; set; }
		public string SearchText { get; set; }
	}
}
