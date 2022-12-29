using Domain.Customer;

namespace Ramp.Contracts.Query.Document
{
    public class DocumentAssignedToUserQuery
    {
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string UserId { get; set; }
    }
}