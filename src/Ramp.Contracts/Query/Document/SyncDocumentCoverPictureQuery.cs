using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Document
{
    public class SyncDocumentCoverPictureQuery
    {
        public string ExistingUploadId { get; set; }
        public string ModelId { get; set; }
    }
}
