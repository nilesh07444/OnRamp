using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TestHistoryReportViewModel
    {
        public IEnumerable<TrainingGuideViewModel> TrainingGuides { get; set; }
        public IEnumerable<UserViewModel> Users { get; set; }
        public IEnumerable<GroupViewModel> Groups { get; set; }
        public IEnumerable<CompanyViewModel> Companies { get; set; }

        public readonly TestHistorySummaryData Data = new TestHistorySummaryData();

        public class TestHistorySummaryData
        {
            public readonly IList<TestHistorySummaryDataItem> Items = new List<TestHistorySummaryDataItem>();
        }

        public class TestHistorySummaryDataItem
        {
            public DateTime Date { get; set; }
            public UserDetail User { get; set; }
            public double Result { get; set; }
            public double MaxResult { get; set; }
            public double Percentage { get; set; }
            public bool Passed { get; set; }
            public string TestName { get; set; }
            public string PlaybookName { get; set; }
            public double MarksObtain { get; set; }
            public int Version { get; set; }
        }

        public class UserDetail : IdentityModel<Guid>
        {
            public string Name { get; set; }
            public string EmployeeNo { get; set; }
            public string GroupName { get; set; }
        }
    }
}