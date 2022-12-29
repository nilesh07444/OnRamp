using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.CustomDocument
{

    public class CustomDocumentSummaryExportReportQuery : IContextQuery
    {
        public PortalContextViewModel PortalContext { get; set; }
        public CustomDocumentSubmissionReportQuery CustomDocumentSubmissionReportQuery { get; set; }
        public string TestQuestionID { get; set; }
        public string CustomDocumentID { get; set; }
        public bool AddOnrampBranding { get; set; }
        public string CompanyId { get; set; }
        public Guid UserId { get; set; }
        public bool IsChecklistTracked { get; set; }
        public bool IsDetail { get; set; } = true;
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }

    }
    public class CustomDocumentScheduleReportQuery : IContextQuery
    {
        public PortalContextViewModel PortalContext { get; set; }
        public CustomDocumentSubmissionReportQuery CustomDocumentSubmissionReportQuery { get; set; }
        public string TestQuestionID { get; set; }
        public string CustomDocumentID { get; set; }
        public bool AddOnrampBranding { get; set; }
        public string CompanyId { get; set; }
        public Guid UserId { get; set; }
        public bool IsChecklistTracked { get; set; }
        public bool IsDetail { get; set; } = true;
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }

    }
}
