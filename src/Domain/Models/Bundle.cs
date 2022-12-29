using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Bundle: IdentityModel<string>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int MaxNumberOfDocuments { get; set; }
        public bool IsForSelfProvision { get; set; }
    }
}
