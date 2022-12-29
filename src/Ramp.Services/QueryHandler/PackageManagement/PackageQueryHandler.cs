using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.PackageManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.PackageManagement
{
    // TODO: Delete when completely unused
    public class PackageQueryHandler : QueryHandlerBase<PackageQueryParameter, PackageViewModel>
    {
        private readonly IRepository<Package> _packageRepository;

        public PackageQueryHandler(IRepository<Package> packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public override PackageViewModel ExecuteQuery(PackageQueryParameter queryParameters)
        {
            var packageViewModel = new PackageViewModel();

            if (queryParameters.id != null && queryParameters.id != Guid.Empty)
            {
                Package packageModel = _packageRepository.Find(queryParameters.id);
                if (packageModel != null)
                {
                    packageViewModel.PackageViewModelShort = new PackageViewModelShort
                    {
                        Id = packageModel.Id,
                        Title = packageModel.Title,
                        Description = packageModel.Description,
                        MaxNumberOfGuides = packageModel.MaxNumberOfGuides,
                        MaxNumberOfChaptersPerGuide = packageModel.MaxNumberOfChaptersPerGuide,
                        IsForSelfProvision = packageModel.IsForSelfProvision
                    };
                }
            }
            //Package result = _packageRepository.GetAll().FirstOrDefault();
            List<Package> packageList = _packageRepository.List.Where(p => p.Title != "Dummy").ToList();
            foreach (Package package in packageList)
            {
                var packageViewModelShort = new PackageViewModelShort
                {
                    Id = package.Id,
                    Title = package.Title,
                    Description = package.Description,
                    MaxNumberOfChaptersPerGuide = package.MaxNumberOfChaptersPerGuide,
                    MaxNumberOfGuides = package.MaxNumberOfGuides,
                    IsForSelfProvision = package.IsForSelfProvision
                };
                packageViewModel.PackageViewModelList.Add(packageViewModelShort);
            }
            return packageViewModel;
        }
    }
}