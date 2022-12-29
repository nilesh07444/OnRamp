using Hangfire;
using Microsoft.Owin;
using Owin;
using SimpleInjector.Lifestyles;
using SimpleInjector;
using System.Web.Mvc;
using Web.UI;

[assembly: OwinStartupAttribute(typeof(Startup))]
namespace Web.UI {
	public partial class Startup {
		public void Configuration(IAppBuilder app) {

			ConfigureAuth(app);
			GlobalConfiguration.Configuration.UseSqlServerStorage("MainContext");
			app.UseHangfireDashboard("/hangfire");
			app.Use(async (context, next) => {
				using (AsyncScopedLifestyle.BeginScope(DependencyResolver.Current.GetService<Container>()))
					await next();
			});
		}
	}
}