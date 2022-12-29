using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Test
{
    public class TestQuestionAnswer : IdentityModel<string>
    {
        public string Option { get; set; }
        public int Number { get; set; }
        public bool Deleted { get; set; }
        public virtual TestQuestion TestQuestion { get; set; }
        public string TestQuestionId { get; set; }
    }
}
