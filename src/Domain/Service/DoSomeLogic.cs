using Domain.Data;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Domain.Service
{
    public class DoSomeLogic : IDoSomeLogic
    {
        private IPersonRepository Repository { get; set; }

        public DoSomeLogic(IPersonRepository repo)
        {
            this.Repository = repo;
        }

        public IEnumerable<Person> GetPeople()
        {
            return this.Repository.GetAll();
        }
    }
}
