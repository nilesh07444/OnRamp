using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Http;
using VirtuaCon;

namespace Web.UI.Controllers
{
    public class DefaultController : RampController
    {
        public string GetGenerateId()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Guid.NewGuid().ToString();
        }
        public IDictionary GetEnumFriendlyNames(string typeName)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
                return EnumUtility.GetFriendlyNamesDictionary(Type.GetType(typeName));
            return null;
        }
    }
}