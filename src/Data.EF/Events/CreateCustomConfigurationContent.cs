using Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.EF.Events
{
    public class CreateCustomConfigurationContent : IEvent
    {
        public Guid CompanyId { get; set; }
    }
}