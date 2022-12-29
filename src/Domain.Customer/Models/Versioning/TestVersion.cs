using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class TestVersion : Base.CustomerDomainObject, IVersion<TrainingTest, Guid>
    {
        public virtual TrainingGuide TrainingGuide { get; set; }
        public virtual TrainingTest CurrentVersion { get; set; }
        public virtual TrainingTest LastPublishedVersion { get; set; }
        public virtual IList<TrainingTest> Versions { get; set; } = new List<TrainingTest>();
    }
}
