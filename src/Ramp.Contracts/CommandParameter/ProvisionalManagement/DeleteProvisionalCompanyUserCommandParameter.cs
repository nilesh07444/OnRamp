using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.ProvisionalManagement
{
    public class DeleteProvisionalCompanyUserCommandParameter : ICommand
    {
        public Guid ProvisionalComapanyUserId { get; set; }
    }
}