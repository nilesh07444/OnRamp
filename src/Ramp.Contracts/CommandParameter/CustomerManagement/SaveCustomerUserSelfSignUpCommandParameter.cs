using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class SaveCustomerUserSelfSignUpCommandParameter : ICommand
    {
        public CompanyViewModel CompanyViewModel { get; set; }
        public CustomerSelfSignUpViewModel CustomerSelfSignUpViewModel { get; set; }
    }
}