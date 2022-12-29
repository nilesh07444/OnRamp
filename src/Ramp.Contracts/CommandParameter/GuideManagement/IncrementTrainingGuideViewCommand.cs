﻿using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class IncrementTrainingGuideViewCommand : ICommand
    {
        public Guid TrainingGuidId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ViewDate { get; set; }
        //public int GuideStatsCount { get; set; }
    }
}