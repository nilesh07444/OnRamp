using System;
using Common.Data;
using Common.Command;
using Domain.Models;
using System.Linq;
using Ramp.Contracts.CommandParameter.PackageManagement;

namespace Ramp.Services.CommandHandler.PackageManagement
{
    public class PackageCommandHandler : CommandHandlerBase<PackageCommandParameter>
    {
        private readonly IRepository<Package> _packageRepository;

        public PackageCommandHandler(IRepository<Package> packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public override CommandResponse Execute(PackageCommandParameter command)
        {
            Package packageModel = _packageRepository.Find(command.Id);

            if (command.IsForSelfProvision == true) 
            {
                var package = _packageRepository.GetAll();
                Package provisionalPackage = package.Where(p => p.IsForSelfProvision == true).FirstOrDefault(); 

                if (provisionalPackage != null) 
                {
                    provisionalPackage.IsForSelfProvision = false; 
                    _packageRepository.SaveChanges(); 
                } 
            }

            if (packageModel == null)
            {
                var package = new Package
                {
                    Id = Guid.NewGuid(),
                    Title = command.Title,
                    Description = command.Description,
                    MaxNumberOfChaptersPerGuide = command.MaxNumberOfChaptersPerGuide,
                    MaxNumberOfGuides = command.MaxNumberOfGuides,
                    IsForSelfProvision = command.IsForSelfProvision
                };
                _packageRepository.Add(package);
                _packageRepository.SaveChanges();
            }
            else
            {
                packageModel.Title = command.Title;
                packageModel.Description = command.Description;
                packageModel.MaxNumberOfChaptersPerGuide = command.MaxNumberOfChaptersPerGuide;
                packageModel.MaxNumberOfGuides = command.MaxNumberOfGuides;
                packageModel.IsForSelfProvision = command.IsForSelfProvision;

                _packageRepository.SaveChanges();
            }
            return null;
        }
    }
}