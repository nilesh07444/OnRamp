using System;
using Common.Command;
using Domain.Models;

namespace Ramp.Contracts.CommandParameter.ActivityManagement
{
    public class AddUserActivityCommand : ICommand
    {
        public Guid CurrentUserId { get; set; }

        public string ActivityDescription { get; set; }

        public DateTime ActivityDate { get; set; }

        public virtual UserActivityEnum ActivityType { get; set; }
    }
}