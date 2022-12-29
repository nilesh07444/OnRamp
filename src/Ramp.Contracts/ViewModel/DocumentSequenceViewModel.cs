using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class DocumentSequenceViewModel : IViewModel
    {
        public Guid ChapterUploadId { get; set; }
        public string DocumentName { get; set; }
        public int sequenceId { get; set; }
        public string Url { get; set; }
        public string DocumentType { get; set; }
    }
}
