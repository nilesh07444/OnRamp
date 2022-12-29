using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ramp.Security.Authorization
{
    public static class RampSecurity
    {
        public static Dictionary<string, string[]> RampSecurityRoles;

        public static void SetRampSecurity(string key, params string[] roles)
        {
            if (RampSecurityRoles == null)
                RampSecurityRoles = new Dictionary<string, string[]>();
			RampSecurityRoles.Add(key.ToLower(), roles);
		}

        public static string MakeKey(string area, string controller, string action)
        {
            StringBuilder result = new StringBuilder();

            if (!string.IsNullOrEmpty(area))
                result.Append(area + ",");
            result.Append(controller);
            if (!string.IsNullOrEmpty(action))
                result.Append("," + action);

            return result.ToString();
        }

        public static string findAreaForControllerType(Type controllerType)
        {
            var areaTypes = getAllAreasRegistered();

            foreach (var areaType in areaTypes)
            {
                if (controllerType.Namespace.StartsWith(areaType.Namespace + "."))
                {
                    var area = (AreaRegistration)Activator.CreateInstance(areaType);
                    return area.AreaName;
                }
            }
            return string.Empty;
        }

        private static IEnumerable<Type> getAllAreasRegistered()
        {
            var assembly = getWebEntryAssembly();
            var areaTypes =
                assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(AreaRegistration)))
                    .OrderByDescending(c => c.FullName);
            return areaTypes;
        }

        private static Assembly getWebEntryAssembly()
        {
            if (System.Web.HttpContext.Current == null ||
                System.Web.HttpContext.Current.ApplicationInstance == null)
            {
                return null;
            }

            var type = System.Web.HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }

            return type == null ? null : type.Assembly;
        }
    }
}