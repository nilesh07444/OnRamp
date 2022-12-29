using Common.Query;
using Data.EF.Customer;
using Ramp.Contracts.QueryParameter.Portal;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using SimpleInjector.Lifestyles;
using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Web.UI.App_Start;
using Web.UI.Code.Cache;
using Web.UI.Code.Extensions;
using static Web.UI.Code.Cache.RequestCache;

namespace Web.UI
{
    public static class PortalContext
    {
        public static PortalContextViewModel Current
        {
            get => SessionCache.Current.Get<PortalContextViewModel>("UI", "PortalContext") == null
                ? Override(Thread.CurrentPrincipal.GetCompanyId())
                : SessionCache.Current.Get<PortalContextViewModel>("UI", "PortalContext");
            set => SessionCache.Current.Set("UI", "PortalContext", value);
        }

        public static PortalContextViewModel Override(Guid? customerCompanyId)
        {
            if (!customerCompanyId.HasValue || (customerCompanyId.HasValue && customerCompanyId.Value.Equals(Guid.Empty)))
                return null;
            var queryExecutor = DependencyResolver.Current.GetService<IQueryExecutor>();
            var portalContext =
                queryExecutor.Execute<PortalQueryParameter, PortalContextViewModel>(new PortalQueryParameter
                {
                    CompanyId = customerCompanyId,
                    FromOverride = true
                });
            SessionCache.Current.Override("UI", "PortalContext", portalContext);
            return portalContext;
        }
    }
}