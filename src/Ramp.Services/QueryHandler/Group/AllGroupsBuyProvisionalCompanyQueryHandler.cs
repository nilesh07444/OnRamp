using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using Domain.Enums;

namespace Ramp.Services.QueryHandler.Group
{
    public class AllGroupsBuyProvisionalCompanyQueryHandler :
        QueryHandlerBase<AllGroupsBuyProvisionalCompanyQueryParameter, GroupViewModelLong>
    {
        private readonly IRepository<Domain.Models.Group> _groupRepository;
        private readonly IRepository<Company> _companyRepository;

        public AllGroupsBuyProvisionalCompanyQueryHandler(IRepository<Domain.Models.Group> groupRepository, IRepository<Company> companyRepository)
        {
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
        }

        public override GroupViewModelLong ExecuteQuery(AllGroupsBuyProvisionalCompanyQueryParameter queryParameters)
        {
            var groupViewModelLong = new GroupViewModelLong();

            if (queryParameters.GroupId != null && queryParameters.GroupId != Guid.Empty)
            {
                Domain.Models.Group groupModel = _groupRepository.Find(queryParameters.GroupId);
                if (groupModel != null)
                {
                    groupViewModelLong.GroupViewModel = new GroupViewModel
                    {
                        GroupId = groupModel.Id,
                        Description = groupModel.Description,
                        Title = groupModel.Title
                    };
                    var customerCompany = _companyRepository.Find(groupModel.CompanyId);

                    if (customerCompany != null)
                    {
                        groupViewModelLong.GroupViewModel.SelectedCustomerCompanyId = customerCompany.Id;

                        var provisionalCompany = _companyRepository.Find(customerCompany.ProvisionalAccountLink);

                        groupViewModelLong.GroupViewModel.SelectedProvisionalCompanyId = provisionalCompany.Id;

                    }
                }
            }

           //Get all customer companies by provisional companyId
            var allCustomerCompanyList = _companyRepository.List
                                                        .AsQueryable()
                                                        .Where(g => g.ProvisionalAccountLink == queryParameters.ProvisionalCompanyId && 
                                                                    g.CompanyType == CompanyType.CustomerCompany)
                                                        .ToList();

            foreach (Company company in allCustomerCompanyList)
            {
                //Get all groups for a customer company
                var groupList = _groupRepository.List
                                                .AsQueryable()
                                                .Where(g => g.CompanyId == company.Id)
                                                .ToList();

                foreach (Domain.Models.Group group in groupList)
                {
                    var groupViewModel = new GroupViewModel
                    {
                        GroupId = group.Id,
                        Title = group.Title,
                        Description = group.Description
                    };
                    var companyModel = new CompanyViewModel
                    {
                        Id = group.Company.Id,
                        CompanyName = group.Company.CompanyName
                    };
                    groupViewModel.SelectedCustomerCompanyId = group.CompanyId;
                    groupViewModel.Company = companyModel;
                    groupViewModelLong.GroupList.Add(groupViewModel);
                }
            }

            return groupViewModelLong;
        }
    }
}