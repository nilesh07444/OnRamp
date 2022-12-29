using Common.Query;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class TestCertificateForResultQueryParameter : IQuery
    {
        public Guid UserId { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public TestResultViewModel ResultVm { get; set; }
        public string BasePreviewPath { get; set; }
        public string DefaultCertPath { get; set; }
        public Guid ResultId { get; set; }
    }
}