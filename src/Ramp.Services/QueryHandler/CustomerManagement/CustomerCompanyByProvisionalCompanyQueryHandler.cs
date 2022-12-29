using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class CustomerCompanyByProvisionalCompanyQueryHandler :
        QueryHandlerBase<CustomerCompanyQueryParameter, ProvisionalCompanyUserViewModel>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Bundle> _bundleRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryExecutor _queryExecutor;

        public CustomerCompanyByProvisionalCompanyQueryHandler(IRepository<Company> companyRepository,
            IRepository<Bundle> bundleRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            ICommandDispatcher commandDispatcher,
            IQueryExecutor queryExecutor)
        {
            _companyRepository = companyRepository;
            _bundleRepository = bundleRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _commandDispatcher = commandDispatcher;
            _queryExecutor = queryExecutor;
        }

        public override ProvisionalCompanyUserViewModel ExecuteQuery(CustomerCompanyQueryParameter queryParameters)
        {
            var model = new ProvisionalCompanyUserViewModel();
            List<Company> companyList;
            if (queryParameters.IsForAdmin)
                companyList = _companyRepository.List.Where(u => u.ProvisionalAccountLink == queryParameters.Id && u.CompanyType == CompanyType.CustomerCompany).ToList();
            else
                companyList = _companyRepository.List.Where(u => u.ProvisionalAccountLink == queryParameters.Id && u.IsActive && u.CompanyType == CompanyType.CustomerCompany).ToList();

            foreach (Company company in companyList)
            {
                var companyViewModel = new CompanyViewModel
                {
                    Id = company.Id,
                    CompanyName = company.CompanyName,
                    LayerSubDomain = company.LayerSubDomain,
                    PhysicalAddress = company.PhysicalAddress,
                    PostalAddress = company.PostalAddress,
                    ClientSystemName = company.ClientSystemName,
                    CreatedOn = company.CreatedOn,
                };
                //model.CompanyList.Add(companyViewModel);

                //Get 'Bundle Name' & 'NumberOfTrainingGuides' data
                if (company.BundleId != Guid.Empty.ToString())
                {
                    var bundle = _bundleRepository.Find(company.BundleId);
                    companyViewModel.BundleName = bundle.Title;
                    var maxDocumentsOnBundle = bundle.MaxNumberOfDocuments;

                    var numberOfDocuments = _queryExecutor.Execute<CompanyDocumentCountQuery, int>(new CompanyDocumentCountQuery
                    {
                        CompanyId = company.Id
                    });
                    companyViewModel.CustomersNumberOfDocuments = $"{numberOfDocuments} out of {maxDocumentsOnBundle}";
                    companyViewModel.BundleSize = maxDocumentsOnBundle;
                }
                model.CompanyList.Add(companyViewModel);
            }
            return model;
        }
    }
}