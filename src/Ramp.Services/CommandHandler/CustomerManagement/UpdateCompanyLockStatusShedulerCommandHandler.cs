using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class UpdateCompanyLockStatusShedulerCommandHandler : CommandHandlerBase<UpdateCompanyLockStatusShedulerCommandParameter>
    {
         private readonly IRepository<Company> _companyRepository;

         public UpdateCompanyLockStatusShedulerCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

         public override CommandResponse Execute(UpdateCompanyLockStatusShedulerCommandParameter command)
        {
            Company companyModel = _companyRepository.Find(command.CompanyId);
            companyModel.IsLock = command.LockStatus;
            _companyRepository.SaveChanges();
            return null;
        }
    }
}
