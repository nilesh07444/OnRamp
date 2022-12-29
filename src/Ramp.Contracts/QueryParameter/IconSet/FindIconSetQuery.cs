using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.IconSet
{
    public class FindIconSetQuery : IQuery
    {
        public string Id { get; set; }
    }
}
