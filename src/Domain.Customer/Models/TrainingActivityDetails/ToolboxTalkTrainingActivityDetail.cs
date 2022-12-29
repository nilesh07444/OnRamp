using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class ToolboxTalkTrainingActivityDetail : Base.CustomerDomainObject
    {
        public virtual IList<StandardUser> ConductedBy { get; set; } = new List<StandardUser>();

    }
}
