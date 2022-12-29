using Data.EF.Customer;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Web.UI.Code.Decorators
{
    public class TransientRepositoryDecorator<T> : ITransientRepository<T> where T : class
    {
        Func<ITransientRepository<T>> _factory;
        internal DbSet<T> dbSet;
        public TransientRepositoryDecorator(Func<ITransientRepository<T>> factory)
        {
            _factory = factory;
        }
        public IEnumerable<T> List => _factory().List;

        public void Add(T entity)
        {
            _factory().Add(entity);
        }

        public void AddRange(params T[] entities)
        {
            _factory().AddRange(entities.ToArray());
        }
        public void Delete(T entity)
        {
            _factory().Delete(entity);
        }

        public void Dispose()
        {
            _factory().Dispose();
        }

        public T Find(object id)
        {
            return _factory().Find(id);
        }

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

        public void SaveChanges()
        {
            _factory().SaveChanges();
        }

        public void SetCustomerCompany(string id)
        {
            _factory().SetCustomerCompany(id);
        }
    }
}