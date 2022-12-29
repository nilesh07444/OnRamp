using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.PackageManagement
{
    // TODO: Delete when completely unused
    public class DeletePackageCommand : ICommand
    {
        public Guid PackageId { get; set; }
    }
}