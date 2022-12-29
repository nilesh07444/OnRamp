using Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Web.UI.Code.Decorators
{

    public class ScopedEventHandlerDecorator<T> : IEventHandler<T> where T : IEvent
    {
        private readonly Func<IEventHandler<T>> _handlerFactory;
        private readonly Container _container;

        public ScopedEventHandlerDecorator(Container container, Func<IEventHandler<T>> handlerFactory)
        {
            _container = container;
            _handlerFactory = handlerFactory;
        }

        public void Handle(T raisedEvent)
        {
            if (_container.Options.DefaultScopedLifestyle.GetCurrentScope(_container) == null)
                using (AsyncScopedLifestyle.BeginScope(_container))
                {
                    _handlerFactory().Handle(raisedEvent);
                }
            else
                _handlerFactory().Handle(raisedEvent);
        }
    }
}