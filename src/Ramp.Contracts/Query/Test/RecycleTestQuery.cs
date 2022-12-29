using System.Collections.Generic;

namespace Ramp.Contracts.Query.Test {
	public class RecycleTestQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
