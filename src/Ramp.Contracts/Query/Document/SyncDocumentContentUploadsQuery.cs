using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Document
{
    public class SyncDocumentContentUploadsQuery
    {
        public IEnumerable<string> ExistingModelIds { get; set; } = new List<string>();
        public IEnumerable<UploadResultViewModel> Models { get; set; } = new List<UploadResultViewModel>(); 
    }
}
