using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.Login
{
    public class LoginQueryParameters : IQuery
    {
        
        public string ReturnUrl { get; private set; }
        public Guid LoggedInUserId { get; set; }
        public bool IsUserLoggedIn { get; set; }
    }
}