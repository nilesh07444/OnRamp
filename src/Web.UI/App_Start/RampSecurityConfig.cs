using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.Security;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Domain.Customer;
using Web.UI.Areas.BundleManagement.Controllers;
using Web.UI.Areas.Categories.Controllers;
using Web.UI.Areas.Configurations.Controllers;
using Web.UI.Areas.CustomerManagement.Controllers;
using Web.UI.Areas.Feedback.Controllers;
using Web.UI.Areas.ManageTrainingGuides.Controllers;
using Web.UI.Areas.ManageTrainingTest.Controllers;
using Web.UI.Areas.ProvisionalManagement.Controllers;
using Web.UI.Areas.Reporting.Controllers;
using Web.UI.Controllers;
using System.Net.Http;

namespace Web.UI.App_Start {
	public static class RampSecurityConfig {
		public static void InitializationSecurityDictionary() {
			SecureControllerAccess();
		}

		private static void SecureControllerAccess<T>(Expression<Func<T, ActionResult>> actionSelector,
			params string[] roles) {
			string action = null;
			var controller = typeof(T).Name;
			var area = RampSecurity.findAreaForControllerType(typeof(T));

			if (actionSelector != null) {
				var exp = actionSelector as LambdaExpression;
				if (exp != null)
					if (exp.Parameters.Count > 0)
						if (exp.Body is MethodCallExpression)
							action = ((MethodCallExpression)exp.Body).Method.Name;
			}
			RampSecurity.SetRampSecurity(RampSecurity.MakeKey(area, controller, action), roles);
		}

		public static void SecureCustomerMgmtController() {
			SecureControllerAccess<CustomerMgmtController>(
			   a => a.SaveCSVCustomerCompanyUser(null),
			   Role.Admin,
			   Role.CustomerAdmin,
			   Role.Reseller,
			   Role.UserAdmin,
			   Role.ManageAutoWorkflow, Role.ManageReportSchedule
			   );

			SecureControllerAccess<CustomerMgmtController>(
				a => a.ManageOwnCustomerCompanyUser(Guid.Empty, Guid.Empty, null, null),
				Role.Reseller,
				Role.CustomerAdmin,
				Role.Admin,
				Role.UserAdmin,
			   Role.ManageAutoWorkflow, Role.ManageReportSchedule
				);
			SecureControllerAccess<CustomerMgmtController>(
				a => a.CustomerAdminSetting(),
				Role.Admin,
				Role.CustomerAdmin,
				Role.Reseller,
				Role.PortalAdmin,
			   Role.ManageAutoWorkflow, Role.ManageReportSchedule
				);
			SecureControllerAccess<CustomerMgmtController>(
			   a => a.CustomerUserSelfSignUpNotApproved(),
			   Role.Admin,
			   Role.Reseller,
			   Role.CustomerAdmin,
			   Role.UserAdmin,
			   Role.ManageAutoWorkflow, Role.ManageReportSchedule
			   );
			SecureControllerAccess<CustomerMgmtController>(
				a => a.UsersForCompany(null),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin);
			SecureControllerAccess<CustomerMgmtController>(
				a => a.ViewCustomerSurvey(),
				Role.Admin,
			   Role.ManageAutoWorkflow,Role.ManageReportSchedule);
		}

		public static void SecureCategoriesController() {
			SecureControllerAccess<CategoriesController>(
			   a => a.Index(null),
			   Role.CategoryAdmin,
			   Role.CustomerAdmin,
			   Role.Admin,
			   Role.Reseller
			   );
			SecureControllerAccess<CategoriesController>(
				a => a.ViewCategoryStatistics(null),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.Reporter
				);
		}

		public static void SecureMeetingController() {
			SecureControllerAccess<VirtualMeetingController>(
			   a => a.Index(),
			   Role.CategoryAdmin,
			   Role.CustomerAdmin,
			   Role.Admin,
			   Role.Reseller,
			   Role.ManageVirtualMeetings
			   );
			
		}

		public static void SecureCategoryController() {
			SecureControllerAccess<CategoryController>(
				a => a.ManageCategoryModalPartial(),
				Role.Admin,
				Role.CustomerAdmin,
				Role.ContentAdmin);
		}

		public static void SecureCertificateController() {
			SecureControllerAccess<AchievementController>(
				a => a.Index(null),
				Role.Admin,
				Role.CustomerAdmin,
				Role.PortalAdmin);
		}

