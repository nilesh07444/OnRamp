using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.User
{
    public class UserSearchQueryParameter: IQuery
    {
        public string Email { get; set; }
    }
}
