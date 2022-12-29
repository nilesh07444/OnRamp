using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public interface IEventHandler<T> where T : IEvent
    {
        void Handle(T @event);
    }
}