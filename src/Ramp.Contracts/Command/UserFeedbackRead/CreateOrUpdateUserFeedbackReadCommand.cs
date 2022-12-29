using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.UserFeedbackRead
{
    public class CreateOrUpdateUserFeedbackReadCommand : IdentityModel<string>
    {
        public DateTimeOffset Created { get; set; }
        public string UserFeedbackId { get; set; }
        public string UserId { get; set; }
    }
}
