using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class GlobalSearchViewModel : IViewModel
    {
        public string SearchText { get; set; }

        public string SearchType { get; set; }

        public string SearchUrl { get; set; }

        public bool IsCustAdmin { get; set; }

        public Guid UserId { get; set; }

        public string FoundIn { get; set; }


    }
}
