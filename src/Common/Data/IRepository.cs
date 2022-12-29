using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Xml.Linq;

namespace Common.Data
{
    public interface IRepository<T> : IDisposable where T : class
    {
        IEnumerable<T> List { get; }
        T Find(object id);
        void Add(T entity);
        void AddRange(params T[] entities);
        void Delete(T entity);
        void SaveChanges();
        IEnumerable<T> Get(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "");
    }
    public interface IReadRepository<T> where T : class
    {
        IEnumerable<T> List { get; }
        IEnumerable<T> Get(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "");
    }
}