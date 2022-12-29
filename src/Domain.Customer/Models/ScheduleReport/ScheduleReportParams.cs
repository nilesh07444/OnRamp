using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.ScheduleReport {
	public class ScheduleReportParams : Base.ScheduleReportObject {
		public Guid? Id { get; set; }
		public Guid? ReportId { get; set; }
		public string ParameterName { get; set; }
		public string ParameterValuess { get; set; }
	}
}
