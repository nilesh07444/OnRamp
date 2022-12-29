using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class DeleteCustomerCompanyCommand : ICommand
    {
        public Guid? CustomerCompanyId { get; set; }
    }
}