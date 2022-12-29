using System;

namespace Domain.Models
{
    public class ChapterUpload : DomainObject
    {
        public virtual TraningGuideChapter TraningGuideChapter { get; set; }
        public virtual Guid TraningGuideChapterId { get; set; }
        public virtual TrainingDocumentTypeEnum DocumentType { get; set; }
        public virtual string DocumentName { get; set; }
        public virtual byte[] DocumentFileContent { get; set; }
        public virtual int ChapterUploadSequence { get; set; }
        public string Content { get; set; }
    }

    public enum TrainingDocumentTypeEnum
    {
        Pdf = 0,
        Image = 1,
        Excel = 2,
        PowerPoint = 3,
        WordDocument = 4,
        Vimeo = 5,
        YoutubeVideo = 6,
        Other = 7,
        Video = 8,
    }
}