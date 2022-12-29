using AutoMapper;
using AutoMapper.Internal;
using Domain.Models;
using Microsoft.Win32.TaskScheduler;
using Quartz;
using Quartz.Impl;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.PackageManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using SimpleInjector.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using Web.UI.App_Start;
using Web.UI.Areas.CustomerManagement.Controllers;
using Web.UI.Code.Cache;
using Web.UI.Code.Jobs;

namespace Web.UI
{
    public class MvcApplication : HttpApplication, IRequiresSessionState
    {
        private static string _rootPath;

        public static string RootPath
        {
            get
            {
                if (HttpContext.Current == null)
                    return _rootPath;

                if (HttpContext.Current.Session == null)
                    return null;

                return HttpContext.Current.Session["RootPath"].ToNullSafeString();
            }
            set
            {
                if (HttpContext.Current.Session != null)
                    HttpContext.Current.Session["RootPath"] = value;
            }
        }


        public static ISchedulerFactory SchedulerFactory;
        public static IScheduler Scheduler;

        protected void Application_Start()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjI5Njc3QDMxMzcyZTM0MmUzMGFvQmdQamFDcytpbUdlOUR4MVFPUzhyVTFuM1NBUEl1YlBieUptelZWY289;MjI5Njc4QDMxMzcyZTM0MmUzME1CNFF6ZjA0MnBvcTlKdW1LWjY0Uk43ampESGYyM25YZW5FS0VyOG5XdUE9;MjI5Njc5QDMxMzcyZTM0MmUzMEpDTEFZQ0thWk42bVloTTRJMGw4M0hSMjBFTDBaQzRId0g0ZDNkYUV1a0k9;MjI5NjgwQDMxMzcyZTM0MmUzMEtlZlh3czJyNHdKUmdWNEJpNG93b2piYVdjd05TN3F4VmVadUtHWVkvMUU9;MjI5NjgxQDMxMzcyZTM0MmUzMFFLb1BHYzV4RE42THk0cFdyVmJnODY0ckZ0dXJHY1JXTU9oQVlITGY4UFU9;MjI5NjgyQDMxMzcyZTM0MmUzMGwzVFhSKzg2UHJ5ckJGSUNGRnpSUFlnNTVTWWRGdjNiWEV5RUNmN3FiVXM9;MjI5NjgzQDMxMzcyZTM0MmUzMFI4MFlyQUpGdmJSVlFLTEVKYUl5eFJodDFHZ3U2U3JwRkxBb3VpS2hQOGc9;MjI5Njg0QDMxMzcyZTM0MmUzMGg3NVZSVXdzWXJKeC9FZnZqMmpqeG45eURGSTVFbGx0MXBOSHRWdG5zQjA9;MjI5Njg1QDMxMzcyZTM0MmUzMFllOU5OTXVFTDVQNDZtL1JrNWhZVU81VEU3TGVSWmh0bDBiUmtMdWRsdUE9;NT8mJyc2IWhiZH1nfWN9ZmZoYmF8YGJ8ampqanNiYmlmamlmanMDHmgjNiEgOzI9Jzs2PRM0PjI6P30wPD4=;MjI5Njg2QDMxMzcyZTM0MmUzMEsxOHBCOGs0K1NCYk5iN3V3U2xtV1h2bjh2b0Z6dlk1UkdRNmtGS3Nzc3c9");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfigs.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = false;
            RampSecurityConfig.InitializationSecurityDictionary();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            CreateMappings();


            SchedulerFactory = new StdSchedulerFactory();
            Scheduler = SchedulerFactory.GetScheduler();
            Scheduler.Start();

            UserMgmtSchedular();
            ReportSchedular_Daily();
            ReportSchedular_Monthly();
            DocumentAssignmentEmail();
            //NotifyTestReportAboutTestExpiry();
            //NotifyCompanyLockStatus();
            //NotifyUserLockStatus();

            // testreport();
            //   SendEmails();

            //VirtualMeetingReminderEmail();
            // new ReportSchedular().ExecuteReportSchedular("Daily");
            // new ReportSchedular().ExecuteReportSchedular("Daily");
        }

        protected void Application_End()
        {
            Scheduler.Shutdown();
        }

        //AD AutoSync
        private void UserMgmtSchedular()
        {
            IJobDetail job = JobBuilder.Create<UserManagementSchedular>()
                .WithIdentity("UserManagementSchedularJob", "UserManagementSchedularLock")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("UserManagementSchedularTrigger", "UserManagementSchedularLock")
                .WithCronSchedule("0 0 6 ? * MON-FRI *")
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }

        //Report Schedule Daily
        private void ReportSchedular_Daily()
        {
            IJobDetail job = JobBuilder.Create<NotifyReportSchedular>()
                .WithIdentity("Daily", "ReportSchedularLock")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ReportSchedularDailyTrigger", "ReportSchedularDailyLock")
                .WithCronSchedule("0 0 6 ? * MON-FRI *")
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }

       

