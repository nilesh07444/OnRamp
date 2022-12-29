using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.UI.Code;

namespace Web.UI
{
	public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, ce, ca, p) => true;

            app.CreatePerOwinContext<OnRampUserManager>(OnRampUserManager.Create);
            app.CreatePerOwinContext<OnRampSignInManager>(OnRampSignInManager.Create);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromDays(7)
            });
        }
    }
}