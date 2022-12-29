using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class FileUpload : DomainObject
    {
        public virtual byte[] Data { get; set; }
        public virtual string Name { get; set; }
        public virtual string MIMEType { get; set; }
        public virtual FileUploadType FileType { get; set; }
    }
}