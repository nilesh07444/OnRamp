using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Group {
	public class StandardUserGroupUpdateParentCommand {
		public string Id { get; set; }
		public string ParentId { get; set; }
	}
}
