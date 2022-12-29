using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class CustomerSurveyDetail : Base.CustomerDomainObject
    {
        public Guid UserId { get; set; }
        public virtual StandardUser User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime RatedOn { get; set; }
        public string Browser { get; set; }
		public string Category { get; set; }
    }
}