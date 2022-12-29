using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
   public class CKEUpload :Base.CustomerDomainObject
    {
        public TraningGuideChapter TrainingGuideChapter { get; set; }
        public string Type { get; set; }
        public virtual FileUploads Upload { get; set; }
    }
}
