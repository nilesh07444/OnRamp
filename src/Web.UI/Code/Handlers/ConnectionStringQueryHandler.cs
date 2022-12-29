using Common;
using Common.Data;
using Common.Query;
using Domain.Models;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Handlers
{
    public class ConnectionStringQueryHandler : IQueryHandler<ConnectionStringQuery, string>
    {
        public string ExecuteQuery(ConnectionStringQuery query)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CustomerContext"].ConnectionString;
            if (string.IsNullOrWhiteSpace(query.CompanyName))
                connectionString = connectionString?.Replace("DBNAME", $"{PortalContext.Current.UserCompany.CompanyName.Replace(" ", string.Empty)}");
            else
                connectionString = connectionString?.Replace("DBNAME", $"{query.CompanyName?.Replace(" ", string.Empty)}");
            return connectionString?.Replace("'", string.Empty);
        }
    }
}