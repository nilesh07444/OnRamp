using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.RecurringJob
{
    public interface IRecurringJob
    {
        string When { get; }
        void Work();
    }
}
