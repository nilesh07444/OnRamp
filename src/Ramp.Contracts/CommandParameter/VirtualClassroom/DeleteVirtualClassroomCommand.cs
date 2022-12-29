using Common.Command;


namespace Ramp.Contracts.CommandParameter.VirtualClassroom {
public	class DeleteVirtualClassroomCommand : ICommand {
		public string DocumentId { get; set; }
	}
}
