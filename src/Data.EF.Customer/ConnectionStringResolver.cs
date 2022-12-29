using Common;
using Common.Data;
using Common.Query;
using Domain.Models;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.EF.Customer
{
    public interface IConnectionStringResolver
    {
        string Resolve();
    }
    public class ConnectionStringResolver : IConnectionStringResolver
    {
        [ThreadStatic]
        public static string _companyId;
        private IQueryExecutor _queryExecutor;
        private IRepository<Company> _companyRepository;
        private void Initialise()
        {
            _queryExecutor = ServiceLocator.IsLocationProviderSet ? ServiceLocator.Current.GetInstance<IQueryExecutor>() : null;
            _companyRepository = ServiceLocator.IsLocationProviderSet ? ServiceLocator.Current.GetInstance<IRepository<Company>>() : null;
        }
        public string Resolve()
        {
            Initialise();
            return _queryExecutor.Execute<ConnectionStringQuery, string>(new ConnectionStringQuery { CompanyName = _companyRepository.Find(_companyId.ConvertToGuid())?.CompanyName });
        }
    }
}
