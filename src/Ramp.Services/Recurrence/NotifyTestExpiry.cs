using Common.Command;
using Common.Query;
using Common.RecurringJob;
using Data.EF.Customer;
using Hangfire;

namespace Ramp.Services.Recurrence {
	public class NotifyTestExpiry : IRecurringJob
    {
        private readonly ICommandDispatcher _dispacther;
        private readonly IQueryExecutor _executor;
        private CustomerContext _context;
        public NotifyTestExpiry()
        {
            _dispacther = new CommandDispatcher();
            _executor = new QueryExecutor();
        }
        public string When => Cron.Daily();

        public void Work()
        {
            //if (Microsoft.Practices.ServiceLocation.ServiceLocator.IsLocationProviderSet)
            //{
            //    var mainContext = new MainContext();
            //    System.Web.HttpContext.Current = new System.Web.HttpContext(new System.Web.HttpRequest("", "https://www.google.com", ""), new System.Web.HttpResponse(new StringWriter()));
            //    var companies = new MainContext().Set<Company>().AsQueryable().Where(x => x.CompanyType == Domain.Enums.CompanyType.CustomerCompany);
            //    companies.ToList().ForEach(delegate (Company company)
            //    {
            //        string companyConnStringOld = ConfigurationManager.ConnectionStrings["CustomerContext"].ConnectionString;
            //        string conString = companyConnStringOld.Replace("DBNAME", "" + company.CompanyName.Replace(" ", string.Empty));
            //        _context = new CustomerContext(conString);

            //        var candidates = _executor.Execute<AllTestsThatRequiresExipryNotificationQuery, IEnumerable<Guid>>(new AllTestsThatRequiresExipryNotificationQuery
            //        {
            //            NotificationInterval = company.TestExpiryNotificationInterval,
            //            ArbitraryTestExpiryIntervalInDaysBefore = company.TestExpiryNotificationInterval == NotificationInterval.Arbitrary ? company.ArbitraryTestExpiryIntervalInDaysBefore : new int?(),
            //            Tests = _context.TrainingTestSet.AsEnumerable()
            //        });
            //        candidates.ToList().ForEach(x => _dispacther.Dispatch(new PrepareTestExpiryNotificationCommand
            //        {
            //            TestId = x,
            //            Company = company,
            //            CorrespondanceLogs = _context.StandardUserCorrespondanceLogs.AsEnumerable(),
            //            Tests = _context.TrainingTestSet.AsEnumerable(),
            //            TestResults = _context.TestResultSet.AsEnumerable(),
            //            TestsAssigned = _context.AssignedTestSet.AsEnumerable(),
            //            Users = _context.StandardUsers.AsEnumerable()
            //        }));
            //        _context.SaveChanges();
            //        _context.Dispose();
            //    });
            //    mainContext.Dispose();
            //}
            
        }
    }
}
