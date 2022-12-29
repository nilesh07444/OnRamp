using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.Group
{
    public class DeleteGroupCommand : ICommand
    {
        public Guid GroupId { get; set; }
    }
}