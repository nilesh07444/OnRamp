using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Course {
	public class UsersAssignedCourseQuery {
		public string DocumentId { get; set; }
		public DocumentType DocumentType { get; set; }
		public string[] GroupIds { get; set; }
		public string[] AllDocumentId { get; set; }
	}
}
