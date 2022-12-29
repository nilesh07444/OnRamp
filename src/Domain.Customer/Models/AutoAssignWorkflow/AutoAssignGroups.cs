using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models {
	public class AutoAssignGroups : Base.CustomerDomainObject {
		//public Guid Id { get; set; }
		public Guid WorkFlowId { get; set; }
		public Guid GroupId { get; set; }
	}
}
