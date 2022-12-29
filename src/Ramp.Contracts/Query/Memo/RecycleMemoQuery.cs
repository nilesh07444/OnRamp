using System.Collections.Generic;

namespace Ramp.Contracts.Query.Memo {
	public	class RecycleMemoQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
