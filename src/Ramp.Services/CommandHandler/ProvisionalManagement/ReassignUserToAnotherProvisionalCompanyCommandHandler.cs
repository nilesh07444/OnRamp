using System;
using Common.Data;
using Common.Command;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;

namespace Ramp.Services.CommandHandler.ProvisionalManagement
{
    public class ReassignUserToAnotherProvisionalCompanyCommandHandler :
        CommandHandlerBase<ReassignUserToAnotherProvisionalCompanyCommand>
    {
        private readonly IRepository<Company> _companyRepository;

        public ReassignUserToAnotherProvisionalCompanyCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CommandResponse Execute(ReassignUserToAnotherProvisionalCompanyCommand command)
        {
            foreach (Guid companyId in command.CustomerCompanyGuidList)
            {
                Company company = _companyRepository.Find(companyId);
                company.ProvisionalAccountLink = command.ToProvisionalCompanyId;
                _companyRepository.SaveChanges();
            }
            return null;
        }
    }
}