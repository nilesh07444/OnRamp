
using System.Collections.Generic;

namespace Ramp.Contracts.Query.Policy {
	public class RecyclePolicyQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
