using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using SimpleInjector.Integration.Web;

namespace Web.UI.Code.Decorators
{
    public class ScopedQueryHandlerDecorator<T, R> : IQueryHandler<T, R>
    {
        private readonly Func<IQueryHandler<T, R>> _handlerFactory;
        private readonly Container _container;

        public ScopedQueryHandlerDecorator(Container container, Func<IQueryHandler<T, R>> handlerFactory)
        {
            _container = container;
            _handlerFactory = handlerFactory;
        }
        public R ExecuteQuery(T query)
        {
            if(_container.Options.DefaultScopedLifestyle.GetCurrentScope(_container) == null)
                using (AsyncScopedLifestyle.BeginScope(_container))
                {
                    return _handlerFactory().ExecuteQuery(query);
                }
            else
                return _handlerFactory().ExecuteQuery(query);
        }
    }
}