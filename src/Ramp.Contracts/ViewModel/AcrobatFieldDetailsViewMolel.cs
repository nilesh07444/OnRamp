using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
  public  class AcrobatFieldDetailsViewModel
    {
        public DateTime GeneratedDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentTitle { get; set; }
        public string PassRequirement { get; set; }
        public IList<string> Groups { get; set; }
        public IList<AcrobatFieldtableModel> AcrobatTableModel { get; set; }
        public IList<AcrobatFieldDetailsModel> GlobalInteractions { get; set; }

        public class AcrobatFieldDetailsModel
        {
            public Guid UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FieldName { get; set; }
            public string FieldValue { get; set; }
            public string CustomDocumentID { get; set; }
            
            public DateTime ViewDate { get; set; }
            
            public TimeSpan Duration { get; set; }
          
        }
        public class AcrobatFieldtableModel
        {
            public Guid UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FieldName { get; set; }
            public string FieldValue { get; set; }
            public string CustomDocumentID { get; set; }
            
            public DateTime ViewDate { get; set; }
            
            public TimeSpan Duration { get; set; }
          
        }
    }
}
