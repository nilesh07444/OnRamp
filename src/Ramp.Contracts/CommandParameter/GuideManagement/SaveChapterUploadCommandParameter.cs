using Common.Command;
using Ramp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class SaveChapterUploadCommandParameter : ICommand
    {
        public Guid Id { get; set; }
        public byte[] DocumentFileContent { get; set; }
        public string DocumentName { get; set; }
        public Guid TrainingGuideChapterId { get; set; }
        public int UploadSequence { get; set; }
    }
}