using Domain.Models;
using System;

namespace Ramp.Contracts.ViewModel
{
    public class UserActivityLogViewModel
    {
        public Guid ActivityId { get; set; }
        public string ActivityType { get; set; }
        public String Description { get; set; }
        public Guid UserId { get; set; }
        public UserViewModel User { get; set; }
        public DateTime ActivityDate { get; set; }
    }
}