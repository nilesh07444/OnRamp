using Common.Query;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.ExternalTrainingProvider {
	public class ExternalTrainingProviderQueryParameter : IContextQuery {
		public string Id { get; set; }
		public string Email { get; set; }
		public PortalContextViewModel PortalContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool AddOnrampBranding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}
