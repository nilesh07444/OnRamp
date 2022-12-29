using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;

namespace Ramp.Contracts.Query.Document
{
    public class DocumentTitlesQuery
    {
        public IEnumerable<DocumentIdentifier> Identifiers { get; set; }
    }

	public class DocumentTitlesAndTypeQuery {
		public string DocumentTitle { get; set; }
		public string AdditionalMsg { get; set; }
		public DocumentType DocumentType { get; set; }
		public string DocumentId { get; set; }
		public string Author { get; set; }
		public int Points { get; set; }
		public decimal Passmark { get; set; }
		//changed by n
		public DateTime? ExpiryDate { get; set; }
	}

	public class DocumentIdentifier
    {
        public string DocumentId { get; set; }
        public string AdditionalMsg { get; set; }
        public DocumentType DocumentType { get; set; }
	}
}
