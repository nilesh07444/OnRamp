using Common.Command;
using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class CloneChapterUploadCommand : IdentityModel<Guid>,ICommand
    {
        public Guid CloneId { get; set; }
        public Guid TrainingGuideChapterId { get; set; }
    }
}
