using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TrophyCabinet
{
    public class AllTrophiesByStandardUserQuery : IQuery
    {
        public Guid UserId { get; set; }
        public string DefaultTrophyPath { get; set; }
    }
}