using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class TrainingGuideExportQuery : IContextQuery
    {
        public PortalContextViewModel PortalContext { get; set; }
        public Guid? TrainingGuideId { get; set; }
        public Guid? UserId { get; set; }
        public bool AddOnrampBranding { get; set; }
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }
    }
}
