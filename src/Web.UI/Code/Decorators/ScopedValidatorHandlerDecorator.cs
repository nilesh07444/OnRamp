using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Web.UI.Code.Decorators
{
    public class ScopedValidatorHandlerDecorator<T> : IValidator<T>
    {
        private readonly Func<IValidator<T>> _handlerFactory;
        private readonly Container _container;

        public ScopedValidatorHandlerDecorator(Container container, Func<IValidator<T>> handlerFactory)
        {
            _container = container;
            _handlerFactory = handlerFactory;
        }

        public IEnumerable<IValidationResult> Validate(T argument)
        {
            if (_container.Options.DefaultScopedLifestyle.GetCurrentScope(_container) == null)
                using (AsyncScopedLifestyle.BeginScope(_container))
                {
                    return _handlerFactory().Validate(argument);
                }
            else
                return _handlerFactory().Validate(argument);
        }
    }
}