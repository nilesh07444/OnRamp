using Common.Query;
using Ramp.Contracts.Query.Document;
using System;

namespace Ramp.Contracts.Query.Course {
	public class CourseListQuery : DocumentListQueryBase, IPagedQuery {
		public int Page { get; set; }
		public int? PageSize { get; set; }
		public bool? EnableChecklistDocument { get; set; }
		public bool IsRecycleBin { get; set; } = false;
		public string userID { get; set; }
	}
}
