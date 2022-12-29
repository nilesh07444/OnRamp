using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Ramp.Services.Helpers;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using Domain.Enums;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class CustomerUserByProvisionalUserQueryHandler :
        QueryHandlerBase<CustomerCompanyUserQueryParameter, ProvisionalCompanyUserViewModel>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;

        public CustomerUserByProvisionalUserQueryHandler(IRepository<Domain.Models.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public override ProvisionalCompanyUserViewModel ExecuteQuery(CustomerCompanyUserQueryParameter queryParameters)
        {
            var model = new ProvisionalCompanyUserViewModel();
            var userList = _userRepository.List
                                        .AsQueryable()
                                        .Where(u => u.Company.ProvisionalAccountLink == queryParameters.CompanyId &&
                                                    u.IsActive && 
                                                    u.Company.CompanyType == CompanyType.CustomerCompany)
                                        .ToList();

            foreach (Domain.Models.User user in userList)
            {
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MobileNumber = user.MobileNumber,
                    ContactNumber = user.ContactNumber,
                    Status = user.IsActive,
                    Password = new EncryptionHelper().Decrypt(user.Password),
                    ConfirmPassword = new EncryptionHelper().Decrypt(user.Password),
                    ParentUserId = user.ParentUserId,
                };
                model.UserList.Add(userViewModel);
            }
            return model;
        }
    }
}

