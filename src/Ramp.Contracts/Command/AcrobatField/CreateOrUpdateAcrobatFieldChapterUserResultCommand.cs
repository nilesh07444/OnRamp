using Common.Data;
using Domain.Customer.Models;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.Command.AcrobatField {

public	class CreateOrUpdateAcrobatFieldChapterUserResultCommand : IdentityModel<string> {

		public string AssignedDocumentId { get; set; }
		public string AcrobatFieldChapterId { get; set; }
		public bool IsChecked { get; set; }
		public string IssueDiscription { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ChapterTrackedDate { get; set; }

		#region Global accesssed
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion
	}

	public class CreateOrUpdateAcrobatFieldValueCommand 
	{
	public List<StandardUserAdobeFieldValues> AcrobatFieldList { get; set; }
		 


	}

}
