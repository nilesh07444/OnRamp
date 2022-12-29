using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Group
{
    public class UpdateRoleDescriptionsCommand : ICommand
    {
        public Dictionary<string, string> RoleDescriptionDictionary { get; set; }
    }
}