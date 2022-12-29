using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class AllTestsThatRequiresExipryNotificationQueryHandler : IQueryHandler<AllTestsThatRequiresExipryNotificationQuery, IEnumerable<Guid>>
    {
        private readonly IRepository<TrainingTest> _testRepository;
        public AllTestsThatRequiresExipryNotificationQueryHandler(IRepository<TrainingTest> testRepository)
        {
            _testRepository = testRepository;
        }
        public IEnumerable<Guid> ExecuteQuery(AllTestsThatRequiresExipryNotificationQuery query)
        {
            var tests = query.Tests.Any() ? query.Tests : _testRepository.List.AsQueryable();
            var allTestsThatExpire = tests.Where(x => x.TestExpiryDate.HasValue && x.TestExpiryDate.Value > DateTime.Today).ToList();
            var tommorow = DateTime.Today.AddDays(1);
            switch (query.NotificationInterval)
            {
                case NotificationInterval.Arbitrary:
                    if (!query.ArbitraryTestExpiryIntervalInDaysBefore.HasValue)
                        break;
                    var applicableDate = DateTime.Today.AddDays(query.ArbitraryTestExpiryIntervalInDaysBefore.Value);
                    allTestsThatExpire = allTestsThatExpire.ToList().Where(x => x.TestExpiryDate.Value.Date  == applicableDate.Date).ToList();
                    break;
                case NotificationInterval.OneDayBefore:
                    allTestsThatExpire = allTestsThatExpire.ToList().Where(x => x.TestExpiryDate.Value.Date == tommorow.Date).ToList();
                    break;
                case NotificationInterval.Daily:
                    break;
                default: break;
            }
            (allTestsThatExpire.ToList()).AddRange(query.Tests.ToList().Where(x => x.TestExpiryDate.HasValue && x.TestExpiryDate.Value.Date == DateTime.Today.Date).ToList());
            return allTestsThatExpire.Select(x => x.Id).Distinct().ToList();
        }
    }
}
