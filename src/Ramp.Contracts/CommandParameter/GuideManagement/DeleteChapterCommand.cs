using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class DeleteChapterCommand : ICommand
    {
        public Guid TraningGuideChapterId { get; set; }
        public string PathToDeleteUploadedFiles { get; set; }
        public string CompanyName { get; set; }
        public string GuideName { get; set; }
        public int ChapterNumber { get; set; }
        public Guid TrainingGuideId { get; set; }
    }
}