using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.Security;
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
    public class AllContentAdminsForCustomerCompanyQueryHandler :
        IQueryHandler<AllContentAdminsForCustomerCompanyQuery, List<UserViewModel>>,
        IQueryHandler<AllContentAdminsForCustomerCompanyQuery, List<UserModelShort>>
    {
        private readonly ITransientReadRepository<StandardUser> _standardUserRepository;

        public AllContentAdminsForCustomerCompanyQueryHandler(ITransientReadRepository<StandardUser> standardUserRepository)
        {
            _standardUserRepository = standardUserRepository;
        }

        public List<UserViewModel> ExecuteQuery(AllContentAdminsForCustomerCompanyQuery query)
        {
            var result = new List<UserViewModel>();
            var contentAdmins =
                _standardUserRepository.List.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.ContentAdmin) || r.RoleName.Equals(Role.ContentApprover) || r.RoleName.Equals(Role.ContentApprover)) && u.IsActive).OrderBy(c => c.FirstName).ToList();
            contentAdmins.ForEach(c => result.Add(Project.UserViewModelFrom(c)));

            return result;
        }

        List<UserModelShort> IQueryHandler<AllContentAdminsForCustomerCompanyQuery, List<UserModelShort>>.ExecuteQuery(AllContentAdminsForCustomerCompanyQuery query)
        {
            return _standardUserRepository.List.AsQueryable().Where(u => u.Roles.Any(r => r.RoleName == Role.ContentCreator )  && u.IsActive).Select(Project.StandardUserToUserModelShort).OrderBy(c => c.Name).ToList();
        }
    }
}