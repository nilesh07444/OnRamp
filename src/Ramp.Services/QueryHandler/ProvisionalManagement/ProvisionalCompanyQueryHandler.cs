using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using Domain.Enums;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class ProvisionalCompanyQueryHandler :
        QueryHandlerBase<ProvisionalCompanyQueryParameter, CompanyViewModelLong>
    {
        private readonly IRepository<Company> _companyRepository;

        public ProvisionalCompanyQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CompanyViewModelLong ExecuteQuery(ProvisionalCompanyQueryParameter queryParameters)
        {
            var companyViewModel = new CompanyViewModelLong();

            if (queryParameters.Id != null && queryParameters.Id != Guid.Empty)
            {
                Company companyModel = _companyRepository.Find(queryParameters.Id);
                if (companyModel != null)
                {
                    companyViewModel.CompanyViewModel = new CompanyViewModel
                    {
                        Id = companyModel.Id,
                        CompanyName = companyModel.CompanyName,
                        LayerSubDomain = companyModel.LayerSubDomain,
                        PhysicalAddress = companyModel.PhysicalAddress,
                        PostalAddress = companyModel.PostalAddress,
                        ProvisionalAccountLink = companyModel.ProvisionalAccountLink,
                        TelephoneNumber = companyModel.TelephoneNumber,
                        WebsiteAddress = companyModel.WebsiteAddress,
                        LogoImageUrl = companyModel.LogoImageUrl,
                        IsChangePasswordFirstLogin = companyModel.IsChangePasswordFirstLogin,
                        IsSendWelcomeSMS = companyModel.IsSendWelcomeSMS,
                        IsForSelfProvision =companyModel.IsForSelfProvision
                    };
                }
            }
            var companyList = _companyRepository.List
                .Where(a => a.PhysicalAddress != "Dummy")
                .Where(x => x.CompanyType == CompanyType.ProvisionalCompany);

            foreach (Company company in companyList)
            {
                var companyModel = new CompanyViewModel
                {
                    Id = company.Id,
                    Status = company.IsActive,
                    CompanyName = company.CompanyName,
                    LayerSubDomain = company.LayerSubDomain,
                    PhysicalAddress = company.PhysicalAddress,
                    PostalAddress = company.PostalAddress,
                    ProvisionalAccountLink = company.ProvisionalAccountLink,
                    TelephoneNumber = company.TelephoneNumber,
                    WebsiteAddress = company.WebsiteAddress,
                    IsChangePasswordFirstLogin = company.IsChangePasswordFirstLogin,
                    IsSendWelcomeSMS = company.IsSendWelcomeSMS,
                };
                companyViewModel.CompanyList.Add(companyModel);
            }
            return companyViewModel;
        }
    }
}