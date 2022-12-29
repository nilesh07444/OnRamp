using System.Collections.Generic;

namespace Ramp.Contracts.Query.AcrobatField {
	public	class RecycleAcrobatFieldQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
