using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler.CorrespondenceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class AllCustomerUserQueryHandler : IQueryHandler<AllCustomerUserQueryParameter, List<UserViewModel>>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;

        public AllCustomerUserQueryHandler(IRepository<StandardUser> standardUserRepository)
        {
            _standardUserRepository = standardUserRepository;
        }

        public List<UserViewModel> ExecuteQuery(AllCustomerUserQueryParameter query)
        {
            var result = new List<UserViewModel>();
            if (_standardUserRepository.List.Any())
            {
                result.AddRange(_standardUserRepository.List.OrderBy(c => c.FirstName).Select(Project.UserViewModelFrom));
            }
            return result;
        }
    }
}