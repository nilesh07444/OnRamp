using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class CustomerGroup : Base.CustomerDomainObject
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public virtual Guid CompanyId { get; set; }
        public bool IsforSelfSignUpGroup { get; set; }
		public string ParentId { get; set; }
		public DateTime? DateCreated { get; set; }
										   //public virtual List<StandardUser> UserList { get; set; }
	}
}