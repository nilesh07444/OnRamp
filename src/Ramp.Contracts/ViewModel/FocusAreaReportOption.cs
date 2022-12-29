using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class FocusAreaReportOption
    {
        public string Option { get; set; }
        public string Id { get; set; }
        public int Count { get; set; }
        public bool Correct { get; set; }
        public int Rank { get; set; }
    }
}
