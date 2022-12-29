using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class PublishTrainingTestCommandParameter : ICommand
    {
        public Guid TrainingTestId { get; set; }
        public Guid CurrentlyLoggedInUser { get; set; }
        public CompanyViewModel Company { get; set; }
        public bool DoNotAssignTests { get; set; }
    }
}