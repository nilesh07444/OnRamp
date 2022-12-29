using System.Collections.Generic;
using System.Linq;

namespace Ramp.Contracts.Security {
	public class Role
    {
		public const string CustomerAdmin = "CustomerAdmin",
			StandardUser = "StandardUser",
			CategoryAdmin = "CategoryAdmin",
			ContentAdmin = "ContentAdmin",
			Publisher = "Publisher",
			Reporter = "Reporter",
			UserAdmin = "UserAdmin",
			PortalAdmin = "PortalAdmin",
			Admin = "Admin",
			Reseller = "Reseller",
			NotificationAdmin = "NotificationAdmin",
			TrainingActivityAdmin = "TrainingActivityAdmin",
			ManageTags = "ManageTags",
			ManageVirtualMeetings = "ManageVirtualMeetings",
		TrainingActivityReporter = "TrainingActivityReporter",
			ManageActivityLog = "ManageActivityLog",
			ContentApprover = "ContentApprover",
			ContentCreator = "ContentCreator",
		ManageAutoWorkflow = "ManageAutoWorkflow",
		ManageReportSchedule = "ManageReportSchedule";


		public const string CustomerAdminDesc = "Global Admin",
			StandardUserDesc = "Standard User",
			CategoryAdminDesc = "Manage Document Categories",
			ContentAdminDesc = "Manage Documents",
			PublisherDesc = "Push content to users",
			ReporterDesc = "Reports",
			UserAdminDesc = "Manage Users",
			PortalAdminDesc = "Manage OnRamp Portal Settings",
			AdminDesc = "Admin",
			ResellerDesc = "Reseller",
			NotificationAdminDesc = "Manage Notifications",
			TrainingActivityAdminDesc = "Manage Training Activities",
			ManageTagsDesc = "Manage Tags",
			ManageVirtualMeetingsDesc = "Manage Virtual Meetings",
			TrainingActivityReporterDesc = "Report on Training Activities",
			ManageActivityLogDesc = "Manage ActivityLog",
			ContentApproverDesc = "Content Approver",
			ContentCreatorDesc = "Content Creator",
			ManageAutoWorkflowDesc = "Manage AutoWork flow",
			ManageReportScheduleDesc = "Manage Report Schedule";

		public static readonly Dictionary<string, string> RoleDescriptionDictionary = new Dictionary<string, string>
		{
			{CustomerAdmin, CustomerAdminDesc},
			{StandardUser, StandardUserDesc},
			{CategoryAdmin, CategoryAdminDesc},
			{ContentAdmin, ContentAdminDesc},
			{Publisher, PublisherDesc},
			{Reporter, ReporterDesc},
			{UserAdmin, UserAdminDesc},
			{PortalAdmin, PortalAdminDesc},
			{Admin, AdminDesc},
			{Reseller, ResellerDesc},
			{NotificationAdmin, NotificationAdminDesc},
			{TrainingActivityAdmin,TrainingActivityAdminDesc},
			{TrainingActivityReporter,TrainingActivityReporterDesc },
			{ ManageTags,ManageTagsDesc},
			{ ManageVirtualMeetings,ManageVirtualMeetingsDesc},
			{ ManageActivityLog,ManageActivityLogDesc},
			{ ContentApprover,ContentApproverDesc},
			{ ContentCreator,ContentCreatorDesc},
			{ ManageAutoWorkflow,  ManageAutoWorkflowDesc},
			{ ManageReportSchedule,  ManageReportScheduleDesc},

		};

        public static bool IsInAdminRole(List<string> userRolesList)
        {
			return userRolesList.Any(r => r.Equals(Role.Reporter)
										  || r.Equals(Role.CategoryAdmin)
										  || r.Equals(Role.ContentAdmin)
										  || r.Equals(Role.CustomerAdmin)
										  || r.Equals(Role.NotificationAdmin)
										  || r.Equals(Role.PortalAdmin)
										  || r.Equals(Role.Publisher)
										  || r.Equals(Role.UserAdmin)
										  || r.Equals(Role.TrainingActivityAdmin)
										  || r.Equals(Role.ManageTags)
										  || r.Equals(Role.ManageVirtualMeetings)
										  || r.Equals(Role.TrainingActivityReporter)
										  || r.Equals(Role.ManageActivityLog)
										  || r.Equals(Role.ContentCreator)
										  || r.Equals(Role.ContentApprover)
										  || r.Equals(Role.ManageAutoWorkflow)
										  || r.Equals(Role.ManageReportSchedule)
										  );
		}

        public static bool IsInGlobalAdminRole(List<string> userRolesList)
        {
            return userRolesList.Any(r => r.Equals(Role.Admin));
        }
		public static bool IsInManageReportScheduleRole(List<string> userRolesList)
		{
			return userRolesList.Any(r => r.Equals(Role.ManageReportSchedule));
		}


		public static bool IsInResellerRole(List<string> userRolesList)
        {
            return userRolesList.Any(r => r.Equals(Role.Reseller));
        }
		public static bool IsInStandardUserRole(List<string> userRolesList)
        {
            return userRolesList.Any(r => r.Equals(Role.StandardUser));
        }
    }
}