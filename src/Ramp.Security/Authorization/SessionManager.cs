using Common;
using Domain.Enums;
using Ramp.Contracts.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web;

namespace Ramp.Security.Authorization
{
    public static class SessionManager
    {
        public static bool Exists
        {
            get
            {
                if (HttpContext.Current == null)
                    return false;

                return HttpContext.Current.Session != null;
            }
        }

        public static void SetSessionInformation(SessionInformation sessionInformation)
        {
            if (HttpContext.Current.Session["USER_SESSION_INFORMATION"] == null)
            {
                HttpContext.Current.Session.Add("USER_SESSION_INFORMATION", sessionInformation);
            }
        }

        public static void RemoveSessionInformation()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Session["USER_SESSION_INFORMATION"] != null)
                    HttpContext.Current.Session["USER_SESSION_INFORMATION"] = null;
                if (HttpContext.Current.Session["StartTime"] != null)
                    HttpContext.Current.Session["StartTime"] = null;
                if (HttpContext.Current.Session["TestDuration"] != null)
                    HttpContext.Current.Session["TestDuration"] = null;
                if (HttpContext.Current.Session["CURRENT_TEST_ID"] != null)
                    HttpContext.Current.Session["CURRENT_TEST_ID"] = null;
            }
        }

        public static SessionInformation GetSessionInformation()
        {
            if (HttpContext.Current.Session["USER_SESSION_INFORMATION"] as SessionInformation != null)
            {
                var session = HttpContext.Current.Session["USER_SESSION_INFORMATION"] as SessionInformation;
                return session;
            }
            return null;
        }

        public static Guid GetCurrentlyLoggedInUserId()
        {
            var identity = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (identity == null)
                return Guid.Empty;
            var id = identity.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(id))
                return Guid.Empty;
            if (Guid.TryParse(id, out Guid gId))
                return gId;
            return Guid.Empty;
        }
        public static IEnumerable<string> GetRolesForCurrentlyLoggedInUser()
        {
            var identity = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (identity == null)
                return Enumerable.Empty<string>();
            return identity.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value);
        }
        public static List<UserRole> GetUserRoleOfCurrentlyLoggedInUser()
        {
            var allRoles = GetRolesForCurrentlyLoggedInUser().ToList();
            var result = new List<UserRole>();
            allRoles.ForEach(role =>
            {
                var userRole = ConvertStringToUserRole(role);
                if (userRole.HasValue)
                    result.Add(userRole.Value);
            });
            return result;
        }

        public static Guid? GetSystemId()
        {
            var identity = Thread.CurrentPrincipal as ClaimsPrincipal;
            var system = identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.System)?.Value;
            if (system == null)
                return null;
            if (Guid.TryParse(system, out var id))
                return id;
            return null;
        }
        public static List<string> GetCustomerRolesOfCurrentlyLoggedInUser()
        {
            return GetRolesForCurrentlyLoggedInUser().Where(role => role != Role.Admin && role != Role.Reseller).ToList();
        }

        public static void SetCustomerSurveyVisibility(bool show)
        {
            var sessionInfo = GetSessionInformation();
            if (sessionInfo != null)
                sessionInfo.ShowSurveyModel = show;
        }
        public static string ConvertUserRoleToString(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin:
                    return Role.Admin;

                case UserRole.Reseller:
                    return Role.Reseller;

                case UserRole.CustomerAdmin:
                    return Role.CustomerAdmin;

                case UserRole.CustomerStandardUser:
                    return Role.StandardUser;
            }
            return null;
        }
        public static UserRole? ConvertStringToUserRole(string role)
        {
            switch (role)
            {
                case Role.Admin:
                    return UserRole.Admin;
                case Role.Reseller:
                    return UserRole.Reseller;
                case Role.CustomerAdmin:
                    return UserRole.CustomerAdmin;
                case Role.StandardUser:
                    return UserRole.CustomerStandardUser;
            }
            return new UserRole?();
        }
    }
}