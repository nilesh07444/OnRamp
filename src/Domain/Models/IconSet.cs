using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class IconSet : DomainObject
    {
        public string Name { get; set; }
        public virtual IList<Icon> Icons { get; set; } = new List<Icon>();
        public bool? Deleted { get; set; }
        public bool Master { get; set; }
    }
}
