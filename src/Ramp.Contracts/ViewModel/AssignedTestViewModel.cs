using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class AssignedTestViewModel
    {
        public Guid TestId { get; set; }
        public Guid UserId { get; set; }
    }
}