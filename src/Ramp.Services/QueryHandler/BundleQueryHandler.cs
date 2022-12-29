using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.Query.Bundle;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler
{
    public class BundleQueryHandler : IQueryHandler<CustomerCompanyByBundleIdQuery, ProvisionalCompanyUserViewModel>,
        IQueryHandler<BundleQuery, BundleViewModel>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Bundle> _bundleRepository;

        public BundleQueryHandler(IRepository<Company> companyRepository,
            IRepository<Bundle> bundleRepository)
        {
            _companyRepository = companyRepository;
            _bundleRepository = bundleRepository;
        }

        public ProvisionalCompanyUserViewModel ExecuteQuery(CustomerCompanyByBundleIdQuery query)
        {
            var model = new ProvisionalCompanyUserViewModel();
            var companyList = _companyRepository.List.Where(c => c.BundleId == query.BundleId);

            foreach (var company in companyList)
            {
                var companyViewModel = new CompanyViewModel
                {
                    Id = company.Id,
                    CompanyName = company.CompanyName,
                    LayerSubDomain = company.LayerSubDomain,
                    PhysicalAddress = company.PhysicalAddress,
                    PostalAddress = company.PostalAddress,
                    ClientSystemName = company.ClientSystemName
                };
                model.CompanyList.Add(companyViewModel);
            }

            return model;
        }

        public BundleViewModel ExecuteQuery(BundleQuery query)
        {
            var bundleViewModel = new BundleViewModel();

            if (query.Id != null && query.Id != Guid.Empty.ToString())
            {
                var bundle = _bundleRepository.Find(query.Id);
                if (bundle != null)
                {
                    bundleViewModel.BundleViewModelShort = new BundleViewModelShort
                    {
                        Id = bundle.Id,
                        Title = bundle.Title,
                        Description = bundle.Description,
                        MaxNumberOfDocuments = bundle.MaxNumberOfDocuments,
                        IsForSelfProvision = bundle.IsForSelfProvision
                    };
                }
            }

            var bundleList = _bundleRepository.List.Where(b => b.Title != "Dummy")
                .Select(b => new BundleViewModelShort
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Title,
                    MaxNumberOfDocuments = b.MaxNumberOfDocuments,
                    IsForSelfProvision = b.IsForSelfProvision
                });
            bundleViewModel.BundleViewModelList = bundleList;

            return bundleViewModel;
        }
    }
}