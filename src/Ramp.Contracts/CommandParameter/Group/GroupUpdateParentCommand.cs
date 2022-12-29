using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Group {
	public class GroupUpdateParentCommand {
		public string Id { get; set; }
		public string ParentId { get; set; }
	}
}
