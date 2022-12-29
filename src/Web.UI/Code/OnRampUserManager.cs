using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using Common.Data;
using Domain.Models;
using Domain.Customer.Models;
using System.Web.Mvc;
using Ramp.Services.Helpers;

namespace Web.UI.Code
{
    public class OnRampUserManager : UserManager<OnRampUserModel>
    {
        public OnRampUserManager(IUserStore<OnRampUserModel> store) : base(store)
        {
        }
        public static OnRampUserManager Create(IdentityFactoryOptions<OnRampUserManager> options, IOwinContext context)
        {
            var manager = new OnRampUserManager(new OnRampUserStore());
            return manager;
        }
        public override Task<ClaimsIdentity> CreateIdentityAsync(OnRampUserModel user, string authenticationType)
        {
            return base.CreateIdentityAsync(user, authenticationType);
        }
        public override Task<bool> CheckPasswordAsync(OnRampUserModel user, string password)
        {
            var encryptor = new EncryptionHelper();
            if (HttpContext.Current.Session["LDAP_xb"] != null && Boolean.Parse(HttpContext.Current.Session["LDAP_xb"].ToString()))
            {
                return Task.FromResult(Boolean.Parse(HttpContext.Current.Session["LDAP_xb"].ToString()));
            }
            else
            {
                return Task.FromResult(encryptor.Decrypt(user.Model.Password) == password);
            }
        }
    }

}