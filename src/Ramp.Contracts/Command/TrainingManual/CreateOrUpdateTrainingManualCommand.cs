using Common.Data;
using Domain.Customer;
using Domain.Enums;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.TrainingManual
{
    public class CreateOrUpdateTrainingManualCommand : IdentityModel<string>
    {
        public string ReferenceId { get; set; }
		public string DocumentUrl { get; set; }
		public string Title { get; set; }
        public string Description { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public bool Printable { get; set; }
        public bool Deleted { get; set; }
		public bool IsGlobalAccessed { get; set; }
        public int Points { get; set; }
        public DateTime? LastEditDate { get; set; }
        public DocumentPreviewMode PreviewMode { get; set; }
        public string TrainingLabels { get; set; } = string.Empty;
        public object CoverPictureUpload { get; set; }
        public UploadResultViewModel CoverPicture { get; set; }
        public CategoryViewModelShort Category { get; set; }
        public IEnumerable<TrainingManualChapterModel> ContentModels { get; set; } = new List<TrainingManualChapterModel>();
        public IEnumerable<Domain.Customer.Models.StandardUser> Collaborators { get; set; }

		//added by neeraj
		public DocumentPublishWorkflowStatus? PublishStatus { get; set; }
		public string Approver { get; set; }
		public Guid? ApproverId { get; set; }
	}
}
