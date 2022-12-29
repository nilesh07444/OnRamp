using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Web.UI.Code
{
    public class OnRampSignInManager : SignInManager<OnRampUserModel, string>
    {
        public OnRampSignInManager(OnRampUserManager userManager, IAuthenticationManager authenticationManager)
                : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(OnRampUserModel user)
        {
            return user.GenerateUserIdentityAsync((OnRampUserManager)UserManager);
        }

        public static OnRampSignInManager Create(IdentityFactoryOptions<OnRampSignInManager> options, IOwinContext context)
        {
            return new OnRampSignInManager(context.GetUserManager<OnRampUserManager>(), context.Authentication);
        }
    }
}