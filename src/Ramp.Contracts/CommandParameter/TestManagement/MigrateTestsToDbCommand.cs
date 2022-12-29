using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class MigrateTestsToDbCommand : ICommand
    {
        public string PathToSaveUploadedFiles { get; set; }
        public string CompanyName { get; set; }
    }
}