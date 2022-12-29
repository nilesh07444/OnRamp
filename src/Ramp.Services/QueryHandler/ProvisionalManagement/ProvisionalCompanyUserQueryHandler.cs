using Common.Data;
using Common.Query;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System.Linq;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class ProvisionalCompanyUserQueryHandler :
        QueryHandlerBase<ProvisionalCompanyUserQueryParameter, CompanyUserViewModel>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;

        public ProvisionalCompanyUserQueryHandler(IRepository<Domain.Models.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public override CompanyUserViewModel ExecuteQuery(
            ProvisionalCompanyUserQueryParameter queryParameters)
        {
            var provisionalCompanyUserViewModel = new CompanyUserViewModel();

            Domain.Models.User userModel = _userRepository.Find(queryParameters.UserId);
            if (userModel != null)
            {
                var userViewModel = new UserViewModel
                {
                    Id = userModel.Id,
                    EmailAddress = userModel.EmailAddress,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    FullName = $"{userModel.FirstName} {userModel.LastName}",
                    MobileNumber = userModel.MobileNumber,
                    Status = userModel.IsActive,
                    ContactNumber = userModel.ContactNumber,
                    Password = new EncryptionHelper().Decrypt(userModel.Password),
                    ConfirmPassword = new EncryptionHelper().Decrypt(userModel.Password),
                    ParentUserId = userModel.ParentUserId,
                    IDNumber = userModel.IDNumber
                };

                var companyViewModel = new CompanyViewModel
                {
                    CompanyName = userModel.Company.CompanyName,
                    WebsiteAddress = userModel.Company.WebsiteAddress,
                    PhysicalAddress = userModel.Company.PhysicalAddress,
                    PostalAddress = userModel.Company.PostalAddress,
                    TelephoneNumber = userModel.Company.TelephoneNumber,
                };

                userViewModel.Company = companyViewModel;
                provisionalCompanyUserViewModel.UserViewModel = userViewModel;
            }

            var userlist = _userRepository.List.ToList().Where(a => a.CompanyId == queryParameters.CompanyId);
            foreach (Domain.Models.User user in userlist)
            {
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    MobileNumber = user.MobileNumber,
                    ContactNumber = user.ContactNumber,
                    Status = user.IsActive,
                    Password = new EncryptionHelper().Decrypt(user.Password),
                    ConfirmPassword = new EncryptionHelper().Decrypt(user.Password),
                    ParentUserId = user.ParentUserId,
                    IDNumber = user.IDNumber
                };
                provisionalCompanyUserViewModel.UserList.Add(userViewModel);
            }
            return provisionalCompanyUserViewModel;
        }
    }
}