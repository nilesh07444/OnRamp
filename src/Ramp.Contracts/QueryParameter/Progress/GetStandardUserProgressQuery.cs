﻿using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Progress
{
    public class GetStandardUserProgressQuery : IQuery
    {
        public Guid UserId { get; set; }
    }
}
