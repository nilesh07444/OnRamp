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
    public class AverageRatingByCustomerCompanyQueryHandler :
        QueryHandlerBase<AverageRatingByCustomerCompanyQueryParameter, List<CompanyViewModel>>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerSurveyDetail> _customerSurveyDetailRepository;

        public AverageRatingByCustomerCompanyQueryHandler(IRepository<Company> companyRepository, 
                                                            IRepository<StandardUser> userRepository, 
                                                            IRepository<CustomerSurveyDetail> customerSurveyDetailRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _customerSurveyDetailRepository = customerSurveyDetailRepository;
        }

        public override List<CompanyViewModel> ExecuteQuery(AverageRatingByCustomerCompanyQueryParameter queryParameters)
        {
            DateTime startDate = queryParameters.FromDate;
            startDate = startDate.AddHours(-12);

            DateTime endDate = queryParameters.ToDate;
            endDate = endDate.AddHours(12).AddSeconds(-1);

            var companyList = new List<CompanyViewModel>();
            foreach (var companyId in queryParameters.CompanyIds)
            {
                try
                {
                    var companyModel = _companyRepository.Find(companyId);
                    var provisionalCompany = _companyRepository.Find(companyModel.ProvisionalAccountLink);
                    var companyUsers = _userRepository.List.Where(u => u.CompanyId == companyId).ToList();

                    var userRatingDataList = _customerSurveyDetailRepository.List
                        .Where(
                            u =>
                               u.RatedOn >= startDate &&
                                u.RatedOn <= endDate
                                && companyUsers.Select(s => s.Id).Contains(u.UserId)).ToList();

                    var list = userRatingDataList
                        .GroupBy(g => g.User.CompanyId, r => r.Rating)
                        .Select(g => new
                        {
                            companyId = g.Key,
                            Rating = g.Average()
                        });

                    var companyViewModel = new CompanyViewModel
                    {
                        Id = companyModel.Id,
                        CompanyName = companyModel.CompanyName,
                        LayerSubDomain = companyModel.LayerSubDomain,
                        PhysicalAddress = companyModel.PhysicalAddress,
                        PostalAddress = companyModel.PostalAddress,
                        ClientSystemName = companyModel.ClientSystemName,
                    };

                    if (provisionalCompany != null)
                    {
                        companyViewModel.ProvisionalAccountName = provisionalCompany.CompanyName;
                    }
                    var firstOrDefault = list.FirstOrDefault();
                    companyViewModel.AverageRatingByCompany = firstOrDefault != null ? firstOrDefault.Rating : 0;
                    companyList.Add(companyViewModel);
                }
                catch { }
            }
            return companyList;
        }
    }
}