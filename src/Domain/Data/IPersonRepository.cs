using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model;
//using System.Threading.Tasks;

namespace Domain.Data
{
    public interface IPersonRepository : IRepository<Person>
    {
        // Define some person specific repository functions
    }
}
