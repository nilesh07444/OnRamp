using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;

namespace Ramp.Contracts.Query.Document
{
    public class UsersAssignedDocumentQuery
    {
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string[] GroupIds { get; set; }
		public string[] AllDocumentId { get; set; }
	}
}
