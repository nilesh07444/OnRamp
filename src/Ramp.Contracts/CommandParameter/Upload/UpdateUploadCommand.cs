using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Upload
{
    public class UpdateUploadCommand  : ICommand
    {
        public UploadModel Model { get; set; }
        public bool? MainContext { get; set; }
    }
	public class UploadLogTrainingCommand : ICommand {
		public UploadModel Model { get; set; }
		public bool? MainContext { get; set; }
	}
}
