using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class AssignViewModel
    {
        public AssignMode AssignMode { get; set; }
        public Guid Id { get; set; }
        public bool? Playbook { get; set; }
        public bool? Test { get; set; }
        public bool ForceRetake { get; set; }
    }
}
