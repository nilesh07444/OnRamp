using Domain.Customer;

namespace Ramp.Contracts.Query.Document
{
    public class DocumentQuery
    {
        public string Id { get; set; }
        public string AdditionalMsg { get; set; }
        public DocumentType? DocumentType { get; set; }
    }
}