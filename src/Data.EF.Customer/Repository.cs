﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Common.Data;

namespace Data.EF.Customer
{
    public class Repository<T> : IRepository<T> where T : class
    {
        static readonly List<string> Initialized = new List<string>();
        private CustomerContext _context;
        internal DbSet<T> dbSet;
        public void SetConnectionString(string str)
        {
            _context.Database.Connection.ConnectionString = str;

            if (!Initialized.Contains(str))
            {
                var initializer = new MigrateDatabaseToLatestVersion<CustomerContext, Migrations.Configuration>(true);
                initializer.InitializeDatabase(_context);
                Initialized.Add(str);
            }
        }

        public Repository(CustomerContext context)
        {
            _context = context;
            this.dbSet = context.Set<T>();
        }

        protected DbContext DbContext { get; set; }


        public virtual T SaveOrUpdate(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            // _context.Set<T>().Attach(entity);
            _context.SaveChanges();
            return entity;
        }

        public IEnumerable<T> List
        {
            get { return _context.Set<T>().AsQueryable(); }
        }

        public T Get(object id)
        {
            return _context.Set<T>().Find(id);
        }


        /// <summary>
        ///     Gets all records as an IEnumberable
        /// </summary>
        /// <returns>An IEnumberable object containing the results of the query</returns>
        /// <summary>
        ///     Finds a record with the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A collection containing the results of the query</returns>
        //public List<T> Find(Func<T, bool> predicate)
        public T Find(object id)
        {
            return _context.Set<T>().Find(id);
            //return _objectSet.Find()
            /*return _objectSet.Where(predicate).ToList();*/
        }

        /// <summary>
        ///     Deletes the specified entitiy
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity" /> is null</exception>
        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }

        /// <summary>
        ///     Adds the specified entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity" /> is null</exception>
        public void Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            _context.Set<T>().Add(entity);
            _context.SaveChanges();

        }
        public void AddRange(params T[] entities)
        {
            _context.Set<T>().AddRange(entities.ToArray());
        }
        /// <summary>
        ///     Attaches the specified entity
        /// </summary>
        /// <param name="entity">Entity to attach</param>
        public void Attach(T entity)
        {
            _context.Set<T>().Attach(entity);
        }

        /// <summary>
        ///     Saves all context changes
        /// </summary>
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void SaveChanges(SaveOptions options)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Releases all resources used by the WarrantManagement.DataExtract.Dal.ReportDataBase
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<T> GetAll()
        {
            return _context.Set<T>().Cast<T>().ToList();
            //throw new NotImplementedException();
        }

        /// <summary>
        ///     Saves all context changes with the specified SaveOptions
        /// </summary>
        /// <param name="options">Options for saving the context</param>
        public void SaveChanges(System.Data.Entity.Core.Objects.SaveOptions options)
        {
            _context.SaveChanges();
        }

        /// <summary>
        ///     Deletes records matching the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        public void Delete(Func<T, bool> predicate)
        {
            /* IEnumerable<T> records = from x in _objectSet.Where(predicate) select x;

             foreach (T record in records)
             {
                 _objectSet.Remove(record);
             }
             _context.SaveChanges();*/
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Releases all resources used by the WarrantManagement.DataExtract.Dal.ReportDataBase
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether or not to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
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


        //public IEnumerable<T> Get(Func<T, bool> expression)
        //{
        //    return _context.Set<T>().Where(expression).AsQueryable();
        //}
    }
}