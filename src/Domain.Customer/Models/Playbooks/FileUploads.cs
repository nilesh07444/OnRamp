using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class FileUploads : Base.CustomerDomainObject
    {
        public string Type { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
    }
    public class Upload : IdentityModel<string>
    {
        public string Type { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public int Order { get; set; }
        public string Content { get; set; }
    }
}
