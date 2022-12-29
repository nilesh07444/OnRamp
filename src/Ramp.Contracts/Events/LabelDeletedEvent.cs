using Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events
{
    public class LabelDeletedEvent :  IEvent
    {
        public string Name { get; set; }
    }
}
