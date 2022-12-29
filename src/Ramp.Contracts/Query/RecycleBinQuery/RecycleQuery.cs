
using System.Collections.Generic;

namespace Ramp.Contracts.Query.RecycleBinQuery {
	public class RecycleQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
