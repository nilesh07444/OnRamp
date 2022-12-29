using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models {
	public class AutoAssignDocuments: Base.CustomerDomainObject {

		//public Guid Id { get; set; }
		public Guid WorkFlowId { get; set; }
		public Guid DocumentId { get; set; }
		public string Title { get; set; }
		public int Type { get; set; }
		public int Order { get; set; }
	}
}
