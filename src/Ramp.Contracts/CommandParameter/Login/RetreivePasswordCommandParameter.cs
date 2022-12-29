using Common.Command;

namespace Ramp.Contracts.CommandParameter.Login
{
    class RetreivePasswordCommandParameter
    {
        public RetreivePasswordCommandParameter(string email)
        {
            Email = email;
        }

        public string Email { get; private set; }
    }
}
