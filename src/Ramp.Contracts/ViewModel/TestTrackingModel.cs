using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TestTrackingModel
    {
        public string Id { get; set; }
        public DateTimeOffset CompletedTime { get; set; }
        public DateTime Started { get; set; }
        public bool Completed { get; set; }
    }
}
