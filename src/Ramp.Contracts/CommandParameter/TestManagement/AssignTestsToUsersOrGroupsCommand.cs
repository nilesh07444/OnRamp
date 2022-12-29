using Common.Command;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class AssignTestsToUsersOrGroupsCommand : ICommand
    {
        public Guid AssignedBy { get; set; }
        public AssignTestToUsersOrGroupViewModel AssignTestToUsersOrGroupViewModel { get; set; }
        public List<Guid> TestIds { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public bool Force { get; set; }
    }
}