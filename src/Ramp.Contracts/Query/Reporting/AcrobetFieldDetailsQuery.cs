using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Reporting
{
  public  class AcrobetFieldDetailsQuery
    {

        public string CustomDocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string[] GroupIds { get; set; }
    }
}
