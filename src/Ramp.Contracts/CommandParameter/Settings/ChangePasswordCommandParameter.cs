using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.Settings
{
    public class ChangePasswordCommandParameter : ICommand
    {
        public Guid Id { get; set; }
        public string Password { get; set; }
    }
}
