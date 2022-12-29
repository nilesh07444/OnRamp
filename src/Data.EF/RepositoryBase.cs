using Domain.Data;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.EF
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
         where TEntity : DomainObject
    {
        protected IContext Context { get; set; }

        public RepositoryBase(IContext context)
        {
            this.Context = context;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.Context.GetSet<TEntity>().AsEnumerable();
        }
    }
}
