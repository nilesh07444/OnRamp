using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.PackageManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.CommandHandler.PackageManagement
{
    //TODO: Delete when completely unused
    //public class DeletePackageCommandHandler : CommandHandlerBase<DeletePackageCommand>
    //{
    //    private readonly IRepository<Company> _companyRepository;
    //    private readonly IRepository<Package> _packageRepository;

    //    public DeletePackageCommandHandler(IRepository<Company> companyRepository, IRepository<Package> packageRepository)
    //    {
    //        _companyRepository = companyRepository;
    //        _packageRepository = packageRepository;
    //    }

    //    public override CommandResponse Execute(DeletePackageCommand command)
    //    {
    //        var customerCompanies = _companyRepository.List.Where(c => c.PackageId == command.PackageId && c.PhysicalAddress != "Dummy").ToList();
    //        foreach (var customerCompany in customerCompanies)
    //        {
    //            _companyRepository.Delete(customerCompany);
    //            _companyRepository.SaveChanges();
    //        }
    //        var package = _packageRepository.Find(command.PackageId);
    //        _packageRepository.Delete(package);
    //        _packageRepository.SaveChanges();

    //        return null;
    //    }
    //}
}