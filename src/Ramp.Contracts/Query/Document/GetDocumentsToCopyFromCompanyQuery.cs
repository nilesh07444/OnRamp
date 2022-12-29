using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Document
{
    public class GetDocumentsToCopyFromCompanyQuery
    {
        public Guid FromCustomerCompanyId { get; set; }
        public Guid ToCustomerCompanyId { get; set; }
        public IEnumerable<string> DocumentList { get; set; }
    }
}
