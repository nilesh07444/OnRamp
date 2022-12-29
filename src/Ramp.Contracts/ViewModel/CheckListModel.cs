using Domain.Customer;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel {

	public class CheckListListModel : DocumentListModel {
	}

	public	class CheckListModel: CheckListListModel {

		public CheckListModel() {
			ContentModels = new List<CheckListChapterModel>();
			NotificationInteval = new TestExpiryNotificationInterval();
			PreviewMode = new DocumentPreviewMode();
		}

		public int? CheckListExpiresNumberDaysFromAssignment { get; set; }
		public TestExpiryNotificationInterval NotificationInteval { get; set; }
		public int? NotificationIntevalDaysBeforeExpiry { get; set; }
		public bool IndivisualReportClick { get; set; } = false;
		public DocumentPreviewMode PreviewMode { get; set; }
		public object CoverPictureUpload { get; set; }
		public IEnumerable<CheckListChapterModel> ContentModels { get; set; }
		public bool Status { get; set; }
		public DateTime SubmittedDate { get; set; }
		//public List<DocumentUrlViewModel> DocLinks { get; set; }
	}

}
