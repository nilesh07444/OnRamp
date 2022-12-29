using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class CustomerSelfSignUpQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }
    }
}
