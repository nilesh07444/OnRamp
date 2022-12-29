using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Policy;
using System;

namespace Domain.Customer.Models.Policy
{
	public class PolicyContentBoxUserUploadResult : IdentityModel<string>
	{
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string PolicyContentBoxId { get; set; }
		public virtual PolicyContentBox PolicyContentBox { get; set; }
		public string UploadId { get; set; }
		public bool isSignOff { get; set; }
		public virtual Upload Upload { get; set; }
		public DateTime CreatedDate { get; set; }



		#region For Global access
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion

	}
}
