using Domain.Enums;
using System;
using System.ComponentModel;

namespace Domain.Models
{
    public class UserCorrespondenceLog : DomainObject
    {
        public virtual UserCorrespondenceEnum CorrespondenceType { get; set; }
        public string Description { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime CorrespondenceDate { get; set; }
        public string Content { get; set; }
    }

    public enum UserCorrespondenceEnum
    {
        [Description("received an email")]
        Email = 0,

        [Description("received an SMS")]
        Sms = 1,
    }
}