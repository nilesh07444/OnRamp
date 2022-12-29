using System;
using System.Collections.Generic;
using System.Linq;
using Data.EF.Data;
using Data.EF.Query;
using Domain.Models;
using Ramp.Services.QueryParameter.ProvisionalManagement;
using Ramp.ViewModel;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class AllProvisionalUserQueryHandler :
        QueryHandlerBase<AllProvisionalUserQueryParameter, UserViewModel>
    {
        private readonly DataRepository<Company> _companyRepository = new DataRepository<Company>();
        private readonly DataRepository<User> _userRepository = new DataRepository<User>();

        public override UserViewModel ExecuteQuery(AllProvisionalUserQueryParameter queryParameters)
        {
            IEnumerable<User> users = _userRepository.GetAll().Where(u => u.ParentUserId != Guid.Empty && u.IsActive);

            IEnumerable<Company> companies =
                _companyRepository.GetAll().Where(c => c.CompanyType == CompanyType.ProvisionalCompany
                                                       && c.PhysicalAddress != "Dummy" && c.IsActive);

            IEnumerable<User> userslist = users.Join(companies, s => s.CompanyId, company => company.Id,
                (s, company) => s);

            var userModel = new UserViewModel();
            foreach (User user in userslist)
            {
                userModel.UserList.Add(new UserModelShort
                {
                    Id = user.CompanyId,
                    Name = user.FirstName
                });
            }
            return userModel;
        }
    }
}