using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class CalendarViewModel {

		public string id { get; set; }
		public string assignedById { get; set; }
		public string createdById { get; set; }
		public int type { get; set; }
		public DateTime assignedDate { get; set; }

		public string title { get; set; }
		public string start { get; set; }
		public string end { get; set; }

		public string userName { get; set; }
		public string assignedBy { get; set; }
		public string additionalMessage { get; set; }

		public string className { get; set; }
	}

	public class CalendarListViewModel {
		public string Date { get; set; }
		public List<CalendarViewModel> Docs { get; set; }
	}

	public class CalendarData {
		public List<CalendarListViewModel> Data { get; set; }
	}
}
