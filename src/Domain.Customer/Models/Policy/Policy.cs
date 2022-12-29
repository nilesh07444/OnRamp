using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models.Document;
using Domain.Enums;

namespace Domain.Customer.Models.Policy
{
    public class Policy : IdentityModel<string>, IDocument
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public virtual List<StandardUser> Collaborators { get; set; }
        public bool Printable { get; set; }
        public bool CallToAction { get; set; }
        public bool IsGlobalAccessed { get; set; }
        public string CallToActionMessage { get; set; }
        public int Points { get; set; }
        public bool Deleted { get; set; }
        public DocumentPreviewMode PreviewMode { get; set; }
        public string TrainingLabels { get; set; }
        public virtual Upload CoverPicture { get; set; }
        public string CoverPictureId { get; set; }
        public virtual DocumentCategory Category { get; set; }
        public string CategoryId { get; set; }
        public DateTime? LastEditDate { get; set; }
        public string LastEditedBy { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;
        public virtual ICollection<PolicyContentBox> ContentBoxes { get; set; } = new List<PolicyContentBox>();

        //added by neeraj
        public DocumentPublishWorkflowStatus? PublishStatus { get; set; }
        public string Approver { get; set; }
        public Guid ApproverId { get; set; }
        public Guid CustomDocummentId { get; set; }
        public bool? IsCustomDocument { get; set; }
    }
}
