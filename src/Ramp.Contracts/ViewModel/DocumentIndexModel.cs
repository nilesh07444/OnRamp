using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class DocumentIndexModel
    {
        public IEnumerable<DocumentListModel> Documents { get; set; } = new List<DocumentListModel>();
        public IDictionary<string, string> Links { get; set; } = new Dictionary<string, string>();
    }
}
