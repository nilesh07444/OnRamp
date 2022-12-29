using Common.Data;
using Domain.Models;
using Microsoft.Practices.ServiceLocation;
using System;

namespace Data.EF.Customer
{
    public class CompanyRepositoryFactory : ICustomerRepositoryFactory
    {
        private readonly IRepository<Company> _companyRepository;

        public CompanyRepositoryFactory(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public string GetConnectionString(Guid customerId)
        {
            var company = _companyRepository.Find(customerId);
            return company.CompanyConnectionString;
        }

        public IRepository<T> GetRepository<T>(Guid customerId) where T : class
        {
            return GetRepository<T>(GetConnectionString(customerId));
        }

        public IRepository<T> GetRepository<T>(string connectionString) where T : class
        {
            var repo = ServiceLocator.Current.GetInstance<IRepository<T>>();

            var r = repo as Repository<T>;
            r.SetConnectionString(connectionString);

            return repo;
        }
    }
}