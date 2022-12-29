using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models {
	public class AssociatedDocument : Base.CustomerDomainObject {

		//public Guid Id { get; set; }
		public Guid CourseId { get; set; }
		public Guid DocumentId { get; set; }
		public string Title { get; set; }
		public DocumentType Type { get; set; }
		public int OrderNo { get; set; }
	}
}
