using Domain.Customer;

namespace Ramp.Contracts.Query.Document
{
    public class DocumentCollaboratorsQuery
    {
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}