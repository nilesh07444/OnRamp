using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Document
{
    public class SyncDocumentContentToolsUploadsQuery
    {
        public IEnumerable<string> ExistingModelIds { get; set; } = new List<string>();
        public IEnumerable<UploadFromContentToolsResultModel> Models { get; set; } = new List<UploadFromContentToolsResultModel>();
    }
}
