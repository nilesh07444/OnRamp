using Domain.Customer;

namespace Ramp.Contracts.Command.Document
{
    public class UpdateDocumentCollaboratorsCommand
    {
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string[] UserIds { get; set; }
        public string CurrentUser { get; set; }
    }
}