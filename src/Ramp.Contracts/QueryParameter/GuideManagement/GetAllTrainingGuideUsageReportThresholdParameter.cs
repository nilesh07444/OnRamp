﻿using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class GetAllTrainingGuideUsageReportThresholdParameter : IQuery
    {
        public Guid CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime LastDate { get; set; }
    }
}
