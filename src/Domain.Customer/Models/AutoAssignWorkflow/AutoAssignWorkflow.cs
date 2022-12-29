using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
	public class AutoAssignWorkflow : Base.CustomerDomainObject {

		//public Guid Id { get; set; }
		public string WorkflowName { get; set; }
		public DateTime DateCreated { get; set; }
		public bool IsDeleted { get; set; }
		public bool SendNotiEnabled { get; set; }
	}
}
