using System;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Portal;
using Ramp.Contracts.ViewModel;
using System.Linq;
using Common.Events;
using Common.Command;
using Ramp.Contracts.CommandParameter.Portal;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using System.Collections.Generic;
using Ramp.Services.Projection;

namespace Ramp.Services.QueryHandler.Portal
{
    public class PortalQueryHandler : QueryHandlerBase<PortalQueryParameter, PortalContextViewModel>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IQueryExecutor _executor;
        private readonly IRepository<IconSet> _iconSetRepository;
        public PortalQueryHandler(IRepository<Company> companyRepository, ICommandDispatcher dispatcher,
            IQueryExecutor executor, IRepository<IconSet> iconSetRepository)
        {
            _companyRepository = companyRepository;
            _dispatcher = dispatcher;
            _executor = executor;
            _iconSetRepository = iconSetRepository;
        }

        public override PortalContextViewModel ExecuteQuery(PortalQueryParameter queryParameters)
        {
            Company company = null;
            if (!string.IsNullOrEmpty(queryParameters.Subdomain))
                company =_companyRepository.List.AsQueryable().FirstOrDefault(c => c.LayerSubDomain == queryParameters.Subdomain);
            if (queryParameters.CompanyId.HasValue)
                company = _companyRepository.List.SingleOrDefault(c => c.Id.Equals(queryParameters.CompanyId));


            if (company == null)
                return null;

            var result = new PortalContextViewModel();
            if (company.ClientSystemName != null)
                result.Name = company.ClientSystemName;
            if (company.ProvisionalAccountLink != Guid.Empty)
                result.Reseller = _companyRepository.Find(company.ProvisionalAccountLink).ClientSystemName;
            result.Type = company.CompanyType;
            result.UserCompany = Project.CompanyViewModelFrom(company);
            result.LogoFileName = result.UserCompany.CustomerConfigurations.Any(x => x.Type == CustomerConfigurationType.DashboardLogo) ? null : company.LogoImageUrl;
            result.UserCompany.CustomColours = _executor.Execute<CustomColourQuery, CustomColourViewModel>(new CustomColourQuery { CompanyId = company.Id });
            result.Icons = GetCompanyIcons(company);
			
            return result;
        }
        private IDictionary<IconType,string> GetCompanyIcons(Company company)
        {
            var defaultSet = _iconSetRepository.List.Where(x => x.Master).FirstOrDefault();
            var icons = new Dictionary<IconType, string>();
            foreach (var v in Enum.GetValues(typeof(IconType)))
            {
                icons.Add((IconType)v, reconsile((IconType)v, company.IconSet, defaultSet));
            }
            return icons;
        }
        private string reconsile(IconType type,IconSet companySet,IconSet defaultSet)
        {
            string id = null;
            if(companySet == null && defaultSet == null)
            {
                //map some other means
            }
            if (companySet == null || companySet.Icons.FirstOrDefault(x => x.IconType == type) == null)
                id = defaultSet.Icons.FirstOrDefault(x => x.IconType == type)?.Upload?.Id.ToString();
            else
                id = companySet.Icons.FirstOrDefault(x => x.IconType == type)?.Upload?.Id.ToString();
			
            return id;

                
        }
    }
}