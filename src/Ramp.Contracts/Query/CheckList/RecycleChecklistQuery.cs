using System.Collections.Generic;

namespace Ramp.Contracts.Query.CheckList {
	public class RecycleChecklistQuery {
		
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
