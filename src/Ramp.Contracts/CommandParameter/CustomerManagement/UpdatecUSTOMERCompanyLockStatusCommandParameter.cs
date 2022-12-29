using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
   public class UpdateCustomerCompanyLockStatusCommandParameter : ICommand
    { 
       public Guid CompanyId { get; set; }
       public bool  LockStatus { get; set; }
    }
}
