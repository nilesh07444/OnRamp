using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Document {
	public class DocumentNotification {
		public int Id { get; set; }
		public string DocumentId { get; set; }
		public string UserId { get; set; }
		public DateTime AssignedDate { get; set; }
		public bool IsViewed { get; set; }
		public string NotificationType { get; set; }
		public string Message { get; set; }
	}
}
