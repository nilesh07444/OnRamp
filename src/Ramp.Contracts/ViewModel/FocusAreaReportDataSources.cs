using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
   public class FocusAreaReportDataSources
    {
        public IEnumerable<FocusAreaReportQuestion> Questions { get; set; }
        public IEnumerable<FocusAreaReportTest> Test { get; set; }
        public IEnumerable<SerializableSelectListItem> TestDropDown { get; set; }
        public string SelectedTestId { get; set; }
    }
}
