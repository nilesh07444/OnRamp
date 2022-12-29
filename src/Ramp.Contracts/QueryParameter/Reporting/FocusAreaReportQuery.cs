using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Reporting
{
    public class FocusAreaReportQuery : IQuery
    {
        public string TestId { get; set; }
    }
}
