﻿using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.ActivityManagement
{
    public class UpdateActivityLogDescriptionsCommand : ICommand
    {
        public bool UpdateActivityIdentity { get; set; }
    }
}