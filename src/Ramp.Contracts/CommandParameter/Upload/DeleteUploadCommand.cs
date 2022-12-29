using Common.Command;

namespace Ramp.Contracts.CommandParameter.Upload {
	public class DeleteUploadCommand : ICommand
    {
        public string Id { get; set; }
        public bool? MainContext { get; set; }
    }
}