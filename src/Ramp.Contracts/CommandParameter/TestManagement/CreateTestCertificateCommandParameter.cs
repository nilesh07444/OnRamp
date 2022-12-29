using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class CreateTestCertificateCommandParameter : ICommand
    {
        public Guid UserId { get; set; }
        public TestResultViewModel Result { get; set; }
        public Guid ResultId { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public bool Migration { get; set; }
        public Guid Id { get; set; }
    }
}