using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class PrepareTestExpiryNotificationCommand
    {
        public Guid TestId { get; set; }
        public Company Company { get; set; }
        public IEnumerable<TestAssigned> TestsAssigned { get; set; } = new List<TestAssigned>();
        public IEnumerable<StandardUser> Users { get; set; } = new List<StandardUser>();
        public IEnumerable<TrainingTest> Tests { get; set; } = new List<TrainingTest>();
        public IEnumerable<StandardUserCorrespondanceLog> CorrespondanceLogs { get; set; } = new List<StandardUserCorrespondanceLog>();
        public IEnumerable<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
