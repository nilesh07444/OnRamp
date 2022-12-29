using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Common.Data;

namespace Data.EF.Customer
{
    public class ReadRepository<T> : IReadRepository<T> where T : class
    {
        private CustomerContext _context;
        internal DbSet<T> dbSet;
        public ReadRepository(CustomerContext context)
        {
            _context = context;
            this.dbSet = context.Set<T>();
        }

        public IEnumerable<T> List => _context.Set<T>().AsNoTracking();

        public virtual IEnumerable<T> Get(
               Expression<Func<T, bool>> filter = null,
               Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
               string includeProperties = "")
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

    }
}
