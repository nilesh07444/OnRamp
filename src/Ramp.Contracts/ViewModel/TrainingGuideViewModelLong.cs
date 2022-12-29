using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingGuideViewModelLong : IViewModel
    {
        public TrainingGuideViewModelLong()
        {
            TraningGuideViewModelList = new List<TrainingGuideViewModel>();
            TrainingGuideChapterList = new List<TraningGuideChapterViewModel>();
            ChapterUploadList = new List<ChapterUploadViewModel>();
            AllCollaborators = new List<UserViewModel>();
        }

        public List<ChapterUploadViewModel> ChapterUploadList { get; set; }
        public List<TraningGuideChapterViewModel> TrainingGuideChapterList { get; set; }
        public List<TrainingGuideViewModel> TraningGuideViewModelList { get; set; }
        public TrainingGuideViewModel TrainingGuide { get; set; }
        public TrainingGuideViewModel TrainingGuideViewModelForDropdown { get; set; }
        public int MaxGuide { get; set; }
        public int MaxChapterPerGuide { get; set; }
        public int CurrnetGuideCount { get; set; }
        public List<UserViewModel> AllCollaborators { get; set; }
    }
}