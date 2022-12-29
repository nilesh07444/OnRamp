using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Settings
{
    public class ClearCustomConfigurationCommadParameter : ICommand
    {
        public bool Certificate { get; set; }
        public bool CSS { get; set; }
        public bool LoginLogo { get; set; }
        public bool DashboardLogo { get; set; }
    }
}