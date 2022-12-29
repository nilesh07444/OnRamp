using System;
using System.Linq;

namespace Domain.Data
{
    public interface IUnitOfWork : IDisposable
    {
        void Join(IUnitOfWork work);
        void Commit();

    }
}