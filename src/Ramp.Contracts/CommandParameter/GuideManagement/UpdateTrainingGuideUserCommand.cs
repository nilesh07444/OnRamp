﻿using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class UpdateTrainingGuideUserCommand : ICommand
    {
        public Guid PreviousUserId { get; set; }
    }
}