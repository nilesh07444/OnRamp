using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class AllUsersByProvisionalCompanyUserQueryHandler :
        QueryHandlerBase<AllUsersByProvisionalCompanyUserQueryParameter, List<UserViewModel>>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;

        public AllUsersByProvisionalCompanyUserQueryHandler(IRepository<Domain.Models.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public override List<UserViewModel> ExecuteQuery(AllUsersByProvisionalCompanyUserQueryParameter queryParameters)
        {
            var users = _userRepository.List.Where(s => s.Id != queryParameters.CurrentUserId && s.Company.ProvisionalAccountLink == queryParameters.CompanyId);

            var userList = new List<UserViewModel>();
            foreach (Domain.Models.User user in users)
            {
                var usermodel = new UserViewModel
                {
                    Id = user.Id,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MobileNumber = user.MobileNumber,
                    Status = user.IsActive,
                    ContactNumber = user.ContactNumber,
                    ParentUserId = user.ParentUserId,
                };
                userList.Add(usermodel);
            }

            return userList.Distinct().ToList();
        }
    }
}