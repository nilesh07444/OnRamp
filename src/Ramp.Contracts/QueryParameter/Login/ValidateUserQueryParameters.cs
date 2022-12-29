using Common.Query;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.QueryParameter.Login
{
    public class ValidateUserQueryParameters : IQuery
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}