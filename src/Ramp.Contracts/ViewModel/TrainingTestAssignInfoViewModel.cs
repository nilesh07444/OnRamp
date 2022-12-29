using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingTestAssignInfoViewModel
    {
        public IEnumerable<Guid> AssignedUserIds { get; set; }
        public string Title { get; set; }
        public Guid Id { get; set; }
        public bool Selected { get; set; }
    }
}