		public static void SecureDocumentController() {
			SecureControllerAccess<DocumentController>(
				a => a.Index(null),
				Role.Admin,
				Role.CustomerAdmin,
				Role.ContentAdmin,
				Role.ContentCreator,
				Role.ContentApprover,
				Role.StandardUser
				);
			SecureControllerAccess<DocumentController>(
				a => a.CollaboratorsForDocument(null, DocumentType.Unknown),
				Role.Admin,
				Role.CustomerAdmin,
				Role.ContentAdmin,
				Role.ContentCreator,
				Role.ContentApprover,
				Role.StandardUser
				);
			SecureControllerAccess<DocumentController>(
				a => a.MyDocuments(),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.ContentAdmin,
				Role.StandardUser,
				Role.ContentCreator,
				Role.ContentApprover,
				Role.StandardUser
				);
		}

		private static void SecureSendController() {
			SecureControllerAccess<SendController>(
				a => a.Index(),
				Role.Publisher,
				Role.CustomerAdmin);
		}

		public static void SecureManageTrainingGuidesController() {
			SecureControllerAccess<ManageTrainingGuidesController>(
			   a => a.Index(),
			   Role.Admin,
			   Role.CustomerAdmin,
			   Role.ContentAdmin,
			   Role.Reseller
			   );
			SecureControllerAccess<ManageTrainingGuidesController>(
			  a => a.AssignTrainingGuideToUsersOrGroups(),
			  Role.Admin,
			  Role.Reseller,
			  Role.CustomerAdmin,
			  Role.Publisher
			  );
			SecureControllerAccess<ManageTrainingGuidesController>(
			a => a.Create(),
			Role.Admin,
			Role.Reseller,
			Role.CustomerAdmin,
			Role.ContentAdmin
			);
			SecureControllerAccess<ManageTrainingGuidesController>(
			 a => a.EditTrainingGuide(Guid.Empty),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.ContentAdmin
			 );
			SecureControllerAccess<ManageTrainingGuidesController>(
			 a => a.PreviewByReferenceId(null),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.ContentAdmin,
			 Role.StandardUser
			 );
			SecureControllerAccess<ManageTrainingGuidesController>(
				a => a.Delete(Guid.Empty),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.ContentAdmin
				);
			SecureControllerAccess<ManageTrainingGuidesController>(
				a => a.AddUsersToCollaborationList(null),
				Role.ContentAdmin,
				Role.CustomerAdmin);
			SecureControllerAccess<ManageTrainingGuidesController>(
				a => a.Assign_UnAssignPlaybooks(0),
				Role.Publisher,
				Role.CustomerAdmin);
		}

		public static void SecureManageTrainingTestController() {
			SecureControllerAccess<ManageTrainingTestController>(
			   a => a.Index(),
			   Role.Admin,
			   Role.ContentAdmin,
			   Role.CustomerAdmin,
			   Role.Reseller
			   );
			SecureControllerAccess<ManageTrainingTestController>(
			   a => a.AssignTestToUserOrGroup(),
			   Role.Admin,
			   Role.Reseller,
			   Role.CustomerAdmin,
			   Role.Publisher
			   );
			SecureControllerAccess<ManageTrainingTestController>(
				a => a.TestNotApperUsers(),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.Reporter
				);
			SecureControllerAccess<ManageTrainingTestController>(
			  a => a.TestHistoryReport(null),
			  Role.Admin,
			  Role.Reseller,
			  Role.CustomerAdmin,
			  Role.Reporter
			  );
			SecureControllerAccess<ManageTrainingTestController>(
			 a => a.Create(Guid.Empty),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.ContentAdmin
			 );
			SecureControllerAccess<ManageTrainingTestController>(
			 a => a.EditTrainingTest(Guid.Empty),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.ContentAdmin
			 );
			SecureControllerAccess<ManageTrainingTestController>(
			 a => a.TakeTrainingTest(Guid.Empty),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.ContentAdmin,
			 Role.StandardUser
			 );
			SecureControllerAccess<ManageTrainingTestController>(
				a => a.MyTests(Guid.Empty),
				Role.StandardUser);
			SecureControllerAccess<ManageTrainingTestController>(
				a => a.Assign_UnAssignTests(),
				Role.Publisher,
				Role.CustomerAdmin
				);
			SecureControllerAccess<ManageTrainingTestController>(
			 a => a.FocusAreaReport(),
			 Role.Admin,
			 Role.CustomerAdmin,
			 Role.Reseller,
			 Role.Reporter
			);
		}

