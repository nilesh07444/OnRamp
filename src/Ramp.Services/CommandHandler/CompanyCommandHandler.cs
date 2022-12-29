using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.Command.Company;

namespace Ramp.Services.CommandHandler
{
    public class CompanyCommandHandler : ICommandHandlerBase<UpdateCompanySelfSignUpSettingsCommand>
    {
        private readonly IRepository<Company> _companyRepository;

        public CompanyCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public CommandResponse Execute(UpdateCompanySelfSignUpSettingsCommand command)
        {
            var company = _companyRepository.Find(command.Id);

            if (company != null)
            {
                company.IsForSelfSignUp = command.IsForSelfSignUp;
                company.IsSelfSignUpApprove = command.IsSelfSignUpApprove;
				company.IsEmployeeCodeReq = command.IsEmployeeCodeReq;
				company.CompanySiteTitle = command.CompanySiteTitle;
				command.IsEnabledEmployeeCode = command.IsEnabledEmployeeCode;

	

				_companyRepository.SaveChanges();
            }

            return null;
        }
    }
}
