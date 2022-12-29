using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.Document
{
    public class PrintDocumentQuery<TEntity> : IContextQuery where TEntity : class
    {
        public string Id { get; set; }
        public bool AddOnrampBranding { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public string userId { get; set; }
    }
}
