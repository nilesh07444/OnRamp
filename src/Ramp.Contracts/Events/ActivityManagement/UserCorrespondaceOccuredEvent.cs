using Common.Events;
using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events.ActivityManagement
{
    public class UserCorrespondaceOccuredEvent : IEvent
    {
        public UserCorrespondenceLogViewModel UserCorrespondenceLogViewModel { get; set; }
    }
}