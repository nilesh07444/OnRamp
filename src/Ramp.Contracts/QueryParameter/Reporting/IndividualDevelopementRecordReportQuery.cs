using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Ramp.Contracts.QueryParameter.Reporting
{
   public class IndividualDevelopementRecordReportQuery : IContextQuery
    {
        [Required(AllowEmptyStrings = false)]
        public string UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }

    }
}
