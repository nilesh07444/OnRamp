using Common.Command;
using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class CloneTrainingGuideChapterCommand : IdentityModel<Guid>,ICommand
    {
        public Guid CurrentlyLoggedInUserId { get; set; }
        public Guid CloneId { get; set; }
        public Guid TrainingGuideId { get; set; }
    }
}
