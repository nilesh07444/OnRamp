using System.Collections.Generic;

namespace Web.UI.Models {
	public class ChartViewModel {
		public List<int> Categories { get; set; }
		public List<string> Name { get; set; }
		public List<string> Type { get; set; }
		public List<int> Count { get; set; }
		public List<string> Status { get; set; }
		public List<int> StatusCount { get; set; }
	}
	
}