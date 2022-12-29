using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
   public  class UpdateSelfProvisionEmailConfirm  : ICommand
    {
       public string EmailId {get;set;}
    }
}
