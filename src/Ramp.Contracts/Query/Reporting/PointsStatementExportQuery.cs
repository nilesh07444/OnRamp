using System;
using System.Collections.Generic;
using Domain.Customer;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.Reporting
{
    public class PointsStatementExportQuery : IContextQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? GroupId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ProvisionalCompanyId { get; set; }
        public Guid? CompanyId { get; set; }
        public DocumentType[] DocumentTypes { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }
		public IEnumerable<string> TrainingLabels { get; set; } = new List<string>();
		public string  ToggleFilter { get; set; }
		public bool EnableGlobalAccessDocuments { get; set; } = false;
		public IEnumerable<string> UserIds { get; set; } = new List<string>();
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }
    }
}