using System;
using System.Collections.Generic;

namespace Domain.Customer.Models.Document {
	public interface IDocumentNewForParallel {
		string ReferenceId { get; set; }
		string Title { get; set; }
		string Description { get; set; }
		DocumentStatus DocumentStatus { get; set; }
		DocumentCategory Category { get; set; }
		DateTime CreatedOn { get; set; }
		string CreatedBy { get; set; }
		List<StandardUser> Collaborators { get; set; }
		int Points { get; set; }
		Upload CoverPicture { get; set; }
		bool Printable { get; set; }
		DateTime? LastEditDate { get; set; }
		string LastEditedBy { get; set; }
		bool Deleted { get; set; }
		string TrainingLabels { get; set; }
		string CategoryId { get; set; }
	}
}
