using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
   public class TrainingGuideusageStats : Base.CustomerDomainObject
    {
       public Guid TrainingGuidId { get; set; }  
       public Guid UserId { get; set; }
       public DateTime ViewDate { get; set; }
       public bool Unassigned { get; set; }
    }
}
