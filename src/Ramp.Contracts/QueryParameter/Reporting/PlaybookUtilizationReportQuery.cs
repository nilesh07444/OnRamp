using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.QueryParameter.Reporting
{
    public class PlaybookUtilizationReportQuery : IContextQuery
    {
        public Guid? GuideId { get; set; }
        public Guid? GroupId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? EffectiveDateTo { get; set; }
        public DateTime? EffectiveDateFrom { get; set; }

        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }
        public bool Excel { get; set; }
    }
}
