using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Command
{
    public interface ICommandHandlerAndValidator<T> : ICommandHandlerBase<T>,IValidator<T>
    {
    }
}
