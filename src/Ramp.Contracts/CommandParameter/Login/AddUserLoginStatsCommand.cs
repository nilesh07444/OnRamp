using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.Login
{
    public class AddUserLoginStatsCommand : ICommand
    {
        public Guid UserLoginStatsId { get; set; }
        public Guid LoggedInUserId { get; set; }
        public bool StandardUser { get; set; }
        public DateTime LogInTime { get; set; }
        public DateTime LogOutTime { get; set; }
        public bool IsUserLoggedIn { get; set; }
    }
}