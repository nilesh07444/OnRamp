using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.Settings
{
    public class SettingCommandParameter : ICommand
    {
        public Guid Id { get; set; }
        public int PasswordPolicy { get; set; }
    }
}
