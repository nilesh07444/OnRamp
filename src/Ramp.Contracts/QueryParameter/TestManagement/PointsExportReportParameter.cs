using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class PointsExportReportParameter : IQuery
    {
        
        public Guid? ProvisionalCompanyId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? GroupId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
