using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Customer.Models;

namespace Ramp.Contracts.ViewModel
{
    public class ChapterLinkViewModel: IViewModel
    {
        public string Url { get; set; }
        public ChapterLinkType Type { get; set; }
        public int ChapterUploadSequence { get; set; }
        public int TrainingGuideChapterNumber { get; set; }
        public bool IsSaved { get; set; }
    }
}
