using Common.RecurringJob;
using Hangfire;
using Hangfire.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Web.UI.App_Start;
using WebActivatorEx;

[assembly: PostApplicationStartMethod(typeof(HangfireBootstrapper), "Initialize", Order = 1)]
namespace Web.UI.App_Start
{
    public class HangfireBootstrapper :
         IRegisteredObject,
         IProcessHostPreloadClient
    {
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();
        private readonly object _lockObject = new object();
        private BackgroundJobServer _backgroundJobServer;
        private bool _started;

        private HangfireBootstrapper()
        {
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_started) return;
                _started = true;

                HostingEnvironment.RegisterObject(this);

                GlobalConfiguration.Configuration.UseSqlServerStorage("MainContext");

                _backgroundJobServer = new BackgroundJobServer();
                var manager = new RecurringJobManager();
                foreach (var job in DependencyResolver.Current.GetServices<IRecurringJob>())
                {
                    var type = job.GetType();
                    var method = type.GetMethod("Work");
                    var j = new Job(type, method);

                    try
                    {
                        manager.AddOrUpdate(type.Name, j, job.When);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                _backgroundJobServer?.Dispose();
                HostingEnvironment.UnregisterObject(this);
            }
        }

        public static void Initialize()
        {
            Instance.Start();
        }

        public void Preload(string[] parameters)
        {
        }
    }
}