using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class ChapterLink : Base.CustomerDomainObject
    {
        public string Url { get; set; }
        public ChapterLinkType Type { get; set; }
        public TraningGuideChapter TraningGuideChapter { get; set; }
        public int ChapterUploadSequence { get; set; }

    }

    public enum ChapterLinkType
    {
        Vimeo,
        Youtube
    }
}
