using System;

namespace Domain.Models
{
    public class UserLoginStats : DomainObject
    {
        public virtual Guid LoggedInUserId { get; set; }
        public virtual User LoggedInUser { get; set; }
        public DateTime LogInTime { get; set; }
        public DateTime? LogOutTime { get; set; }
    }
}