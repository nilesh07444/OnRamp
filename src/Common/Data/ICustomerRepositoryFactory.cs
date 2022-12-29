using System;

namespace Common.Data
{
    public interface ICustomerRepositoryFactory
    {
        string GetConnectionString(Guid customerId);
        IRepository<T> GetRepository<T>(Guid customerId) where T : class;
        IRepository<T> GetRepository<T>(string connectionString) where T : class;
    }
}