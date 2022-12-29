using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class UsersRatingHistoryByUserIdQueryHandler :
        QueryHandlerBase<UsersRatingHistoryByUserIdQueryParameter, List<CustomerSurveyDetailViewModel>>
    {
        private readonly IRepository<CustomerSurveyDetail> _customerSurveyDetailRepository;

        public UsersRatingHistoryByUserIdQueryHandler(IRepository<CustomerSurveyDetail> customerSurveyDetailRepository)
        {
            _customerSurveyDetailRepository = customerSurveyDetailRepository;
        }

        public override List<CustomerSurveyDetailViewModel> ExecuteQuery(
            UsersRatingHistoryByUserIdQueryParameter queryParameters)
        {
            queryParameters.FromDate = queryParameters.FromDate.AtBeginningOfDay();
            queryParameters.ToDate = queryParameters.ToDate.AtEndOfDay();
            List<CustomerSurveyDetail> userRatingDataList =
                _customerSurveyDetailRepository.List.AsQueryable()
                    .Where(
                        u =>
                            u.RatedOn >= queryParameters.FromDate &&
                            u.RatedOn <= queryParameters.ToDate   &&
                            u.User != null                        &&
                            u.User.Id.Equals(queryParameters.UserId)).ToList();
            var list = new List<CustomerSurveyDetailViewModel>();
            foreach (CustomerSurveyDetail customerSurveyDetail in userRatingDataList)
            {
                var model = new CustomerSurveyDetailViewModel
                {
                    CustomerSurveyId = customerSurveyDetail.Id,
                    RatedOn = customerSurveyDetail.RatedOn,
                    Comment = customerSurveyDetail.Comment,
                    Browser = customerSurveyDetail.Browser,
                    UserId = customerSurveyDetail.UserId,
                    Rating = customerSurveyDetail.Rating,
                    User = Project.UserViewModelFrom(customerSurveyDetail.User)
                };
                list.Add(model);
            }
            return list;
        }
    }
}