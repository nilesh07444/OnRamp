using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class UploadCustomCSSCommandHandler : CommandHandlerBase<UploadCustomCSSCommandParameter>
    {
        private readonly IRepository<Company> _companyRepository;

        public UploadCustomCSSCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CommandResponse Execute(UploadCustomCSSCommandParameter command)
        {
            Company companyModel = _companyRepository.Find(command.CompanyId);
            companyModel.ApplyCustomCss = true;
            companyModel.customCssFile = command.CSSFile;

            _companyRepository.SaveChanges();
            return null;
        }
    }
}
