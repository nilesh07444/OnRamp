using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Report;

namespace Ramp.Contracts.ViewModel
{
    public abstract class ContextReportModel<T> : IReportModel<T>
    {
        public IEnumerable<T> FilteredResults { get; set; }
    }
}
