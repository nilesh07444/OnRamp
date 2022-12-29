using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using System.Web.Mvc;
using SimpleInjector.Integration.Web.Mvc;
using System.Reflection;
using Common.Query;
using Common.Command;
using Common.Events;
using Common.RecurringJob;
using Ramp.Contracts.ViewModel;
using System.Configuration;
using Web.UI.Code.Cache;
using Common.Data;
using SimpleInjector.Lifestyles;
using Web.UI.Code.Decorators;
using WebActivatorEx;
using Web.UI.App_Start;
using Hangfire;
using Hangfire.SimpleInjector;
using Microsoft.Practices.ServiceLocation;
using Data.EF.Customer;
using Domain.Customer.Models;

[assembly:PostApplicationStartMethod(typeof(SimpleInjectorConfig),"Initialize",Order = 0)]
namespace Web.UI.App_Start
{
    public class SimpleInjectorConfig
    {
        public static void Initialize()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = Lifestyle.CreateHybrid(new WebRequestLifestyle(),new AsyncScopedLifestyle());
            //container.Options.SuppressLifestyleMismatchVerification = true;
            RegisterServices(container);
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.Verify();
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            ServiceLocator.SetLocatorProvider(() => new CommonServiceLocator.SimpleInjectorAdapter.SimpleInjectorServiceLocatorAdapter(container));
        }
        public static void RegisterServices(Container container)
        {
			var lifestyle = Lifestyle.Scoped;

			var assemblies = new[]
            {
                typeof(Common.Command.CommandDispatcher).Assembly,
                typeof(Data.EF.MainContext).Assembly,
                typeof(Data.EF.Customer.CustomerContext).Assembly,
                typeof(Domain.Models.Bundle).Assembly,
                typeof(Domain.Customer.Base.CustomerDomainObject).Assembly,
                typeof(Ramp.Contracts.Command.Bundle.CreateOrUpdateBundleCommand).Assembly,
                typeof(Ramp.Services.QueryHandler.BundleQueryHandler).Assembly,
                typeof(Ramp.Security.Authorization.RampSecurity).Assembly,
                System.Reflection.Assembly.GetExecutingAssembly()
            };
            container.Register<IQueryExecutor, QueryExecutor>();
            container.Register<ICommandDispatcher, CommandDispatcher>();
            container.Register<IEventPublisher, EventPublisher>();
            container.Register<Ramp.Services.INotificationService, Ramp.Services.Implementations.NotificationService>();

            container.Register(typeof(ICommandHandlerBase<>), assemblies);
            container.Register(typeof(IValidator<>), assemblies);
            container.Register(typeof(IQueryHandler<,>), assemblies);

            container.RegisterCollection(typeof(IEventHandler<>), assemblies);
            container.RegisterCollection(typeof(IRecurringJob), assemblies);
            Func<Data.EF.Customer.CustomerContext> customerContextFactory = () =>
            {
                if (PortalContext.Current == null)
                    return new Data.EF.Customer.CustomerContext();
                else
                {

                    var connectionString = DependencyResolver.Current.GetService<IConnectionStringResolver>()?.Resolve();
                    var cached = RequestCache.Current.Get<Data.EF.Customer.CustomerContext>("CustomerContext", connectionString);
                    if (cached == null)
                    {
                        var customerContext = new Data.EF.Customer.CustomerContext(connectionString);
                        customerContext.Database.Initialize(false);
                        RequestCache.Current.Set("CustomerContext", connectionString, customerContext);
                        cached = customerContext;
                    }
                    return cached;
                }
            };
            container.Register(typeof(Data.EF.MainContext), () => new Data.EF.MainContext(), lifestyle);
            container.Register(typeof(Data.EF.Customer.CustomerContext), () => customerContextFactory(), lifestyle);
            container.Register(typeof(Func<CustomerContext>), () => customerContextFactory, lifestyle);
            container.Register(typeof(IConnectionStringResolver), typeof(ConnectionStringResolver),lifestyle);

            Func<PredicateContext, bool> isMainContext = (context) => context.ServiceType.GetGenericArguments()[0].Namespace == typeof(Domain.Models.Company).Namespace;
            Func<PredicateContext, bool> isCustomerContext = c => c.ServiceType.GetGenericArguments()[0].Namespace.StartsWith(typeof(StandardUser).Namespace);
            container.RegisterConditional(typeof(IRepository<>), typeof(Data.EF.Repository<>), lifestyle, (context) => isMainContext(context));
            container.RegisterConditional(typeof(IReadRepository<>), typeof(Data.EF.MainReadRepository<>), lifestyle, context => isMainContext(context));
            container.RegisterConditional(typeof(IRepository<>), typeof(Data.EF.Customer.Repository<>), lifestyle, (context) => isCustomerContext(context));
            container.RegisterConditional(typeof(IReadRepository<>), typeof(ReadRepository<>), lifestyle, context => isCustomerContext(context));
            container.RegisterConditional(typeof(ITransientRepository<>), typeof(TransientRepository<>), lifestyle, (c) => isCustomerContext(c));
            container.RegisterConditional(typeof(ITransientReadRepository<>), typeof(TransientReadRepository<>), lifestyle, (c) => isCustomerContext(c));

            container.RegisterDecorator(typeof(ICommandHandlerBase<>), typeof(ScopedCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(ScopedQueryHandlerDecorator<,>));
            container.RegisterDecorator(typeof(IEventHandler<>), typeof(ScopedEventHandlerDecorator<>));
            GlobalConfiguration.Configuration.UseActivator(new SimpleInjectorJobActivator(container));
        }
    }
}