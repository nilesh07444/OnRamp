using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Groups {
	class StandardUserGroups : Base.CustomerDomainObject {
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid GroupId { get; set; }
		public DateTime? DateCreated { get; set; }
	}
}
