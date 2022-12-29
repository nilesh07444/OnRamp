using VirtuaCon;

namespace Domain.Models
{
	public class Icon : DomainObject
	{
		public IconType IconType { get; set; }
		public virtual FileUpload Upload { get; set; }
	}
	public static class IconTypeDescriptions
	{
		public const string MyPlaybooks = "My Playbooks",
							MyTests = "My Tests",
							TrophyCabinet = "Trophy Cabinet",
							ViewPlaybook = "View Playbook",
							ViewPublishedTest = "View Published Test",
							ViewDraftTest = "View Draft Test",
							CreateTest = "Create Test",
							Edit = "Edit",
							Delete = "Delete",
							Collaborate = "Collaborate",
							Feedback = "Feedback Read",
							FeedbackUnread = "Feedback Unread",
							Build = "Build",
							Send = "Send",
							Track = "Track",
							Home = "Home",
							PortalSettings = "Portal Settings",
							ManageCategories = "Manage Categories",
							ManagePlaybookLibrary = "Manage Playbook Library",
							ManageTestLibrary = "Manage Test Library",
							GroupManagement = "Group Management",
							UserManagement = "User Management",
							SelfSignupManagement = "Self Sign Up Management",
							Assign_UnAssignPlaybooks = "Assign/Un-Assign Playbooks",
							Assign_UnAssignTests = "Assign/Un-Assign Tests",
							Report_Category_Certification_Utilization = "Category,Certification & Utilization Report",
							Report_Points = "Points Report",
							Report_Test_History = "Test History Report",
							Report_User_Activities = "User Activities Report",
							Report_User_KPI = "User KPI Report",
							Report_User_Login_Stats = "User Login Stats Report",
							Reset_Password = "Reset Password",
							AddUsers = "Add Users",
							Duplicate_Playbook = "Duplicate Playbook",
							Export = "Export",
							ManageBEECertificate = "Manage BEE Certificate",
							ManageExternalTrainingProviders = "Manage External Training Providers",
							ManageTrainingActivities = "Manage Training Activities",
							Report_Training_Activities = "Training Activities Report",
							ManageTrainingLabels = "Manage Training Labels",
							TakeTest = "Take Test",
							DownloadCertificate = "Download Certificate",
							TestExpired = "Test Expired",
							Print = "Print",
							TrainingManualType = "Type: Training Manual",
							MemoType = "Type: Memo1",
							PolicyType = "Type: Policy",
							//Added By GK
							TestType = "Type: Test",
							FormType = "Type: Form",
							AcrobatFieldType = "Type: AcrobatField",
							BuildFromScratch = "Build From Scratch",
							BuildFromTemplate = "Build From Template",
							MyDocuments = "My Documents",
							MyProgress = "My Progress",
							SendAssign = "Send: Assign",
							CheckListType = "Type: CheckList",
							SendUnassign = "Send: Unassign",
							Schedule = "Schedule",
							Today = "Today",
							SendReassign = "Send: Reassign",
							Recurrence = "Recurrence",
						VirtualClassRoom = "Virtual Classroom",
						GlobalDocuments = "Global Documents",
			LearnMore = " Learn More",
			MyVirtualMeetings = "My Virtual Meetings",
			BuildVirtualMeeting = "Build Virtual Meeting"
			;
	}
	public enum IconType
	{
		[EnumFriendlyName(IconTypeDescriptions.MyPlaybooks)]
		MyPlaybooks,
		[EnumFriendlyName(IconTypeDescriptions.MyTests)]
		MyTests,
		[EnumFriendlyName(IconTypeDescriptions.TrophyCabinet)]
		TrophyCabinet,
		[EnumFriendlyName(IconTypeDescriptions.ViewPlaybook)]
		ViewPlaybook,
		[EnumFriendlyName(IconTypeDescriptions.ViewPublishedTest)]
		ViewPublishedTest,
		[EnumFriendlyName(IconTypeDescriptions.ViewDraftTest)]
		ViewDraftTest,
		[EnumFriendlyName(IconTypeDescriptions.CreateTest)]
		CreateTest,
		[EnumFriendlyName(IconTypeDescriptions.Edit)]
		Edit,
		[EnumFriendlyName(IconTypeDescriptions.Delete)]
		Delete,
		[EnumFriendlyName(IconTypeDescriptions.Collaborate)]
		Collaborate,
		[EnumFriendlyName(IconTypeDescriptions.Feedback)]
		Feedback,
		[EnumFriendlyName(IconTypeDescriptions.FeedbackUnread)]
		FeedbackUnread,
		[EnumFriendlyName(IconTypeDescriptions.Build)]
		Build,
		[EnumFriendlyName(IconTypeDescriptions.Send)]
		Send,
		[EnumFriendlyName(IconTypeDescriptions.Track)]
		Track,
		[EnumFriendlyName(IconTypeDescriptions.Home)]
		Home,
		[EnumFriendlyName(IconTypeDescriptions.PortalSettings)]
		PortalSettings,
		[EnumFriendlyName(IconTypeDescriptions.ManageCategories)]
		ManageCategories,
		[EnumFriendlyName(IconTypeDescriptions.ManagePlaybookLibrary)]
		ManagePlaybookLibrary,
		[EnumFriendlyName(IconTypeDescriptions.ManageTestLibrary)]
		ManageTestLibrary,
		[EnumFriendlyName(IconTypeDescriptions.GroupManagement)]
		GroupManagement,
		[EnumFriendlyName(IconTypeDescriptions.UserManagement)]
		UserManagement,
		[EnumFriendlyName(IconTypeDescriptions.SelfSignupManagement)]
		SelfSignupManagement,
		[EnumFriendlyName(IconTypeDescriptions.Assign_UnAssignPlaybooks)]
		Assign_UnAssignPlaybooks,
		[EnumFriendlyName(IconTypeDescriptions.Assign_UnAssignTests)]
		Assign_UnAssignTests,
		[EnumFriendlyName(IconTypeDescriptions.Report_Category_Certification_Utilization)]
		Report_Category_Certification_Utilization,
		[EnumFriendlyName(IconTypeDescriptions.Report_Points)]
		Report_Points,
		[EnumFriendlyName(IconTypeDescriptions.Report_Test_History)]
		Report_Test_History,
		[EnumFriendlyName(IconTypeDescriptions.Report_User_Activities)]
		Report_User_Activities,
		[EnumFriendlyName(IconTypeDescriptions.Report_User_KPI)]
		Report_User_KPI,
		[EnumFriendlyName(IconTypeDescriptions.Report_User_Login_Stats)]
		Report_User_Login_Stats,
		[EnumFriendlyName(IconTypeDescriptions.Reset_Password)]
		ResetPassword,
		[EnumFriendlyName(IconTypeDescriptions.AddUsers)]
		AddUsers,
		[EnumFriendlyName(IconTypeDescriptions.Duplicate_Playbook)]
		DuplicatePlaybook,
		[EnumFriendlyName(IconTypeDescriptions.Export)]
		Export,
		[EnumFriendlyName(IconTypeDescriptions.ManageBEECertificate)]
		ManageBEECertificate,
		[EnumFriendlyName(IconTypeDescriptions.ManageExternalTrainingProviders)]
		ManageExternalTrainingProviders,
		[EnumFriendlyName(IconTypeDescriptions.ManageTrainingActivities)]
		ManageTrainingActivities,
		[EnumFriendlyName(IconTypeDescriptions.Report_Training_Activities)]
		Report_TrainingActivities,
		[EnumFriendlyName(IconTypeDescriptions.ManageTrainingLabels)]
		ManageTrainingLabels,
		[EnumFriendlyName(IconTypeDescriptions.TakeTest)]
		TakeTest,
		[EnumFriendlyName(IconTypeDescriptions.DownloadCertificate)]
		DownloadCertificate,
		[EnumFriendlyName(IconTypeDescriptions.TestExpired)]
		TestExpired,
		[EnumFriendlyName(IconTypeDescriptions.Print)]
		Print,
		[EnumFriendlyName(IconTypeDescriptions.TrainingManualType)]
		TrainingManualType,
		[EnumFriendlyName(IconTypeDescriptions.MemoType)]
		MemoType,
		[EnumFriendlyName(IconTypeDescriptions.PolicyType)]
		PolicyType,
		[EnumFriendlyName(IconTypeDescriptions.FormType)]
		FormType,
		[EnumFriendlyName(IconTypeDescriptions.AcrobatFieldType)]
		AcrobatFieldType,
		[EnumFriendlyName(IconTypeDescriptions.TestType)]
		TestType,
		[EnumFriendlyName(IconTypeDescriptions.BuildFromScratch)]
		BuildFromScratch,
		[EnumFriendlyName(IconTypeDescriptions.BuildFromTemplate)]
		BuildFromTemplate,
		[EnumFriendlyName(IconTypeDescriptions.MyDocuments)]
		MyDocuments,
		[EnumFriendlyName(IconTypeDescriptions.MyProgress)]
		MyProgress,
		[EnumFriendlyName(IconTypeDescriptions.SendAssign)]
		SendAssign,
		[EnumFriendlyName(IconTypeDescriptions.SendUnassign)]
		SendUnassign,
		[EnumFriendlyName(IconTypeDescriptions.CheckListType)]
		CheckListType,
		[EnumFriendlyName(IconTypeDescriptions.Schedule)]
		Schedule,
		[EnumFriendlyName(IconTypeDescriptions.Today)]
		Today,
		[EnumFriendlyName(IconTypeDescriptions.SendReassign)]
		Reassign,
		[EnumFriendlyName(IconTypeDescriptions.Recurrence)]
		Recurrence,
		[EnumFriendlyName(IconTypeDescriptions.VirtualClassRoom)]
		VirtualClassRoom,
		[EnumFriendlyName(IconTypeDescriptions.GlobalDocuments)]
		GlobalDocuments,
		[EnumFriendlyName(IconTypeDescriptions.LearnMore)]
		LearnMore,
		[EnumFriendlyName(IconTypeDescriptions.MyVirtualMeetings)]
		MyVirtualMeetings,
		[EnumFriendlyName(IconTypeDescriptions.BuildVirtualMeeting)]
		BuildVirtualMeeting
	}
}
