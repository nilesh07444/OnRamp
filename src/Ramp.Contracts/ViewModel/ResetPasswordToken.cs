using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class ResetPasswordToken
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
