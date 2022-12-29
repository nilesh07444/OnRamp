using Common.Data;
using Domain.Customer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.EF.Customer
{
    public static class TransientReadRepositoryExtensions
    {
        public static T Find<T>(this ITransientReadRepository<T> repo,string id) where T : IdentityModel<string>
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            return repo.List.AsQueryable().FirstOrDefault(e => e.Id == id);
        }
        public static T Find<T>(this ITransientReadRepository<T> repo, Guid? id) where T : CustomerDomainObject
        {
            if (!id.HasValue)
                return null;
            return repo.List.AsQueryable().FirstOrDefault(e => e.Id == id.Value);
        }
    }
}
