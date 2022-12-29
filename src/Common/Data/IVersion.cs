using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data
{
    public interface IVersion<TEntity,TId>
    {
        TId Id { get; set; }
        TEntity CurrentVersion { get; set; }
        TEntity LastPublishedVersion { get; set; }
        IList<TEntity> Versions { get; set; }
    }
}
