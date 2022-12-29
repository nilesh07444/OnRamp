using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data
{
    public abstract class IdentityModel<T> 
    {
        public T Id { get; set; }
    }
}
