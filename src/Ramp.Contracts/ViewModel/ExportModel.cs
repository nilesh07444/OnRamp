using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Query;
using Common.Report;
using VirtuaCon.Reporting;

namespace Ramp.Contracts.ViewModel
{
    public class ExportModel : IExportModel
    {
        public string Title { get; set; }
        public ReportDocument Document { get; set; }
        public string Recepients { get; set; }
    }
}
