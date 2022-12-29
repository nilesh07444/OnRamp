using Domain.Customer.Models;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class AllTestsThatRequiresExipryNotificationQuery
    {
        public NotificationInterval NotificationInterval { get; set; }
        public int? ArbitraryTestExpiryIntervalInDaysBefore { get; set; }
        public IEnumerable<TrainingTest> Tests { get; set; } = new List<TrainingTest>();
    }
}
