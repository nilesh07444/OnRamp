using Common.Query;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class EmailExistQueryParameter : IQuery
    {
        public string Email { get; set; }
        public PortalContextViewModel PortContext { get; set; }
    }
}