using System.Collections.Generic;

namespace Ramp.Contracts.Query.TrainingManual {
	public class RecycleManualQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
