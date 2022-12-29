using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Data.EF.Customer;

namespace Web.UI.Code.Decorators
{
    public class ScopedCommandHandlerDecorator<T> : ICommandHandlerBase<T>
    {
        private readonly Func<ICommandHandlerBase<T>> _handlerFactory;
        private readonly Container _container;

        public ScopedCommandHandlerDecorator(Container container, Func<ICommandHandlerBase<T>> handlerFactory)
        {
            _container = container;
            _handlerFactory = handlerFactory;
        }

        public CommandResponse Execute(T command)
        {
            if (_container.Options.DefaultScopedLifestyle.GetCurrentScope(_container) == null)
                using (AsyncScopedLifestyle.BeginScope(_container))
                {
                    return _handlerFactory().Execute(command);
                }
            else
                return _handlerFactory().Execute(command);
        }
    }
}