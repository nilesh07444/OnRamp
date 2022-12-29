using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class DuplicateTrainingGuideCommand : ICommand
    {
        public Guid Id { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
    }
}
