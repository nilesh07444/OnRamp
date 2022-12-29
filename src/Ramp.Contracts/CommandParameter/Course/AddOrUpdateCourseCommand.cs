using Domain.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Course {
	public class AddOrUpdateCourseCommand{

		public Guid Id { get; set; }
		public Guid CreatedBy { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DocumentStatus Status { get; set; }
		public int Points { get; set; }
		public UploadResultViewModel CoverPicture { get; set; }
		public bool GlobalAccessEnabled { get; set; }
		public bool ExpiryEnabled { get; set; }
		public int ExpiryInDays { get; set; }
		public string AchievementId { get; set; }
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

		public string[] AssignedUsers { get; set; }
		public DocumentStatus CourseStatus { get; set; }
		public List<NewDocument> Documents { get; set; }
	}
}
