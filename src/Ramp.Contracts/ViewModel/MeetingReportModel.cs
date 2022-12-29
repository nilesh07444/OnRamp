using System;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel {
public	class MeetingReportModel {

		public SelectList MeetingRoom { get; set; }
		public string Id { get; set; }
		public SelectList StatusRoom { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

	}
}
