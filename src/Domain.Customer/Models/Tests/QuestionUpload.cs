using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class QuestionUpload : Base.CustomerDomainObject
    {
        public virtual TrainingDocumentTypeEnum DocumentType { get; set; }
        public string DocumentName { get; set; }
        public byte[] DocumentFileContent { get; set; }
        public virtual TrainingQuestion TrainingQuestion { get; set; }
        public virtual FileUploads Upload { get; set; }
    }
}