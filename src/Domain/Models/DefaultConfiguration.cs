using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class DefaultConfiguration : DomainObject
    {
        public DefaultConfiguration()
        {
            Trophys = new List<FileUpload>();
        }

        public virtual List<FileUpload> Trophys { get; set; }
        public virtual FileUpload Certificate { get; set; }
    }
}