		public static void SecureHomeController() {
			SecureControllerAccess<HomeController>(
			 a => a.AdminReports(),
			 Role.Admin,
			 Role.CustomerAdmin,
			 Role.Reseller,
			 Role.Reporter,
			 Role.TrainingActivityReporter
			 );
			SecureControllerAccess<HomeController>(
				a => a.CanvasUpload(),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.ContentAdmin
				);
			SecureControllerAccess<HomeController>(
				a => a.UploadTrophy(),
				Role.Admin);
			SecureControllerAccess<HomeController>(
				a => a.UserManagement(),
				Role.Admin,
				Role.CustomerAdmin,
				Role.UserAdmin);
			SecureControllerAccess<HomeController>(
				a => a.LogTraining(),
				Role.Admin,
				Role.CustomerAdmin,
				Role.TrainingActivityAdmin,
				Role.ContentAdmin);
		}


		public static void SecureCheckListController() {
			SecureControllerAccess<ActivitybookController>(
			 a => a.Edit(null),
		   Role.Admin,
			   Role.CustomerAdmin,
			   Role.ContentAdmin,
			   Role.Reseller,
				Role.ContentCreator,
				Role.ContentApprover
			 );
		}
		public static void SecureMemoController() {
			SecureControllerAccess<MemoController>(
			 a => a.Edit(null),
			   Role.Admin,
			   Role.CustomerAdmin,
			   Role.ContentAdmin,
			   Role.Reseller,
			   Role.ContentCreator,
				Role.ContentApprover
			 );
		}
		public static void SecurePolicyController() {
			SecureControllerAccess<PolicyController>(
			 a => a.Edit(null),
		   Role.Admin,
			   Role.CustomerAdmin,
			   Role.ContentAdmin,
			   Role.Reseller,
			   Role.ContentCreator,
				Role.ContentApprover
			 );
		}
		public static void SecureTrainingManualController() {
			SecureControllerAccess<TrainingManualController>(
			 a => a.Edit(null),
			   Role.Admin,
			   Role.CustomerAdmin,
			   Role.ContentAdmin,
			   Role.Reseller,
			   Role.ContentCreator,
				Role.ContentApprover
			 );
		}
		public static void SecureTestController() {
			SecureControllerAccess<TestController>(
			 a => a.Edit(null),
			   Role.Admin,
			   Role.CustomerAdmin,
			   Role.ContentAdmin,
			   Role.Reseller,
			   Role.ContentCreator,
				Role.ContentApprover
			 );
		}

		public static void SecureMagageGroupsController() {
			SecureControllerAccess<ManageGroupsController>(
				a => a.Index(null, null),
			   Role.Admin,
			   Role.CustomerAdmin,
			   Role.ContentAdmin,
			   Role.Reseller
				);
		}

		public static void SecureManageUserController() {
			SecureControllerAccess<ManagerUserController>(
			a => a.KpiReport(null),
			Role.Admin,
			Role.Reseller,
			Role.CustomerAdmin,
			Role.Reporter
			);
			SecureControllerAccess<ManagerUserController>(
			 a => a.UserActivityDetails(),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.Reporter
			 );
			SecureControllerAccess<ManagerUserController>(
			 a => a.UserCorrespondenceDetails(),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.Reporter
			 );
			SecureControllerAccess<ManagerUserController>(
				a => a.Index(),
				Role.Admin);
		}

		public static void SecureProvisionalMgmtController() {
			SecureControllerAccess<ProvisionalMgmtController>(
			 a => a.GetUserFrequency(),
			 Role.Admin,
			 Role.Reseller,
			 Role.CustomerAdmin,
			 Role.Reporter
			 );
			SecureControllerAccess<ProvisionalMgmtController>(
			   a => a.Index(null),
			   Role.Admin);
			SecureControllerAccess<ProvisionalMgmtController>(
				a => a.ProvisionalCompanyUser(Guid.Empty, Guid.Empty, null),
				Role.Admin);
			SecureControllerAccess<ProvisionalMgmtController>(
				a => a.ReassignCompanyUser(),
				Role.Admin);
			SecureControllerAccess<ProvisionalMgmtController>(
			   a => a.CustomerExpiryDateReport(),
			   Role.Admin);
			SecureControllerAccess<ProvisionalMgmtController>(
				a => a.DeleteProvisionalCompany(Guid.Empty),
				Role.Admin);
		}

		public static void SecureBundleController() {
			SecureControllerAccess<BundleController>(
			 a => a.Index(null),
			 Role.Admin);
		}

