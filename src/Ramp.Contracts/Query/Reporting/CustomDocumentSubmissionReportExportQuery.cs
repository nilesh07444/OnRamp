using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Reporting {
	public class CustomDocumentSubmissionReportExportQuery : CheckListSubmissionReportQuery, IContextQuery {
		public bool AddOnrampBranding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public PortalContextViewModel PortalContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}
