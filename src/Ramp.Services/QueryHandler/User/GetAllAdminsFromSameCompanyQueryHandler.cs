using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Ramp.Services.Helpers;
using Common.Query;
using Domain.Enums;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts;
using Domain.Models;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.User
{
    public class GetAllAdminsFromSameCompanyQueryHandler :
        QueryHandlerBase<GetAllAdminsFromSameCompanyQueryParameter, IEnumerable<UserViewModel>>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;

        public GetAllAdminsFromSameCompanyQueryHandler(IRepository<Domain.Models.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public override IEnumerable<UserViewModel> ExecuteQuery(GetAllAdminsFromSameCompanyQueryParameter queryParameters)
        {
            var list = new List<UserViewModel>();

            var users = _userRepository.GetAll().Where(c => c.CompanyId == queryParameters.CompanyId &&  c.Roles.FirstOrDefault().RoleName.Contains("Admin") == true).ToList();
            
            foreach (Domain.Models.User user in users)
            {
                if(user != null)
                {
                    var author = new UserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        ContactNumber = user.ContactNumber,
                        CompanyId = user.CompanyId,
                        EmailAddress = user.EmailAddress,
                        MobileNumber = user.MobileNumber,
                        LastName = user.LastName,
                    };
                    list.Add(author);
                }
            }

            IEnumerable<UserViewModel> allUsers = list.GroupBy(g => g.Id).Select(user => user.First());
            return allUsers;
        }
    }
}