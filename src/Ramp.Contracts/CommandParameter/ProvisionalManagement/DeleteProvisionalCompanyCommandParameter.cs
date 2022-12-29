using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.ProvisionalManagement
{
    public class DeleteProvisionalCompanyCommandParameter : ICommand
    {
        public Guid ProvisionalComapanyId { get; set; }
    }
}