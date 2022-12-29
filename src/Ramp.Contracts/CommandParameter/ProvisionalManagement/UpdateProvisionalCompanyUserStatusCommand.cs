using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.ProvisionalManagement
{
    public class UpdateProvisionalCompanyUserStatusCommand : ICommand
    {
        public Guid ProvisionalCompanyUserId { get; set; }
        public bool ProvisionalCompanyUserStatus { get; set; }
        public bool SentFromProvisionalManagement { get; set; }
    }
}