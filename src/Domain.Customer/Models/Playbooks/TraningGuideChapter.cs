using System;
using System.Collections.Generic;

namespace Domain.Customer.Models
{
    public class TraningGuideChapter : Base.CustomerDomainObject
    {
        public virtual TrainingGuide TrainingGuide { get; set; }
        public virtual Guid TraningGuidId { get; set; }
        public virtual string ChapterName { get; set; }
        public virtual Int32 ChapterNumber { get; set; }
        public virtual string ChapterContent { get; set; }
        public virtual List<CKEUpload> CKEUploads { get; set; }
        public virtual List<ChapterLink> ChapterLinks { get; set; }

        public virtual TrainingGuide LinkedTrainingGuide { get; set; }

        public virtual List<ChapterUpload> ChapterUploads { get; set; }

        public TraningGuideChapter()
        {
            ChapterLinks = new List<ChapterLink>();
            ChapterUploads = new List<ChapterUpload>();
            CKEUploads = new List<CKEUpload>();
        }
    }
}