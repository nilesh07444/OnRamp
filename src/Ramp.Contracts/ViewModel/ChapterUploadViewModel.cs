using Domain.Customer.Models;
using Domain.Models;
using System;
using System.Web;

namespace Ramp.Contracts.ViewModel
{
    public class ChapterUploadViewModel : IViewModel
    {
        public Guid ChapterUploadId { get; set; }

        public Guid TraningGuideChapterId { get; set; }

        public TrainingDocumentTypeEnum DocumentType { get; set; }

        public string DocumentName { get; set; }

        public string DocumentUrl { get; set; }

        public byte[] Content { get; set; }

        public string File { get; set; }

        public string ThumbnailUrl { get; set; }

        public int ChapterUploadSequence { get; set; }
    }

    public class ChapterUploadViewModelShort : IViewModel
    {
        public string DocumentName { get; set; }
        public TrainingDocumentTypeEnum DocumentType { get; set; }
        public Int32 TrainingGuideChapterNumber { get; set; }
        public bool IsSaved { get; set; }
        public int ChapterUploadSequence { get; set; }
    }
}