        //Report Schedule Monthly
        private void ReportSchedular_Monthly()
        {
            IJobDetail job = JobBuilder.Create<NotifyReportSchedular>()
                .WithIdentity("Monthly", "ReportSchedularMonthlyLock")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ReportSchedularMonthlyTrigger", "ReportSchedularMonthlyLock")                
                .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(1, 12, 0))
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }

        //Document Assignment
        private void DocumentAssignmentEmail()
        {
            IJobDetail job = JobBuilder.Create<DocumentAssignmentEmailJob>()
                .WithIdentity("DocumentAssignmentEmailJob", "Email")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("DocumentAssignmentEmailTrigger", "Email")
                .WithCronSchedule("0 0 6 ? * MON-FRI *")
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }


        private void NotifyCompanyLockStatus()
        {
            IJobDetail job = JobBuilder.Create<NotifyCompanyLockStatusJob>()
                .WithIdentity("NotifyCompanyLockStatusJob", "CompanyLock")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("NotifyCompanyLockStatusTrigger", "CompanyLock")
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(12))
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }

        private void NotifyTestReportAboutTestExpiry()
        {
            IJobDetail job = JobBuilder.Create<NotifyTestReportJob>()
                .WithIdentity("NotifyTestReportJob", "TestExpired")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("NotifyTestReportTrigger", "TestExpired")
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(12))
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }
        //private void DocumentAssignmentEmail()
        //{
        //    IJobDetail job = JobBuilder.Create<DocumentAssignmentEmailJob>()
        //        .WithIdentity("DocumentAssignmentEmailJob", "Email")
        //        .Build();
        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("DocumentAssignmentEmailTrigger", "Email")
        //        .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(12))
        //        .StartNow()
        //        .Build();
        //    Scheduler.ScheduleJob(job, trigger);
        //}
        private void VirtualMeetingReminderEmail()
        {
            IJobDetail job = JobBuilder.Create<VirtualMeetingReminderJob>()
                .WithIdentity("VirtualMeetingReminderJob", "Email")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("VirtualMeetingReminderEmailTrigger", "Email")
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInMinutes(15))
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }
        public override void Init()
        {
            base.Init();
            this.BeginRequest += MvcApplication_BeginRequest;
            this.EndRequest += delegate (object sender, EventArgs args)
            {
                RequestCache.Current.Dispose();
            };

            this.PreRequestHandlerExecute += delegate (object sender, EventArgs args)
            {
                if (HttpContext.Current.Session != null)
                {
                    if (RootPath == null)
                    {
                        Uri uri = HttpContext.Current.Request.Url;
                        var host = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port +
                                   HttpContext.Current.Request.ApplicationPath;
                        RootPath = host.Replace(":80", string.Empty).Replace(":443", string.Empty);
                    }
                    _rootPath = RootPath;
                }
            };
        }
        private void SendEmails()
        {
            IJobDetail job = JobBuilder.Create<SendEmails>()
                .WithIdentity("SendEmailJob", "SendEmail")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("SendEmailTrigger", "SendEmail")
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInMinutes(1))
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }

        private void MvcApplication_BeginRequest(object sender, EventArgs e)
        {
            if (!Request.IsLocal && !Request.IsSecureConnection &&
                System.Configuration.ConfigurationManager.AppSettings["requiressl"] == "true")
            {
                string redirectUrl = Request.Url.ToString().Replace("http:", "https:");
                Response.Redirect(redirectUrl, false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();

            }
        }

        private void Application_Error(object sender, EventArgs e)
        {
            //// Code that runs when an unhandled error occurs
            Exception ex = new Exception();
            ex = Server.GetLastError();
            //Errorloghelper.getErrorexception(ex);
        }

        private void CreateMappings()
        {
            Mapper.CreateMap<PackageViewModelShort, Package>();
            Mapper.CreateMap<PackageViewModelShort, PackageCommandParameter>();
            Mapper.CreateMap<CompanyViewModel, Company>();
        }

        private void NotifyUserLockStatus()
        {
            IJobDetail job = JobBuilder.Create<NotifyUserLockStatusJob>()
                .WithIdentity("NotifyUserLockStatusJob", "UserLock")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("NotifyUserLockStatusTrigger", "UserLock")
                .StartNow()
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(12))
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }




       



        private void testreport()
        {
            IJobDetail job = JobBuilder.Create<testreport>()
                .WithIdentity("Daily1", "ReportSchedularLock1")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ReportSchedularDailyTrigger1", "ReportSchedularDailyLock1")
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInMinutes(5))
                .StartNow()
                .Build();
            Scheduler.ScheduleJob(job, trigger);
        }







    }
}