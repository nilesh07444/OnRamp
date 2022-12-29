using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon.Reporting;

namespace Common.Report
{
    public interface IReportModel<TResult>
    {
        IEnumerable<TResult> FilteredResults { get; set; }
    }

    public interface IExportModel
    {
        string Title { get; set; }
        ReportDocument Document { get; set; }
        string Recepients { get; set; }

    }
}
