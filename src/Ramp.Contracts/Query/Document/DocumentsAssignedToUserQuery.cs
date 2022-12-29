using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Document
{
    public class DocumentsAssignedToUserQuery
    {
        public string UserId { get; set; }
        public string CompanyId { get; set; }
    }
}
