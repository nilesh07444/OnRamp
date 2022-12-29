using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;

namespace Domain.Customer.Models
{
    public class Certificate : IdentityModel<string>
    {
        public virtual Upload Upload { get; set; }
        public string UploadId { get; set; }
		public string Title { get; set; }
		public DateTime CreatedOn { get; set; }
		public string Type { get; set; }
	}
}
