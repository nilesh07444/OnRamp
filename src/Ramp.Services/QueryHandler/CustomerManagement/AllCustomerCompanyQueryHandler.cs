using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using Role = Ramp.Contracts.Security.Role;
using Domain.Enums;
using System.Collections.Generic;
using System;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class AllCustomerCompanyQueryHandler : QueryHandlerBase<AllCustomerUserQueryParameter, CompanyViewModel>,
                                                  IQueryHandler<AllCustomerUserQueryParameter,IEnumerable<SerializableSelectListItem>>
    {
        private readonly IRepository<Company> _companyRepository;

        public AllCustomerCompanyQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CompanyViewModel ExecuteQuery(AllCustomerUserQueryParameter queryParameters)
        {
            if (queryParameters.LoginUserRole != null && queryParameters.LoginUserRole.Any(r => r.Equals(Role.Admin)))
            {
                var companies = _companyRepository.List.Where(c => c.CompanyType == CompanyType.CustomerCompany
                                                       && c.PhysicalAddress != "Dummy" && c.IsActive).OrderBy(c => c.CompanyName);
                var companyModel = new CompanyViewModel();
                foreach (var company in companies)
                {
                    companyModel.CompanyList.Add(new CompanyModelShort
                    {
                        Id = company.Id,
                        Name = company.CompanyName,
                        CompanyConnectionString = company.CompanyConnectionString
                    });
                }
                foreach (var company1 in companies)
                {
                    companyModel.FromCustomerCompanyList.Add(new CompanyModelShort
                    {
                        Id = company1.Id,
                        Name = company1.CompanyName
                    });
                }
                foreach (var company2 in companies)
                {
                    companyModel.ToCustomerCompanyList.Add(new CompanyModelShort
                    {
                        Id = company2.Id,
                        Name = company2.CompanyName
                    });
                }
                return companyModel;
            }
            else if (queryParameters.LoginUserRole != null && queryParameters.LoginUserRole.Any(r => r.Equals(Role.Reseller)))
            {

                var companies = _companyRepository.List.Where(c => c.CompanyType == CompanyType.CustomerCompany
                                           && c.PhysicalAddress != "Dummy" && c.IsActive && c.ProvisionalAccountLink == queryParameters.LoginUserCompanyId).OrderBy(u => u.CompanyName);
                var companyModel = new CompanyViewModel();
                foreach (var company in companies)
                {
                    companyModel.CompanyList.Add(new CompanyModelShort
                    {
                        Id = company.Id,
                        Name = company.CompanyName,
                        CompanyConnectionString = company.CompanyConnectionString
                    });
                }
                foreach (var company1 in companies)
                {
                    companyModel.FromCustomerCompanyList.Add(new CompanyModelShort
                    {
                        Id = company1.Id,
                        Name = company1.CompanyName
                    });
                }
                foreach (var company2 in companies)
                {
                    companyModel.ToCustomerCompanyList.Add(new CompanyModelShort
                    {
                        Id = company2.Id,
                        Name = company2.CompanyName
                    });
                }
                return companyModel;
            }
            else
            {
                var companies = _companyRepository.List.Where(c => c.CompanyType == CompanyType.CustomerCompany
                                                       && c.PhysicalAddress != "Dummy" && c.IsActive && c.ProvisionalAccountLink == queryParameters.LoginUserCompanyId).OrderBy(c => c.CompanyName);
                var companyModel = new CompanyViewModel();
                foreach (var company in companies)
                {
                    companyModel.CompanyList.Add(new CompanyModelShort
                    {
                        Id = company.Id,
                        Name = company.CompanyName
                    });
                }
                return companyModel;
            }
        }

        IEnumerable<SerializableSelectListItem> IQueryHandler<AllCustomerUserQueryParameter, IEnumerable<SerializableSelectListItem>>.ExecuteQuery(AllCustomerUserQueryParameter query)
        {
            var data = _companyRepository.List.AsQueryable().Where(c => c.CompanyType == CompanyType.CustomerCompany && c.PhysicalAddress != "Dummy" && c.IsActive).OrderBy(c => c.CompanyName)
                                                        .Select(x => new SerializableSelectListItem
                                                        {
                                                            Text = x.CompanyName,
                                                            Value = x.Id.ToString()
                                                        }).ToList();
            data.Insert(0, new SerializableSelectListItem { Value = Guid.Empty.ToString(), Selected = false, Text = "Select Company" });
            return data;
        }
    }
}
