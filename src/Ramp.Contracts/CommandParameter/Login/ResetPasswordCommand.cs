using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.Login
{
    public class ResetPasswordCommand : ICommand
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Guid Id { get; set; }
        public string Token { get; set; }
    }
}