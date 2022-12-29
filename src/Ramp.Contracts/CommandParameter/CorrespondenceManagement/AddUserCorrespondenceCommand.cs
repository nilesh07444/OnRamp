using Common.Command;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using System;

namespace Ramp.Contracts.CommandParameter.CorrespondenceManagement
{
    public class AddUserCorrespondenceCommand : ICommand
    {
        public Guid CurrentUserId { get; set; }

        public string CorrespondenceDescription { get; set; }

        public DateTime CorrespondenceDate { get; set; }

        public virtual UserCorrespondenceEnum CorrespondenceType { get; set; }
        public string Content { get; set; }
    }
}