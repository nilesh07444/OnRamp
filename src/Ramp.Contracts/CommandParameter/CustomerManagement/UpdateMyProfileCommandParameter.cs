using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class UpdateMyProfileCommandParameter : ICommand
    {
        public EditUserProfileViewModel EditUserProfileViewModel { get; set; }
    }
}
