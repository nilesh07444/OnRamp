using Domain.Customer.Models;
using Domain.Enums;
using Ramp.Contracts.Security;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace Ramp.Security.Attributes {
	public class RampSecurityAttribute : AuthorizeAttribute {
		public override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
				return;

			if (filterContext.ActionDescriptor != null) {
				var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
				var userRoles = Enumerable.Empty<string>();
				if (claimsPrincipal != null)
					userRoles = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
				if (userRoles.Count() > 0) {
					bool allowed = false;
					string action = filterContext.ActionDescriptor.ActionName;
					string controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerType.Name;
					string area = RampSecurity.findAreaForControllerType(filterContext.ActionDescriptor.ControllerDescriptor.ControllerType);

					if (RampSecurity.RampSecurityRoles.ContainsKey(RampSecurity.MakeKey(area, controller, action).ToLower())) {
						var authorizedRoles =
							RampSecurity.RampSecurityRoles[RampSecurity.MakeKey(area, controller, action).ToLower()];

						foreach (var userRole in userRoles) {
							if (authorizedRoles.Any(r => r.Equals(userRole)))
								allowed = true;
						}
						if (!allowed) {
							HttpContext.Current.Response.Redirect("~/Account/AccessDenied");
						}
					}
				} else {
					HttpContext.Current.Response.Redirect("~/Account/Login?returnUrl=/" + HttpContext.Current?.Request?.UrlReferrer?.PathAndQuery, true);
				}
			}
		}
	}
}