using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models {
	public class AssignedCourse : Base.CustomerDomainObject {
		//public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid CourseId { get; set; }
		//public DocumentType DocumentType { get; set; }
		public Guid AssignedBy { get; set; }
		public string AdditionalMsg { get; set; }
		public DateTime AssignedDate { get; set; }
		public bool Deleted { get; set; }
		public bool IsRecurring { get; set; }
		//public int OrderNumber { get; set; }
	}
}
