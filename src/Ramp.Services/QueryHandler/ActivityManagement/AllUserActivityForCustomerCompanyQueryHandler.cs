using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.ActivityManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.ActivityManagement
{
    public class AllUserActivityForCustomerCompanyQueryHandler : IQueryHandler<AllUserActivityForCustomerCompanyQuery, List<UserActivityLogViewModel>>
    {
        private readonly IRepository<StandardUserActivityLog> _standardUserActivityLogRepository;
        private readonly IRepository<StandardUserDisclaimerActivityLog> _standardUserDisclaimerActivityLogRespository;

        public AllUserActivityForCustomerCompanyQueryHandler(IRepository<StandardUserActivityLog> standardUserActivityLogRepository,
            IRepository<StandardUserDisclaimerActivityLog> standardUserDisclaimerActivityLogRespository)
        {
            _standardUserActivityLogRepository = standardUserActivityLogRepository;
            _standardUserDisclaimerActivityLogRespository = standardUserDisclaimerActivityLogRespository;
        }

        public List<UserActivityLogViewModel> ExecuteQuery(AllUserActivityForCustomerCompanyQuery query)
        {
            var result = new List<UserActivityLogViewModel>();
            query.FromDate = query.FromDate.AtBeginningOfDay();
            query.ToDate = query.ToDate.AtEndOfDay();


            if (_standardUserActivityLogRepository.List.Any())
            {
                var entries = _standardUserActivityLogRepository.List.AsQueryable();
                if (query.UserId.HasValue)
                    entries = entries = entries.Where(a => a.User.Id.Equals(query.UserId.Value));
                if (query.FromDate.HasValue)
                   entries = entries.Where(x => x.ActivityDate >= query.FromDate.Value);

                if (query.ToDate.HasValue)
                   entries = entries.Where(x => x.ActivityDate <= query.ToDate.Value);
                entries.OrderByDescending(a => a.ActivityDate).ToList().ForEach(x => result.Add(Project.UserActivityLogViewModelFrom(x)));
            }
            if (_standardUserDisclaimerActivityLogRespository.List.Any())
            {
                var entries = _standardUserDisclaimerActivityLogRespository.List.AsQueryable();
                if (query.UserId.HasValue)
                    entries = entries.Where(x => x.User != null && x.User.Id.Equals(query.UserId.Value));
                if (query.FromDate.HasValue)
                   entries = entries.Where(x => x.Stamp >= query.FromDate.Value);
                
                if (query.ToDate.HasValue)
                    entries = entries.Where(x => x.Stamp <= query.ToDate.Value);
                entries.OrderByDescending(a => a.Stamp).ToList().ForEach(x => result.Add(Project.UserActivityLogViewModelFrom(x)));

            }

            return result.OrderByDescending(x => x.ActivityDate).ToList();
        }
    }
}