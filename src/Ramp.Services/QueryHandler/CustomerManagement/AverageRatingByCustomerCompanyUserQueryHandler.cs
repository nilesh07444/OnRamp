using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class AverageRatingByCustomerCompanyUserQueryHandler :
        QueryHandlerBase<AverageRatingByCustomerCompanyQueryParameter, List<UserViewModel>>
    {
        private readonly IRepository<CustomerSurveyDetail> _customerSurveyDetailRepository;
        private readonly IRepository<StandardUser> _userRepository;

        public AverageRatingByCustomerCompanyUserQueryHandler(IRepository<CustomerSurveyDetail> customerSurveyDetailRepository, IRepository<StandardUser> userRepository)
        {
            _customerSurveyDetailRepository = customerSurveyDetailRepository;
            _userRepository = userRepository;
        }

        public override List<UserViewModel> ExecuteQuery(AverageRatingByCustomerCompanyQueryParameter queryParameters)
        {
            var userList = new List<UserViewModel>();
            queryParameters.FromDate = queryParameters.FromDate.AtBeginningOfDay();
            queryParameters.ToDate = queryParameters.ToDate.AtEndOfDay();
            foreach (Guid companyId in queryParameters.CompanyIds)
            {
                Guid id = companyId;
                var allCustomerUsers = _userRepository.List.AsQueryable();

                var userRatingDataList = _customerSurveyDetailRepository.List.AsQueryable().Where(
                    x =>
                    x.RatedOn >= queryParameters.FromDate && 
                    x.RatedOn <= queryParameters.ToDate);

                //To get individual average rating by a customer user
                var list = userRatingDataList
                    .GroupBy(g => g.UserId, r => r.Rating)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        Rating = g.Average()
                    }).ToList();
                foreach (var user in list)
                {
                    var userModel = _userRepository.Find(user.UserId);

                    try
                    {
                        var userViewModel = new UserViewModel
                        {
                            Id = userModel.Id,
                            EmailAddress = userModel.EmailAddress,
                            FirstName = userModel.FirstName,
                            LastName = userModel.LastName,
                            MobileNumber = userModel.MobileNumber,
                            Status = userModel.IsActive,
                            SelectedGroupId = userModel.Group?.Id,
                            ContactNumber = userModel.ContactNumber,
                            ParentUserId = userModel.ParentUserId,
                            AverageUserRating = user.Rating
                        };
                        userList.Add(userViewModel);
                    }
                    catch { }
                }
            }
            return userList;
        }
    }
}