using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Web.UI.Code.Cache
{
    public class RequestCache : IDisposable
    {
        private static Cache _cache;

        public static Cache Current => _cache ?? (_cache = new Cache(true));

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}