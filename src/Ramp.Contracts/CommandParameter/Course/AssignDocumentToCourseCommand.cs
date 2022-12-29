using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Course {
	public class AssignDocumentToCourseCommand {

		public Guid Id { get; set; }
		public Guid CourseId { get; set; }
		public Guid DocumentId { get; set; }
		public string Title { get; set; }
		public DocumentType Type { get; set; }
		public int OrderNo { get; set; }
	}
}