		public static void SecureSettingsController() {
			SecureControllerAccess<SettingsController>(
			   a => a.Create(),
			   Role.Admin, Role.ManageAutoWorkflow,Role.ManageReportSchedule);
		}
		public static void SecureTrainingLabelController() {
			SecureControllerAccess<TagController>(
				a => a.Index(),
				Role.ContentAdmin,
				Role.CustomerAdmin,
				Role.TrainingActivityAdmin,
				Role.ManageTags
				);
			//SecureControllerAccess<TrainingLabelController>(
			//	a => a.Save(),
			//	Role.ContentAdmin,
			//	Role.CustomerAdmin,
			//	Role.TrainingActivityAdmin
			//	);
		}
		public static void SecureTrainingActivityController() {
			SecureControllerAccess<TrainingActivityController>(
				a => a.Index(null),
				Role.CustomerAdmin,
				Role.ManageActivityLog,
				Role.TrainingActivityAdmin
				);
		}
		public static void SecureExternalTrainingProviderController() {
			SecureControllerAccess<ExternalTrainingProviderController>(
				a => a.Index(null),
				Role.CustomerAdmin,
				Role.ManageActivityLog,
				Role.TrainingActivityAdmin
				);
		}
		public static void SecureTrainingActivityReportController() {
			SecureControllerAccess<TrainingActivityReportController>(
				a => a.Index(null),
				Role.CustomerAdmin,
				Role.TrainingActivityReporter
				);
		}
		public static void SecureExternalTrainingProviderReportController() {
			SecureControllerAccess<ExternalTrainingProviderReportController>(
				a => a.Index(null),
				Role.CustomerAdmin,
				Role.TrainingActivityReporter
				);
		}
		public static void SecureFeedbackController() {
			SecureControllerAccess<FeedbackController>(
				a => a.ContentFeedbackReport(),
				Role.Admin,
				Role.CustomerAdmin,
				Role.Reseller,
				Role.Reporter
			);
			SecureControllerAccess<FeedbackController>(
				a => a.ContentFeedbackData(null, null, null, null, null, null),
				Role.Admin,
				Role.CustomerAdmin,
				Role.Reseller,
				Role.Reporter
			);
		}
		public static void SecureReportingControllers() {
			SecureControllerAccess<IndividualDevelopmentRecordReportController>(
			  a => a.Index(null),
			  Role.CustomerAdmin,
			  Role.TrainingActivityReporter
			  );
			SecureControllerAccess<PlaybookUtilizationReportController>(
			  a => a.Index(null),
			  Role.CustomerAdmin,
			  Role.Reporter
			  );
			SecureControllerAccess<ActivitybookSubmissionReportController>(
			  a => a.Index(null),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.Reporter
			  );
			SecureControllerAccess<PointsStatementController>(
				a => a.Index(null),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.Reporter
			);
			SecureControllerAccess<UserActivityAndPerformanceReportController>(
				a => a.Index(null),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.Reporter);
			SecureControllerAccess<InteractionReportController>(
				a => a.Index(),
				Role.Admin,
				Role.Reseller,
				Role.CustomerAdmin,
				Role.Reporter);
		}

		public static void SecureAdminController() {
			SecureControllerAccess<AdminController>(
				a => a.ViewDocuments(),
				Role.Admin,
				Role.Reseller);
			SecureControllerAccess<AdminController>(
				a => a.CopyDocuments(),
				Role.Admin);
			SecureControllerAccess<Sprint16MigrationController>(
				a => a.Index(),
				Role.Admin);
		}

		public static void SecureControllerAccess() {
			SecureCategoriesController();
			SecureCategoryController();
			SecureCertificateController();
			SecureDocumentController();
			SecureSendController();
			SecureCustomerMgmtController();
			SecureHomeController();
			SecureMagageGroupsController();
			SecureManageTrainingGuidesController();
			SecureManageTrainingTestController();
			SecureManageUserController();
			SecureBundleController();
			SecureProvisionalMgmtController();
			SecureSettingsController();
			SecureTrainingLabelController();
			SecureTrainingActivityController();
			SecureExternalTrainingProviderController();
			SecureExternalTrainingProviderReportController();
			SecureTrainingActivityReportController();
			SecureReportingControllers();
			SecureFeedbackController();
			SecureAdminController();
			SecureCheckListController();
			SecureMemoController();
			SecurePolicyController();
			SecureTrainingManualController();
			SecureTestController();

		}
	}
}