using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model;

namespace Domain.Data
{
    //public interface IRepository<T>
    //{
    //    IEnumerable<T> Select { get; }
    //    void Save(T entity);
    //    void Delete(T entity);
    //}

    //Added new code
    public interface IRepository<TEntity> where TEntity : DomainObject
    {
        // Define generic CRUD functions.

        IEnumerable<TEntity> GetAll();
    }
}