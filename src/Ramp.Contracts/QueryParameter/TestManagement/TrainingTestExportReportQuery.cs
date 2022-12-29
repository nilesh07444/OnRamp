using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class TrainingTestExportReportQuery : IContextQuery
    {
        public PortalContextViewModel PortalContext { get ; set; }
        public Guid? TestId { get; set; }
        public bool HighlightCorrectAnswer { get; set; }
        public Guid? ResultId { get; set; }
        public bool AddOnrampBranding { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
