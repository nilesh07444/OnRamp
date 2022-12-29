using Common.Data;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Ramp.Services
{
    internal static class CustomerDbRepositoryHelper
    {
        public static IEnumerable<T> GetAll<T>(this IRepository<T> repository)
            where T : class
        {
            return repository.List;
        }

        public static IRepository<T> GetCustomerRepository<T>(string connectionString)
            where T : class
        {
            return ServiceLocator.Current.GetInstance<ICustomerRepositoryFactory>().GetRepository<T>(connectionString);
        }
    }
}
