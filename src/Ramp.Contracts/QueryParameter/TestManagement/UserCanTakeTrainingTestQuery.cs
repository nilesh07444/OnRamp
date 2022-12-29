using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
   public  class UserCanTakeTrainingTestQuery
    {
        public Guid? UserId { get; set; }
        public Guid TestId { get; set; }
    }
}
