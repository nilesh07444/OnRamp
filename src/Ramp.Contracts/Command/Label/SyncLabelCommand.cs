using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.Label
{
    public class SyncLabelCommand
    {
        public IEnumerable<string> Values { get; set; } = new List<string>();
    }
}
