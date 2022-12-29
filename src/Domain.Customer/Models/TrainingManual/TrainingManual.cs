using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models.Document;
using Domain.Enums;

namespace Domain.Customer.Models.TrainingManual
{
    public class TrainingManual : IdentityModel<string>, IDocument
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public virtual List<StandardUser> Collaborators { get; set; }
        public bool Printable { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public int Points { get; set; }
        public bool Deleted { get; set; }
        public DocumentPreviewMode PreviewMode { get; set; }
        public virtual Upload CoverPicture { get; set; }
        public string CoverPictureId { get; set; }
        public virtual DocumentCategory Category { get; set; }
        public string DocumentCategoryId { get; set; }
        public DateTime? LastEditDate { get; set; }
        public string LastEditedBy { get; set; }
        public string TrainingLabels { get; set; }
		public virtual ICollection<TrainingManualChapter> Chapters { get; set; } = new List<TrainingManualChapter>();

		//added by neeraj
		public DocumentPublishWorkflowStatus? PublishStatus { get; set; }
		public string Approver { get; set; }
		public Guid ApproverId { get; set; }
        public Guid CustomDocummentId { get; set; }
        public bool? IsCustomDocument { get; set; }
	}


}
