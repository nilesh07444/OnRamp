using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.ProvisionalManagement
{
    public class UpdateProvisionalCompanyStatusCommand : ICommand
    {
        public Guid ProvisionalCompanyId { get; set; }
        public bool ProvisionalCompanyStatus { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}