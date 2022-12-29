using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.CustomDocument {
	public class RecycleCustomDocumentQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
