using Ramp.Contracts.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Web.UI.Code.Extensions
{
    public static class Extensions
    {
        public static void AddUnique<T>(this List<T> list, T newEntry) where T : class
        {
            if (!list.Contains(newEntry))
            {
                list.Add(newEntry);
            }
        }

        public static void AddRangeUnique<T>(this List<T> list, T[] entries) where T : class
        {
            foreach (var entry in entries)
            {
                list.AddUnique(entry);
            }
        }
        public static IEnumerable<string> GetRoles(this IPrincipal principal)
        {
            var claims = principal.GetClaims();
            if (!claims.Any())
                return Enumerable.Empty<string>();
            return claims.Where(c => c.Type == ClaimTypes.Role && c.Value != null).Select(c => c.Value);
        }
        public static bool IsInGlobalAdminRole(this IPrincipal principal)
        {
            return Role.IsInGlobalAdminRole(principal.GetRoles().ToList());
        }
        public static bool IsInResellerRole(this IPrincipal principal)
        {
            return Role.IsInResellerRole(principal.GetRoles().ToList());
        }
        public static bool IsInAdminRole(this IPrincipal principal)
        {
            return Role.IsInAdminRole(principal.GetRoles().ToList());
        }
		
        public static bool IsInStandardUserRole(this IPrincipal principal)
        {
            return Role.IsInStandardUserRole(principal.GetRoles().ToList());
        }
        public static Guid GetId(this IPrincipal principal)
        {
            var claims = principal.GetClaims();
            if (claims.Any())
            {
                var id = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out Guid gId))
                    return gId;
            }
            return Guid.Empty;
        }
        public static Guid GetStatsId(this IPrincipal principal)
        {
            var claims = principal.GetClaims();
            if (claims.Any())
            {
                var id = claims.FirstOrDefault(claim => claim.Type == OnrampClaimTypes.StatsIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out Guid gId))
                    return gId;
            }
            return Guid.Empty;
        }
        public static IEnumerable<Claim> GetClaims(this IPrincipal principal)
        {
            var claimPrincipal = principal as ClaimsPrincipal;
            if (claimPrincipal == null)
                return Enumerable.Empty<Claim>();
            return claimPrincipal.Claims;
        }
        public static string GetGivenName(this IPrincipal principal)
        {
            return principal.GetClaims().FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        }
        public static Guid GetCompanyId(this IPrincipal principal)
        {
            var claims = principal.GetClaims();
            if (claims.Any())
            {
                var id = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.System)?.Value;
                if (!string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out Guid gId))
                    return gId;
            }
            return Guid.Empty;
        }
        public static bool IsUserInRole(this IPrincipal principal,string role)
        {
            return principal.GetRoles().Any(r => r == role);
        }
    }
}