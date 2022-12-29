using System;
using System.Collections.Generic;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.ProvisionalManagement
{
    public class ReassignUserToAnotherProvisionalCompanyCommand : ICommand
    {
        public ReassignUserToAnotherProvisionalCompanyCommand()
        {
            CustomerCompanyGuidList = new List<Guid>();
        }

        public Guid ToProvisionalCompanyId { get; set; }
        public List<Guid> CustomerCompanyGuidList { get; set; }
    }
}