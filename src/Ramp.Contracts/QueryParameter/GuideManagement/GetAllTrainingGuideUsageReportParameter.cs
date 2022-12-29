using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class GetAllTrainingGuideUsageReportParameter : IQuery
    {
        public Guid CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime LastDate { get; set; }
        public bool IsCustomerLayerDashboard { get; set; }
        public Guid LoggedInUserId { get; set; }

    }
}
