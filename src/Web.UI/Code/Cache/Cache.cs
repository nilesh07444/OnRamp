using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebGrease.Css.Extensions;

namespace Web.UI.Code.Cache
{
    public class Cache : IDisposable
    {
        [ThreadStatic]
        private static Dictionary<string, object> _threadStaticCache;

        private readonly bool _request;

     
        public Cache(bool request = false)
        {
            _request = request;
            if (_threadStaticCache == null)
                _threadStaticCache = new Dictionary<string, object>();
        }

        private IDictionary<string, object> GetSessionCache(HttpContext context)
        {
            if (context.Session == null)
                return new Dictionary<string, object>();

            if (context.Session["SessionCache"] == null)
                context.Session["SessionCache"] = new Dictionary<string, object>();

            return (IDictionary<string, object>)context.Session["SessionCache"];
        }

        private IDictionary<string, object> GetRequestCache(HttpContext context)
        {
            if (context.Items["RequestCache"] == null)
            {
                context.Items["RequestCache"] = new Dictionary<string, object>();
            }

            return (IDictionary<string, object>)context.Items["RequestCache"];
        }

        private IDictionary<string, object> CacheObject
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    if (_request)
                        return GetRequestCache(HttpContext.Current);
                    return GetSessionCache(HttpContext.Current);
                }
                if (_threadStaticCache == null)
                    _threadStaticCache = new Dictionary<string, object>();

                return _threadStaticCache;
            }
        }

        private static string MakeKey(string desc, string key)
        {
			var keyString = string.Concat(desc, key);

			return keyString;
        }

        public void Set<T>(string description, string key, T @object)
        {
            if (!CacheObject.ContainsKey(MakeKey(description, key)))
                CacheObject[MakeKey(description, key)] = @object;
        }

        public void Override<T>(string description, string key, T newObject)
        {
            if (CacheObject.ContainsKey(MakeKey(description, key)))
                CacheObject[MakeKey(description, key)] = newObject;
            else
            {
                Set(description, key, newObject);
            }
        }

        public void onStartAsyc(HttpContext context)
        {
            _threadStaticCache = new Dictionary<string, object>(GetSessionCache(context));
        }

        public void onStartTask(object state)
        {
            _threadStaticCache = new Dictionary<string, object>(GetSessionCache((System.Web.HttpContext)state));
        }

        public T Get<T>(string description, string key) where T : class
        {
            if (CacheObject.ContainsKey(MakeKey(description, key)))
            {
                return CacheObject[MakeKey(description, key)] as T;
            }

            return default(T);
        }

        public void Dispose()
        {
            CacheObject.Values.ForEach(delegate (object o)
            {
                var disposible = o as IDisposable;
                disposible?.Dispose();
            });
            CacheObject.Clear();
        }

        public void Clear()
        {
            CacheObject.Clear();
        }
    }
}