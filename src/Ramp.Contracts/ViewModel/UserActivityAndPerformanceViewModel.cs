using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
	public class UserActivityAndPerformanceViewModel
	{
		public UserActivityAndPerformanceViewModel()
		{
			TestResultList = new List<UserTestResult>();
			TestResultGlobalList = new List<UserTestResult>();
		}

		public bool EnableChecklistDocument { get; set; }
		public bool EnableCategoryTree { get; set; }
		public bool EnableGlobalAccessDocuments { get; set; }
		public bool EnableVirtualClassRoom { get; set; }
		public string JitsiServerName { get; set; }
		public UserViewModel UserModel { get; set; }
		public bool EnableRaceCode { get; set; }
		public bool EnableTrainingActivities { get; set; }
		public IList<InteractionModel> TrainingManualInteractions { get; set; } = new List<InteractionModel>();
		public IList<InteractionModel> MemoInteractions { get; set; } = new List<InteractionModel>();
		public IList<PolicyInteractionModel> PolicyInteractions { get; set; } = new List<PolicyInteractionModel>();
		public IList<TestInteractionModel> TestInteractions { get; set; } = new List<TestInteractionModel>();
		public IList<CheckLisInteractionModel> CheckLisInteractions { get; set; } = new List<CheckLisInteractionModel>();
		public IList<InteractionModel> customDocumentInteractions { get; set; } = new List<InteractionModel>();

		#region Global Access
		public IList<InteractionModel> TrainingManualGlobalInteractions { get; set; } = new List<InteractionModel>();
		public IList<InteractionModel> MemoGlobalInteractions { get; set; } = new List<InteractionModel>();
		public IList<PolicyInteractionModel> PolicyGlobalInteractions { get; set; } = new List<PolicyInteractionModel>();
		public IList<TestInteractionModel> TestGlobalInteractions { get; set; } = new List<TestInteractionModel>();
		public IList<CheckLisInteractionModel> CheckListGlobalInteractions { get; set; } = new List<CheckLisInteractionModel>();
		#endregion

		public IEnumerable<TrainingActivityModelShort> TrainingActivities { get; set; } = new List<TrainingActivityModelShort>();
		public IList<PointModel> PointsStatement { get; set; } = new List<PointModel>();
		public IEnumerable<FeedbackModel> Feedback { get; set; } = new List<FeedbackModel>();

		#region Display more than 3 result
		public List<UserTestResult> TestResultList { get; set; }
		public List<UserTestResult> TestResultGlobalList { get; set; }
		public string Title { get; set; }
		#endregion

		public class UserViewModel
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public string ContactNumber { get; set; }
			public string Email { get; set; }
			public string IDNumber { get; set; }
			public string Gender { get; set; }
			public string EmployeeNumber { get; set; }
			public string Race { get; set; }
			public string Group { get; set; }
		}

		public class InteractionModel
		{
			public string Id { get; set; }
			public string DocumentTitle { get; set; }
			public string TrainingLabels { get; set; }
			public bool Viewed { get; set; }
			public DateTime DateAssigned { get; set; }
			public DateTime? DateViewed { get; set; }
			public string TimeTaken { get; set; }
			public bool IsGlobalAccess { get; set; }
		}

		public class PolicyInteractionModel : InteractionModel
		{
			public string Response { get; set; }
			public DateTime? ResponseDate { get; set; }
		}

		public class UserTestResult
		{

			public string Attempt { get; set; }
			public bool Viewed { get; set; }
			public string Result { get; set; }
			public DateTime DateAssigned { get; set; }
			public DateTime DateViewed { get; set; }
			public string FinalResult { get; set; }
			public string TestId { get; set; }
			public string Title { get; set; }
			public bool IsGlobalAccess { get; set; }
		}



		public class ChecklistInteractionModel
		{
			public List<CheckLisInteractionModel> Checklist { get; set; }
			public string DocumentTitle { get; set; }
			public string Id { get; set; }
		}
		public class CustomdocumentInteractionModel
		{
			public List<CustomDocumentInteractionModel> Checklist { get; set; }
			public string DocumentTitle { get; set; }
			public string Id { get; set; }
		}


		public class CustomDocumentInteractionModel
		{
			public string UserName { get; set; }
			public string DocumentTitle { get; set; }
			public string CustomDocumentId { get; set; }
			public bool Viewed { get; set; }
			public DateTime DateAssigned { get; set; }
			public DateTime DateViewed { get; set; }


			public string Status { get; set; }
			public string Access { get; set; }
			public string Group { get; set; }


			public string TestSelectedAnswer { get; set; }
			public string TestQuestion { get; set; }
			public Guid TestQuestionID { get; set; }
			public Guid CustomDocumentID { get; set; }

			//public bool Viewed { get; set; }
			//public string Group { get; set; }
			//public DateTime DateAssigned { get; set; }
			//public DateTime? DateViewed { get; set; }
			public string Completed { get; set; }
			public string ViewSubmition { get; set; }
			public DateTime? DateSubmitted { get; set; }
			public string Id { get; set; }
			public string DocumentType { get; set; }
			public string UserId { get; set; }
			public string AssignedDocId { get; set; }
			//public string Status { get; set; }
			public string StandarduserID { get; set; }
			public string IdNumber { get; set; }

			public string EmployeeCode { get; set; }
			//public string Access { get; set; }
			public bool IsGlobalAccess { get; set; } = false;

			public Object Mesages { get; set; }
		}
		public class CheckLisInteractionModel
		{
			public string UserName { get; set; }
			public string TrainingLabels { get; set; }
			public string DocumentTitle { get; set; }
			public bool Viewed { get; set; }
			public string Group { get; set; }
			public DateTime DateAssigned { get; set; }
			public DateTime? DateViewed { get; set; }
			public string Completed { get; set; }
			public string ViewSubmition { get; set; }
			public DateTime? DateSubmitted { get; set; }
			public string Id { get; set; }
			public string ChecksCompleted { get; set; }
			public string UserId { get; set; }
			public string AssignedDocId { get; set; }
			public string Status { get; set; }
			public string IdNumber { get; set; }
			public string EmployeeCode { get; set; }
			public string Access { get; set; }
			public bool IsGlobalAccess { get; set; } = false;
		}
		public class TestInteractionModel : InteractionModel
		{
			public string Result1 { get; set; }
			public string ResultId1 { get; set; }
			public DateTime? DateViewed1 { get; set; }
			public string TimeTaken1 { get; set; }
			public string Result2 { get; set; }
			public string ResultId2 { get; set; }
			public DateTime? DateViewed2 { get; set; }
			public string TimeTaken2 { get; set; }
			public string Result3 { get; set; }
			public string ResultId3 { get; set; }
			public DateTime? DateViewed3 { get; set; }
			public string TimeTaken3 { get; set; }
			public string Result4 { get; set; }
			public string ResultId4 { get; set; }
			public DateTime? DateViewed4 { get; set; }
			public string TimeTaken4 { get; set; }
			public string Result5 { get; set; }
			public string ResultId5 { get; set; }
			public DateTime? DateViewed5 { get; set; }
			public string TimeTaken5 { get; set; }
			public string Result6 { get; set; }
			public string ResultId6 { get; set; }
			public DateTime? DateViewed6 { get; set; }
			public string TimeTaken6 { get; set; }
		}

		public class TrainingActivityModelShort
		{
			public string Title { get; set; }
			public string Type { get; set; }
			public DateTime? FromDate { get; set; }
			public DateTime? ToDate { get; set; }
			public string Cost { get; set; }
			public int Points { get; set; }
			public string ActivityId { get; set; }
		}

		public class PointModel
		{
			public string Type { get; set; }
			public string Title { get; set; }
			public DateTime Date { get; set; }
			public int Points { get; set; }
			public string IsGlobalAccess { get; set; }
		}

		public class FeedbackModel
		{
			public string DocumentType { get; set; }
			public string DocumentTitle { get; set; }
			public DateTime Date { get; set; }
			public string Type { get; set; }
			public string Comment { get; set; }
		}
	}
}