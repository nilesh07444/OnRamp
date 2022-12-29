
using System.Collections.Generic;

namespace Ramp.Contracts.Query.Course {
	public class RecycleCourseQuery {
		public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
	}
}
