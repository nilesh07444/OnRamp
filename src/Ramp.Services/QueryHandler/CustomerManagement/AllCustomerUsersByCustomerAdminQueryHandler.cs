using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.Security;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class AllCustomerUsersByCustomerAdminQueryHandler :
        QueryHandlerBase<AllCustomerUsersByCustomerAdminQueryParameter, List<UserViewModel>>
    {
        private readonly IRepository<StandardUser> _userRepository;

        public AllCustomerUsersByCustomerAdminQueryHandler(IRepository<StandardUser> userRepository)
        {
            _userRepository = userRepository;
        }

        public override List<UserViewModel> ExecuteQuery(AllCustomerUsersByCustomerAdminQueryParameter queryParameters)
        {
            return _userRepository.List.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser))).OrderBy(u => u.FirstName).ToList().Select(userModel => new UserViewModel
            {
                Id = userModel.Id,
                EmailAddress = userModel.EmailAddress,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName ?? "",
                FullName = $"{userModel.FirstName} {userModel.LastName}",
                MobileNumber = userModel.MobileNumber,
                Status = userModel.IsActive,
                SelectedGroupId = userModel.Group != null ? userModel.Group.Id : System.Guid.Empty,
                ContactNumber = userModel.ContactNumber,
                Password = new EncryptionHelper().Decrypt(userModel.Password),
                ConfirmPassword = new EncryptionHelper().Decrypt(userModel.Password),
                ParentUserId = userModel.ParentUserId,
            }).ToList();
        }
    }
}