using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class LatestSurveyByCustomerUserQueryHandler :
        QueryHandlerBase<LatestSurveyByCustomerUserQueryParameter, CustomerSurveyDetailViewModel>
    {
        private readonly IRepository<CustomerSurveyDetail> _customerSurveyDetailRepository;

        public LatestSurveyByCustomerUserQueryHandler(IRepository<CustomerSurveyDetail> customerSurveyDetailRepository)
        {
            _customerSurveyDetailRepository = customerSurveyDetailRepository;
        }

        public override CustomerSurveyDetailViewModel ExecuteQuery(
            LatestSurveyByCustomerUserQueryParameter queryParameters)
        {
            var customerSurveyDetailModel = new CustomerSurveyDetailViewModel();
            var customerSurveyDetail = _customerSurveyDetailRepository.List.Where(u => u.UserId == queryParameters.CurrentUserId)
                    .OrderByDescending(a => a.RatedOn)
                    .FirstOrDefault();
            if (customerSurveyDetail != null)
            {
                customerSurveyDetailModel.UserId = customerSurveyDetail.UserId;
                customerSurveyDetailModel.Comment = customerSurveyDetail.Comment;
                customerSurveyDetailModel.RatedOn = customerSurveyDetail.RatedOn;
            }
            return customerSurveyDetailModel;
        }
    }
}