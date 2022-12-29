using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class DeleteChapterUploadCommand : ICommand
    {
        public Guid ChapterUploadId { get; set; }
        public string CompanyName { get; set; }
        public string DocumentName { get; set; }
        public string GuideTitle { get; set; }
        public string PathToDeleteUploadedFiles { get; set; }
    }
}