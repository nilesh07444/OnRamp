using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Settings
{
    public class CreateDefaultContentCommandParameter : ICommand
    {
        public CreateDefaultContentCommandParameter()
        {
            TrophyPaths = new List<string>();
        }

        public string CertificatePath { get; set; }
        public List<string> TrophyPaths { get; set; }
        public string CSSPath { get; set; }
    }
}