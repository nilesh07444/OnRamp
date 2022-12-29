using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class DeleteCustomColourCommand :
        ICommand
    {
        public Guid? CompanyId { get; set; }
    }
}
