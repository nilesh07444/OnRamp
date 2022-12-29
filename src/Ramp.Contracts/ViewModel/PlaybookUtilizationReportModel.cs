using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class PlaybookUtilizationReportModel { 
        public string GuideTitle { get; set; }
        public IEnumerable<Entry> Entries = new List<Entry>();
        public class Entry
        {
            public string GroupName { get; set; }
            public IEnumerable<EntryDetail> Details { get; set; } = new List<EntryDetail>();
            public class EntryDetail
            {
                public string FullName { get; set; }
                public string EmployeeNumber { get; set; }
                public string IDNumber { get; set; }
                public bool Interacted { get; set; }
                public bool YetToInteract { get; set; }
                public decimal? PassedTest { get; set; }
                public decimal? FailedTest { get; set; }
                public int TestVersion { get; set; }

            }
        }
    }
}
