using Common.Data;
using Domain.Customer.Models.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models {
	public class Course : Base.CustomerDomainObject {

		//public Guid Id { get; set; }
		public Guid CreatedBy { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DocumentStatus Status { get; set; }
		public int Points { get; set; }
		public string CoverPicture { get; set; }
		public bool GlobalAccessEnabled { get; set; }
		public bool ExpiryEnabled { get; set; }
		public int ExpiryInDays { get; set; }
		public Guid AchievementId { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime? EditedOn { get; set; }
		public DateTime? DeletedOn { get; set; }
		public bool IsActive { get; set; }
		public bool IsDeleted { get; set; }
		public bool WorkflowEnabled { get; set; }
		public string AllocatedAdmins { get; set; }
		public Guid CategoryId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public virtual ICollection<AssociatedDocument> Documents { get; set; } = new List<AssociatedDocument>();

	}
}
