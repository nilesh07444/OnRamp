using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.Group
{
    public class DeleteStandardUserGroupCommand : ICommand
    {
		public Guid? UserId { get; set; }
		//public Guid GroupId { get; set; }
    }
}