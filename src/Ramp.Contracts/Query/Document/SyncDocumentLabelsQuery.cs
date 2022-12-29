using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Document
{
    public class SyncDocumentLabelsQuery
    {
        public IEnumerable<string> ExistingModelIds { get; set; } = new List<string>();
        public IEnumerable<string> Values { get; set; } = new List<string>();
    }
}
