using System;
using System.Collections;
using System.Collections.Generic;
using Common.Data;
using Domain.Customer;
using Domain.Enums;
using VirtuaCon;

namespace Ramp.Contracts.ViewModel
{
    public class PointsStatementViewModel
    {
		public PointsStatementViewModel() {
			
			GlobalAccessDict.Add(0, "Assigned");
			GlobalAccessDict.Add(1, "Global");
		}
        public IEnumerable<CompanyViewModel> Companies { get; set; }
        public IEnumerable<DocumentCategoryViewModel> Categories { get; set; }
        public IEnumerable<GroupViewModel> Groups { get; set; }
        public IEnumerable<UserViewModel> Users { get; set; }
        public IEnumerable<TrainingActivityModel> TrainingActivities { get; set; }
        public IDictionary<int, string> DocumentTypesDict { get; set; }
        public int[] SelectedDocumentTypes { get; set; }
		public IDictionary<string, string> TrainingLabelDict { get; set; }
		public string[] SelectedTrainingLabels { get; set; } 
		public string[] SelectedUsers { get; set; } 
		public IDictionary<int, string> GlobalAccessDict { get; set; }  =new Dictionary<int, string>(); 
		public int[] SelectedGlobalAccess { get; set; }

		public IList<DataItem> Data = new List<DataItem>();

        public class DataItem
        {
            public DateTime Date { get; set; }
            public UserDetail User { get; set; }
            public string Category { get; set; }
            public DocumentType DocumentType { get; set; }
			public string TrainingLabels { get; set; }
			public string DocumentTitle { get; set; }
            public PointsStatementResult Result { get; set; }
            public int Points { get; set; }
			public bool IsGlobalAccessed { get; set; }
			public bool IsCertificate { get; set; } = false;
			public TimeSpan Duration { get; set; }
		}

        public class UserDetail : IdentityModel<Guid>
        {
            public string FullName { get; set; }
            public string EmployeeNo { get; set; }
            public string GroupTitle { get; set; }
			public string Email { get; set; }
			public string MobileNumber { get; set; }
			public string IDNumber { get; set; }
			public string Gender { get; set; }
			public string Race { get; set; }
		}
    }

    public enum PointsStatementResult
    {
        Passed,
        Failed,
        Yes,
        No,
        Later,
        Viewed,
        [EnumFriendlyName("Not Viewed")]
        NotViewed,
    }
}