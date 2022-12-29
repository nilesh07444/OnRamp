using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.Command.Bundle;

namespace Ramp.Services.CommandHandler
{
    public class BundleCommandHandler : ICommandHandlerBase<DeleteBundleCommand>,
                                        ICommandHandlerBase<CreateOrUpdateBundleCommand>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Bundle> _bundleRepository;

        public BundleCommandHandler(IRepository<Company> companyRepository, IRepository<Bundle> bundleRepository)
        {
            _companyRepository = companyRepository;
            _bundleRepository = bundleRepository;
        }

        public CommandResponse Execute(DeleteBundleCommand command)
        {
            var customerCompanies = _companyRepository.List.Where(c => c.BundleId == command.BundleId);
            foreach (var customerCompany in customerCompanies)
            {
                _companyRepository.Delete(customerCompany);
                _companyRepository.SaveChanges();
            }

            var bundle = _bundleRepository.Find(command.BundleId);
            _bundleRepository.Delete(bundle);
            _bundleRepository.SaveChanges();

            return null;
        }

        public CommandResponse Execute(CreateOrUpdateBundleCommand command)
        {
            var bundle = _bundleRepository.Find(command.Id);

            if (command.IsForSelfProvision == true)
            {
                var bundles = _bundleRepository.GetAll();
                var provisionalBundle = bundles.FirstOrDefault(p => p.IsForSelfProvision);

                if (provisionalBundle != null)
                {
                    provisionalBundle.IsForSelfProvision = false;
                    _bundleRepository.SaveChanges();
                }
            }

            if (bundle == null)
            {
                var bundleModel = new Bundle
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = command.Title,
                    Description = command.Description,
                    MaxNumberOfDocuments = command.MaxNumberOfDocuments,
                    IsForSelfProvision = command.IsForSelfProvision
                };
                _bundleRepository.Add(bundleModel);
                _bundleRepository.SaveChanges();
            }
            else
            {
                bundle.Title = command.Title;
                bundle.Description = command.Description;
                bundle.MaxNumberOfDocuments = command.MaxNumberOfDocuments;
                bundle.IsForSelfProvision = command.IsForSelfProvision;

                _bundleRepository.SaveChanges();
            }
            return null;
        }
    }
}
