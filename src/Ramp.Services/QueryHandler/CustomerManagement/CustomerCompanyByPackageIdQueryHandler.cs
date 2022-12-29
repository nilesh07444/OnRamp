using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using Domain.Enums;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    // TODO: Delete when completely unused
    //public class CustomerCompanyByPackageIdQueryHandler :
    //    QueryHandlerBase<CustomerCompanyByPackageIdQueryParameter, ProvisionalCompanyUserViewModel>
    //{
    //    private readonly IRepository<Company> _companyRepository;

    //    public CustomerCompanyByPackageIdQueryHandler(IRepository<Company> companyRepository)
    //    {
    //        _companyRepository = companyRepository;
    //    }

    //    public override ProvisionalCompanyUserViewModel ExecuteQuery(
    //        CustomerCompanyByPackageIdQueryParameter queryParameters)
    //    {
    //        var model = new ProvisionalCompanyUserViewModel();

    //        var companyList = _companyRepository.List.Where(u => u.PackageId == queryParameters.PackageId && u.CompanyType == CompanyType.CustomerCompany).ToList();

    //        foreach (Company company in companyList)
    //        {
    //            var companyViewModel = new CompanyViewModel
    //            {
    //                Id = company.Id,
    //                CompanyName = company.CompanyName,
    //                LayerSubDomain = company.LayerSubDomain,
    //                PhysicalAddress = company.PhysicalAddress,
    //                PostalAddress = company.PostalAddress,
    //                ClientSystemName = company.ClientSystemName,
    //            };
    //            model.CompanyList.Add(companyViewModel);
    //        }
    //        return model;
    //    }
    //}
}