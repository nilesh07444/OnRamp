using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class ChangeCustomerSelfSignUpApproveStatusCommandParameter : ICommand
    {
        public Guid UserId { get; set; }

        public bool ApproveStatus { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
    }
}