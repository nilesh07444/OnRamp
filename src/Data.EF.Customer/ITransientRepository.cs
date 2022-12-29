using Common;
using Common.Command;
using Common.Data;
using Domain.Models;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Data.EF.Customer
{
    public interface ITransientRepository<T> : IRepository<T> where T : class
    {
        void SetCustomerCompany(string id);
    }
    public interface ITransientReadRepository<T> : IReadRepository<T> where T : class
    {
        void SetCustomerCompany(string id);
    }
    public class TransientRepository<T> : ITransientRepository<T> where T : class
    {
        private readonly Func<CustomerContext> _factory;
        private CustomerContext _context;
        private ICommandDispatcher _commandDispatcher;
        internal DbSet<T> dbSet;
        public TransientRepository(Func<CustomerContext> factory)
        {
            _factory = factory;
            this.dbSet = factory().Set<T>();
        }
        private void Initialize()
        {
            _context = _factory();
            _commandDispatcher = ServiceLocator.IsLocationProviderSet ? ServiceLocator.Current.GetInstance<ICommandDispatcher>() : null;
        }
        public IEnumerable<T> List => _factory().Set<T>();

        public void Add(T entity)
        {
            Initialize();
            _context.Set<T>().Add(entity);
        }
        public void AddRange(params T[] entities)
        {
            Initialize();
            _context.Set<T>().AddRange(entities.ToArray());
        }
        public void Delete(T entity)
        {
            Initialize();
            _context.Set<T>().Remove(entity);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public T Find(object id)
        {
            Initialize();
            return _context.Set<T>().Find(id);
        }

        public void SaveChanges()
        {
            Initialize();
            _context.SaveChanges();
        }

        public void SetCustomerCompany(string id)
        {
            Initialize();
            if (string.IsNullOrEmpty(id))
                _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
            else
                _commandDispatcher.Dispatch(new UpdateConnectionStringCommand { CompanyId = id });
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
    }
    public class TransientReadRepository<T> : ITransientReadRepository<T> where T : class
    {
        private readonly Func<CustomerContext> _factory;
        private CustomerContext _context;
        private ICommandDispatcher _commandDispatcher;
        internal DbSet<T> dbSet;
        public TransientReadRepository(Func<CustomerContext> factory)
        {
            _factory = factory;
            this.dbSet = _factory().Set<T>();
        }
        private void Initialize()
        {
            _context = _factory();
            _commandDispatcher = ServiceLocator.IsLocationProviderSet ? ServiceLocator.Current.GetInstance<ICommandDispatcher>() : null;
        }
        public IEnumerable<T> List => _factory().Set<T>().AsNoTracking();


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

        public void SetCustomerCompany(string id)
        {
            Initialize();
            if (string.IsNullOrEmpty(id))
                _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
            else
                _commandDispatcher.Dispatch(new UpdateConnectionStringCommand { CompanyId = id });
        }
    }
}
