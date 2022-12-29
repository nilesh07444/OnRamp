using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class Assign_UnAssignPlaybookToUserCommand : ICommand
    {
        public ManageAssignmentUserViewModel ManageAssignmentUser { get; set; }
        public Guid TrainingGuideId { get; set; }
        public Guid CurrentlyLoggedInUser { get; set; }
        public AssignViewModel AssignViewModel { get; set; }
    }
}