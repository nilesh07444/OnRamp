using Common.Data;
using Common.Command;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;

namespace Ramp.Services.CommandHandler.ProvisionalManagement
{
    public class UpdateProvisionalCompanyStatusCommandHandler :
        CommandHandlerBase<UpdateProvisionalCompanyStatusCommand>
    {
        private readonly IRepository<Company> _companyRepository;

        public UpdateProvisionalCompanyStatusCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CommandResponse Execute(UpdateProvisionalCompanyStatusCommand command)
        {
            Company company = _companyRepository.Find(command.ProvisionalCompanyId);
            company.IsActive = command.ProvisionalCompanyStatus;
            _companyRepository.SaveChanges();
            return null;
        }
    }
}