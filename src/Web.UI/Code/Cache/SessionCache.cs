using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Cache
{
    public class SessionCache
    {
        private static Cache _cache;

        public static Cache Current => _cache ?? (_cache = new Cache());
    }
}