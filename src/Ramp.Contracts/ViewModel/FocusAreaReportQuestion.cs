using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class FocusAreaReportQuestion
    {
        public string TestTitle { get; set; }
        public string Question { get; set; }
        public string Id { get; set; }
        public int Rank { get; set; }
        public IEnumerable<FocusAreaReportOption> Options { get; set; }
        public int IncorrectAnswers { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
