using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Command
{
    public class DeleteByIdCommand<TEntity> :IdentityModel<string> where TEntity : class
    {
    }
}
