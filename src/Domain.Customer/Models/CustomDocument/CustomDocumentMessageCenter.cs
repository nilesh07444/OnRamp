using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;

namespace Domain.Customer.Models
{
    public class CustomDocumentMessageCenter : IdentityModel<Guid>
    {
        public string UserId { get; set; }
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public DateTime CreatedOn { get; set; }        
		public bool IsGlobalAccessed { get; set; }
		public DocumentUsageStatus? Status { get; set; }
		public string Messages { get; set; }
        public string AssignedDocumentId { get; set; }


    }

	
}
