using Data.EF;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Customer.Models.VirtualClassRooms;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.CorrespondenceManagement;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.CommandParameter.Login;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using Ramp.Contracts.CommandParameter.Settings;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.PackageManagement;
using Ramp.Contracts.QueryParameter.Progress;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.QueryParameter.TrophyCabinet;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Web.UI.Code.Extensions;
using Role = Ramp.Contracts.Security.Role;


namespace Web.UI.Controllers {
	public class HomeController : RampController {
		private ITransientRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly ITransientRepository<Memo> _memoRepository;
		private readonly ITransientRepository<Policy> _policyRepository;
		private readonly ITransientRepository<Test> _testRepository;
		private readonly ITransientRepository<TrainingManual> _trainingManualRepository;
		private readonly ITransientRepository<CheckList> _checkListRepository;
		private readonly ITransientRepository<VirtualClassRoom> _virtualClassRoomRepository;
		public HomeController(ITransientRepository<AssignedDocument> assignedDocumentRepository,
			ITransientRepository<Memo> memoRepository,
		ITransientRepository<Policy> policyRepository,
	ITransientRepository<Test> testRepository,
		ITransientRepository<TrainingManual> trainingManualRepository,
		ITransientRepository<CheckList> checkListRepository,
		ITransientRepository<VirtualClassRoom> virtualClassRoomRepository
			) {
			_assignedDocumentRepository = assignedDocumentRepository;
			_memoRepository = memoRepository;
			_policyRepository = policyRepository;
			_testRepository = testRepository;
			_trainingManualRepository = trainingManualRepository;
			_checkListRepository = checkListRepository;
			_virtualClassRoomRepository = virtualClassRoomRepository;
		}
		#region Index
		
		//[AdminAuthFilter]
		public ActionResult Index() {

			var roles = SessionManager.GetRolesForCurrentlyLoggedInUser().ToList();
			if (Role.IsInGlobalAdminRole(roles) || Role.IsInResellerRole(roles)) {
				var packageQueryParameter = new PackageQueryParameter();
				PackageViewModel result = ExecuteQuery<PackageQueryParameter, PackageViewModel>(packageQueryParameter);

				return View();
			} else {
				return RedirectToAction("Login", "Account");
			}
		}

		#endregion Index

		#region UnAuthorisedAccess

		public ActionResult UnAuthorisedAccess() {
			if (Session["UNAUTHORISED_ACCESS"] != null && (bool)Session["UNAUTHORISED_ACCESS"]) {
				Session["UNAUTHORISED_ACCESS"] = false;
				return View();
			}
			var roles = SessionManager.GetRolesForCurrentlyLoggedInUser().ToList();
			if (!Role.IsInStandardUserRole(roles))
				return RedirectToAction("Index", "Home");
			return RedirectToAction("StandardUser", "Home");
		}

		#endregion UnAuthorisedAccess

		#region AdminUser

		public ActionResult Administrator() {
			if (PortalContext.Current != null && !string.IsNullOrWhiteSpace(PortalContext.Current.LogoFileName)) {
				string path = null;
				if (Role.IsInResellerRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList()))
					path = Server.MapPath("~/LogoImages/ProvisionalLogo/" + PortalContext.Current.LogoFileName);
				else
					path = Server.MapPath("~/LogoImages/CustomerLogo/" + PortalContext.Current.LogoFileName);
				if (!System.IO.File.Exists(path))
					PortalContext.Current.LogoFileName = null;
			}
			if (TempData["IsApproved"] != null)
				ViewData["Message"] = TempData["IsApproved"].ToString();
			else
				ViewData["Message"] = null;
			Session["ON_DASHBOARD"] = true;
			return View();
		}

		#endregion AdminUser


		#region StandardUser

		public ActionResult StandardUser() {
			if (PortalContext.Current != null && !string.IsNullOrWhiteSpace(PortalContext.Current.LogoFileName)) {
				string path = null;
				if (Role.IsInResellerRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList()))
					path = Server.MapPath("~/LogoImages/ProvisionalLogo/" + PortalContext.Current.LogoFileName);
				else
					path = Server.MapPath("~/LogoImages/CustomerLogo/" + PortalContext.Current.LogoFileName);
				if (!System.IO.File.Exists(path))
					PortalContext.Current.LogoFileName = null;
			}
			Session["ON_DASHBOARD"] = true;

			try {

				//var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>(new DocumentsAssignedToUserQuery() {
				//	UserId = Thread.CurrentPrincipal.GetId().ToString(),
				//	CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
				//}).ToList();
				//ViewBag.PendingCount = model.Where(c => c.DocumentType != DocumentType.VirtualClassRoom && c.Status == AssignedDocumentStatus.Pending).ToList().Count;

				var meeting = ExecuteQuery<VirtualClassroomQuery, IEnumerable<VirtualClassModel>>(new VirtualClassroomQuery() {
					UserId = Thread.CurrentPrincipal.GetId().ToString(),
					CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
				}).ToList();

				ViewBag.MeetingCount = meeting.Where(c => c.StartDateTime <= DateTime.Now && c.EndDateTime >= DateTime.Now).ToList().Count;
				//var notificationList = GetUserNotifications();
				//foreach (var item in notificationList) {
				//	item.DocumentHref = "/" + item.DocumentType + "/Preview/" + item.Id;
				//	if(DocumentType.Checklist == item.DocumentType) {
				//		item.ChecklistHref = "/" + item.DocumentType + "/WorkbookComplete/" + item.Id;
				//	}
				//}
				Session["notifications"] = null;
			}
			catch (Exception) {

				throw;
			}

			return View();
		}
		#region Get Notification List
		public List<AssignedDocumentListModel> GetUserNotifications() {
			var loggedInUser = SessionManager.GetCurrentlyLoggedInUserId();
			var userId = loggedInUser.ToString();
			var userNotificationDocs = ExecuteQuery<DocumentNotification, IEnumerable<DocumentNotificationViewModel>>(new DocumentNotification {
				UserId = userId
			}).ToList();

			var notifications = userNotificationDocs.Where(x => !x.IsViewed);

			var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>(new DocumentsAssignedToUserQuery() {
				UserId = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).Where(c => c.DocumentType != DocumentType.VirtualClassRoom && c.Status == AssignedDocumentStatus.Pending && c.LastViewedDate == null).ToList();


			var vmList = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>(new DocumentsAssignedToUserQuery() {
				UserId = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).Where(c => c.DocumentType == DocumentType.VirtualClassRoom && c.Status == AssignedDocumentStatus.Pending).ToList();

			var unassignedDocs = _assignedDocumentRepository.List.Where(x => x.UserId == userId && x.Deleted).Select(x => new AssignedDocumentListModel {
				AssignedDate = x.AssignedDate,
				DocumentType = x.DocumentType,
				Id = x.Id,
				Message = x.AdditionalMsg,
				AssignedDocumentId = x.DocumentId
			});

			var docList = new List<AssignedDocumentListModel>();
			foreach (var doc in notifications) {
				foreach (var item in model) {
					if (doc.DocId == item.Id) {
						item.IsViewed = doc.IsViewed;
						item.DateAssigned = doc.AssignedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
						item.NotificationType = doc.NotificationType;
						item.Message = doc.Message;
						docList.Add(item);
					}
				}

				foreach (var item in vmList) {
					if (doc.DocId == item.Id) {
						var vmModel = _virtualClassRoomRepository.List.Where(x => x.Id == Guid.Parse(item.Id)).FirstOrDefault();
						if (vmModel != null)
						{
							item.Title = vmModel.VirtualClassRoomName;
							item.IsViewed = doc.IsViewed;
							item.DateAssigned = doc.AssignedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
							item.NotificationType = doc.NotificationType;
							item.Message = doc.Message;
							item.VirtualMeetingStartDate = vmModel.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
							item.VirtualMeetingEndDate = vmModel.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
							var todayDate = DateTime.Now;
							if (todayDate.Date >= Convert.ToDateTime(vmModel.StartDate).Date && todayDate.Date <= Convert.ToDateTime(vmModel.EndDate).Date)
							{
								item.IsJoinMeeting = true;
							}
							else
							{
								item.IsJoinMeeting = false;
							}
							item.Id = doc.DocId;
							docList.Add(item);
						}
					}
				}

				foreach (var item in unassignedDocs) {
					if (doc.DocId == item.AssignedDocumentId) {
						var vmStatus = vmList.Where(x => x.Id == item.AssignedDocumentId).Any();
						var modelStatus = model.Where(x => x.Id == item.AssignedDocumentId).Any();
						if (!(vmStatus || modelStatus)) {
							switch (item.DocumentType) {
								case DocumentType.TrainingManual:
									item.Title = _trainingManualRepository.Find(doc.DocId).Title;
									break;
								case DocumentType.Test:
									item.Title = _testRepository.Find(doc.DocId).Title;
									break;
								case DocumentType.Policy:
									item.Title = _policyRepository.Find(doc.DocId).Title;
									break;
								case DocumentType.Memo:
									item.Title = _memoRepository.Find(doc.DocId).Title;
									break;
								case DocumentType.Checklist:
									var xx = _checkListRepository.Find(doc.DocId);
									item.Title = xx.Title;
									break;
								case DocumentType.VirtualClassRoom:
									var virtualClassModel = _virtualClassRoomRepository.Find(Guid.Parse(item.AssignedDocumentId));
									item.Title = virtualClassModel.VirtualClassRoomName;
									break;
							}
							item.IsViewed = doc.IsViewed;
							item.DateAssigned = doc.AssignedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
							item.NotificationType = doc.NotificationType;
							item.Message = doc.Message;
							item.Id = item.AssignedDocumentId;
							docList.Add(item);
						}

					}
				}
			}
			if (docList != null && docList.Count > 0) {
				var list = docList.OrderByDescending(x => x.DateAssigned).ToList();
				return list;
			}
			return docList;
		}
		#endregion

		[HttpGet]
		public JsonResult GetNotifications() {
			try {
				var docList = GetUserNotifications();
				Session["notifications"] = docList;
				return Json(docList, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex) {
				throw ex;
			}			
		}

		[HttpPost]
		public void UpdateNotificationStatus(List<string> docIds) {
			try {
				var userNotificationDocs = ExecuteQuery<DocumentNotification, IEnumerable<DocumentNotificationViewModel>>(new DocumentNotification {
					UserId = SessionManager.GetCurrentlyLoggedInUserId().ToString()
				}).ToList();
				var docIdList = docIds.Distinct().ToList();
				
				var documentNotifications = new List<DocumentNotificationViewModel>();
				foreach (var item in userNotificationDocs) {
					foreach (var Id in docIdList) {
						if(item.DocId == Id) {
							item.IsViewed = true;
							documentNotifications.Add(item);
						}
					}
				}
				
				if(documentNotifications != null && documentNotifications.Count > 0) {
					ExecuteCommand(documentNotifications);
				}
				Session["notifications"] = GetUserNotifications();

			} catch(Exception ex) {
				throw ex;
			}
		}

		#endregion StandardUser

		#region AdminReports

		public ActionResult AdminReports() {
			Session["ON_DASHBOARD"] = true;
			return View();
		}

		#endregion AdminReports
		public ActionResult Build() {
			return View();
		}
		public ActionResult Send() {
			return View();
		}
		public ActionResult Track() {
			return View();
		}
		public ActionResult UserManagement() {
			return View();
		}
		public ActionResult LogTraining() {
			return View();
		}
		[HttpGet]
		public JsonResult OverallProgress() {
			return Json(ExecuteQuery<GetStandardUserProgressQuery, StandardUserProgressViewModel>(new GetStandardUserProgressQuery { UserId = SessionManager.GetCurrentlyLoggedInUserId() }), JsonRequestBehavior.AllowGet);
		}
		#region LoadDonutChartForOverallProgress

		//Bind donut chart for 'Overall Progress'
		[HttpPost]
		public JsonResult LoadDonutChartForOverallProgress() {
			try {
				if (Role.IsInStandardUserRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList())) {
					//For 'Play Book' total count
					int totalTrainingGuideCount = 0;
					var getAllTrainingGuideUsageReportParameter = new GetAllTrainingGuideUsageReportParameter {
						CompanyId = PortalContext.Current.UserCompany.Id,
						IsCustomerLayerDashboard = true,
						LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
					};
					TrainingGuideusageStatsViewModel trainingGuideUsageReport =
						ExecuteQuery<GetAllTrainingGuideUsageReportParameter, TrainingGuideusageStatsViewModel>(
							getAllTrainingGuideUsageReportParameter);

					double totalViewTrainingGuidCnt = trainingGuideUsageReport.TrainingGuidUsageList.Count;
					double totalAssignedTrainingGuidCount = trainingGuideUsageReport.TotalAssignedCount;
					double outstandingTrainingGuideCount = totalAssignedTrainingGuidCount -
														   Convert.ToInt32(totalViewTrainingGuidCnt);
					double totalOutstanding = (outstandingTrainingGuideCount / totalAssignedTrainingGuidCount) * 100;

					totalTrainingGuideCount = totalTrainingGuideCount + trainingGuideUsageReport.TrainingGuidUsageList.Count;
					var allAssignedTestsForAUserQueryParameter =
						new AllAssignedTestsForAUserQueryParameter {
							UserId = SessionManager.GetCurrentlyLoggedInUserId()
						};

					List<TrainingTestViewModel> allTestsAssignedToUser =
					   ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
						   allAssignedTestsForAUserQueryParameter);

					List<TrainingTestViewModel> testCompletedByUser =
						ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
							allAssignedTestsForAUserQueryParameter)
					.Where(em => !em.IsUserEligibleToTakeTest && em.TestStatus).ToList();

					List<TrainingTestViewModel> testAssigned =
						ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
							allAssignedTestsForAUserQueryParameter)
							.ToList();

					List<TrainingTestViewModel> modelTrainingTest =
						ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
							allAssignedTestsForAUserQueryParameter)
							.Where(em => em.IsUserEligibleToTakeTest).ToList();

					var demoList = new List<ReportData>();
					if (totalTrainingGuideCount > 0 || testCompletedByUser.Count > 0) {
						var d1 = new ReportData {
							label = "Play Book",
							value = Convert.ToInt32(totalTrainingGuideCount)
						};
						var d = new ReportData {
							label = "Tests Passed",
							value = testCompletedByUser.Count
						};
						demoList.Add(d1);
						demoList.Add(d);
					}

					double persResult = 100;
					double persResult1 = 100;
					double persResult2 = 100;

					double totalTrainingTestCnt = testAssigned.Count;
					double trainingTestCompletedCnt = 0;
					foreach (TrainingTestViewModel trainingTestViewModel in testCompletedByUser) {
						if (!trainingTestViewModel.IsUserEligibleToTakeTest) {
							trainingTestCompletedCnt = trainingTestCompletedCnt + 1;
						}
					}

					double totalAssignedCount = trainingGuideUsageReport.TotalAssignedCount;
					if (totalAssignedCount > 0) {
						persResult1 = (totalViewTrainingGuidCnt / totalAssignedCount) * 100;
					} else {
						persResult1 = 0;
					}
					if (totalTrainingTestCnt > 0) {
						persResult2 = (trainingTestCompletedCnt / totalTrainingTestCnt) * 100;
						persResult = (persResult1 + persResult2) / 2;
					} else {
						persResult2 = 100;
						persResult = persResult1;
					}
					////persResult = (persResult1 + persResult2) / 2;

					if (totalTrainingGuideCount == 0 && modelTrainingTest.Count == 0) {
						return Json(new { ReportList = demoList, PersResult = Math.Round(0.0, 2) },
						JsonRequestBehavior.AllowGet);
					}

					return Json(new { ReportList = demoList, PersResult = Math.Round(persResult, 2) },
						JsonRequestBehavior.AllowGet);
				} else {
					var demoList = new List<ReportData>
					{
						new ReportData
						{
							label = "My overall progress",
							value = 100
						}
					};
					int persResult = 100;
					return Json(new { ReportList = demoList, PersResult = persResult }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception) {
				return Json(null, JsonRequestBehavior.AllowGet);
			}
			// return null;
		}

		#endregion LoadDonutChartForOverallProgress

		#region LoadDonutChartForPlayBookOutstanding

		//Currently not in use
		//Bind donut chart for 'Play Book Outstanding %'
		public JsonResult LoadDonutChartForPlayBookOutstanding() {
			try {
				if (Role.IsInStandardUserRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList())) {
					var getAllTrainingGuideUsageReportParameter = new GetAllTrainingGuideUsageReportParameter {
						CompanyId = PortalContext.Current.UserCompany.Id,
						IsCustomerLayerDashboard = true,
						LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
					};
					TrainingGuideusageStatsViewModel trainingGuideUsageReport =
						ExecuteQuery<GetAllTrainingGuideUsageReportParameter, TrainingGuideusageStatsViewModel>(
							getAllTrainingGuideUsageReportParameter);

					var reportDataList = new List<ReportData>();
					if (trainingGuideUsageReport.TrainingGuidUsageList.Count > 0) {
						foreach (
							TrainingGuideusageStatsViewModelShort lst in trainingGuideUsageReport.TrainingGuidUsageList) {
							var rd = new ReportData();
							rd.label = lst.TraigingGuideName;
							rd.value = lst.TotalView;
							reportDataList.Add(rd);
						}
					}

					double totalTrainingTestCnt = trainingGuideUsageReport.TrainingGuidUsageList.Count;
					double trainingTestCompletedCnt = 0;
					double persResult = 0;
					foreach (
						TrainingGuideusageStatsViewModelShort trainingTestViewModel in
							trainingGuideUsageReport.TrainingGuidUsageList) {
						if (trainingTestViewModel.TotalView > 0) {
							trainingTestCompletedCnt = trainingTestCompletedCnt + 1;
						}
					}
					if (trainingTestCompletedCnt > 0) {
						persResult = (trainingTestCompletedCnt / totalTrainingTestCnt) * 100;
					} else {
						persResult = (totalTrainingTestCnt) * 100;
					}
					return Json(new { ReportList = reportDataList, PersResult = persResult }, JsonRequestBehavior.AllowGet);
				} else {
					var reportDataList = new List<ReportData>
					{
						new ReportData
						{
							label = "Play Book",
							value = 100
						}
					};
					double persResult = 100;
					return Json(new { ReportList = reportDataList, PersResult = persResult }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception) {
				return Json(null, JsonRequestBehavior.AllowGet);
			}
		}

		#endregion LoadDonutChartForPlayBookOutstanding

		#region LoadDonutChartForPlayBookOutstandingNew

		//Bind donut chart for 'Play Book Outstanding %'
		public JsonResult LoadDonutChartForPlayBookOutstandingNew() {
			try {
				if (Role.IsInStandardUserRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList())) {
					var getAllTrainingGuideUsageReportParameter = new GetAllTrainingGuideUsageReportParameter {
						CompanyId = PortalContext.Current.UserCompany.Id,
						IsCustomerLayerDashboard = true,
						LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId(),
						LastDate = DateTime.Now
					};
					TrainingGuideusageStatsViewModel trainingGuideUsageReport =
						ExecuteQuery<GetAllTrainingGuideUsageReportParameter, TrainingGuideusageStatsViewModel>(
							getAllTrainingGuideUsageReportParameter);

					double totalViewTrainingGuidCnt = trainingGuideUsageReport.TrainingGuidUsageList.Count;
					double persResultSeen = 0.00;
					double inputValue = Math.Round(persResultSeen, 2);

					double totalAssignedCount = trainingGuideUsageReport.TotalAssignedCount;
					double outstandingCount = totalAssignedCount - Convert.ToInt32(totalViewTrainingGuidCnt);

					var reportDataList = new List<ReportData>();
					if (totalViewTrainingGuidCnt > 0) {
						var d1 = new ReportData {
							label = "Seen",
							value = Convert.ToInt32(totalViewTrainingGuidCnt)
						};
						reportDataList.Add(d1);
					}

					if (outstandingCount > -1 && totalAssignedCount > 0) {
						var d = new ReportData {
							label = "Outstanding",
							value = Convert.ToInt32(outstandingCount)
						};
						reportDataList.Add(d);
					}

					if (totalAssignedCount > 0) {
						persResultSeen = (totalViewTrainingGuidCnt / totalAssignedCount) * 100;
					} else {
						persResultSeen = 0;
					}

					return Json(new { ReportList = reportDataList, PersResult = Math.Round(persResultSeen, 2) },
						JsonRequestBehavior.AllowGet);
				} else {
					var reportDataList = new List<ReportData>
					{
						new ReportData
						{
							label = "Play Book",
							value = 100
						}
					};
					double persResult = 100;
					return Json(new { ReportList = reportDataList, PersResult = persResult }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception) {
				return Json(null, JsonRequestBehavior.AllowGet);
			}
		}

		#endregion LoadDonutChartForPlayBookOutstandingNew

		#region LoadDonutChartForTestsCompleted

		//Bind donut chart for 'Tests Completed %'
		public JsonResult LoadDonutChartForTestsCompleted() {
			try {
				if (Role.IsInStandardUserRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList())) {
					var allAssignedTestsForAUserQueryParameter =
						new AllAssignedTestsForAUserQueryParameter {
							UserId = SessionManager.GetCurrentlyLoggedInUserId()
						};

					List<TrainingTestViewModel> allresult =
						ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
							allAssignedTestsForAUserQueryParameter);

					List<TrainingTestViewModel> testCompleted =
						 ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
							 allAssignedTestsForAUserQueryParameter)
							 .Where(em => !em.IsUserEligibleToTakeTest && em.TestStatus).ToList();

					int testNotattempted = allresult.Count - testCompleted.Count;
					var demoList = new List<ReportData>();

					if (testNotattempted > 0 || testCompleted.Count > 0) {
						var d = new ReportData {
							label = "Tests not Passed",
							value = testNotattempted
						};

						var d1 = new ReportData {
							label = "Tests Passed",
							value = testCompleted.Count
						};
						demoList.Add(d1);
						demoList.Add(d);
					}

					List<TrainingTestViewModel> modelNew =
						ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
							allAssignedTestsForAUserQueryParameter)
							.ToList();

					double totalTrainingTestCnt = modelNew.Count;
					double trainingTestCompletedCnt = 0;
					double trainingTestNotTakenCnt = 0;
					double persResult = 0;
					foreach (TrainingTestViewModel trainingTestViewModel in testCompleted) {
						if (!trainingTestViewModel.IsUserEligibleToTakeTest) {
							trainingTestCompletedCnt = trainingTestCompletedCnt + 1;
						}
					}
					if (trainingTestCompletedCnt > 0) {
						trainingTestNotTakenCnt = totalTrainingTestCnt - trainingTestCompletedCnt;
						persResult = (trainingTestCompletedCnt / totalTrainingTestCnt) * 100;
					} else {
						persResult = 0;
					}
					return Json(new { ReportList = demoList, PersResult = Math.Round(persResult, 2) },
						JsonRequestBehavior.AllowGet);
				} else {
					var demoList = new List<ReportData>
					{
						new ReportData
						{
							label = "Tests Passed",
							value = 100
						}
					};
					double persResult = 100;
					return Json(new { ReportList = demoList, PersResult = persResult }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception) {
				return Json(null, JsonRequestBehavior.AllowGet);
			}
		}

		#endregion LoadDonutChartForTestsCompleted

		#region About

		public ActionResult About() {
			ViewBag.Message = "Your application description page.";
			return View();
		}

		#endregion About

		#region Contact

		public ActionResult Contact() {
			ViewBag.Message = "Your contact page.";

			return View();
		}

		#endregion Contact

		#region ChangeRoleDescriptions

		public ActionResult ChangeRoleDescriptions() {
			MainContext mainDb = new MainContext();
			var customerCompanies =
				mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { CompanyId = c.Id });
			foreach (var company in customerCompanies) {
				PortalContext.Override(company.CompanyId);
				ExecuteCommand(new UpdateRoleDescriptionsCommand {
					RoleDescriptionDictionary = Ramp.Contracts.Security.Role.RoleDescriptionDictionary
				});
			}

			return View();
		}

		#endregion ChangeRoleDescriptions

		#region AddTrainingGuideCreatorsToCollaboratorsList

		public ActionResult AddTrainingGuideCreatorsToCollaboratorsList() {
			MainContext mainDb = new MainContext();
			var customerCompanies =
				mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany)
					.Select(c => new { CompanyId = c.Id });
			foreach (var customerCompany in customerCompanies) {
				PortalContext.Override(customerCompany.CompanyId);
				ExecuteCommand(new AddCreatorToCollaboratorsListForCustomerCompanyCommand());
			}
			return View();
		}

		#endregion AddTrainingGuideCreatorsToCollaboratorsList

		#region DataMigrationToCustomerCompanies

		public ActionResult DataMigrationToCustomerCompanies() {
			MainContext mainDb = new MainContext();
			var customerCompanies =
				mainDb.CompanySet.Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
			foreach (var company in customerCompanies) {
				PortalContext.Override(company.CompanyId);
				var groups = mainDb.GroupSet.Where(g => g.CompanyId.Equals(company.CompanyId)).ToList();
				groups.ForEach(g =>
					ExecuteCommand(new SaveOrUpdateGroupCommand() {
						CompanyId = g.CompanyId,
						Title = g.Title,
						Description = g.Description,
						IsforSelfSignUpGroup = g.IsforSelfSignUpGroup
					})
					);
				var adminUsers =
					mainDb.UserSet.Where(
						u =>
							u.CompanyId.Equals(company.CompanyId) &&
							u.Roles.Any(r => r.RoleName.Equals(UserRole.CustomerAdmin.ToString()))).ToList();
				var standardUsers =
					mainDb.UserSet.Where(
						u =>
							u.CompanyId.Equals(company.CompanyId) &&
							u.Roles.Any(r => r.RoleName.Equals(UserRole.CustomerStandardUser.ToString()))).ToList();
				foreach (var user in adminUsers) {
					MigrateUser(user, company.CompanyId);
				}
				foreach (var user in standardUsers) {
					MigrateUser(user, company.CompanyId);
				}
				var allUsers = new List<User>();
				allUsers.AddRange(adminUsers);
				allUsers.AddRange(standardUsers);
				allUsers.ForEach(u => ExecuteCommand(new DeleteProvisionalCompanyUserCommandParameter {
					ProvisionalComapanyUserId = u.Id
				}));
			}
			return View();
		}

		private void MigrateUser(User user, Guid companyId) {
			var customerGroups =
				ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter {
					CompanyId = companyId
				});

			var addCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand() {
				CompanyId = companyId,
				EmailAddress = user.EmailAddress,
				EmployeeNo = user.EmployeeNo,
				ExpireDays = user.ExpireDays,
				FirstName = user.FirstName,
				LastName = user.LastName,
				MobileNumber = user.MobileNumber,
				ParentUserId = user.ParentUserId,
				Password = new EncryptionHelper().Decrypt(user.Password),
				IsFromDataMigration = true
			};
			Guid groupId = Guid.Empty;
			if (user.Group != null && user.Group.Title != null)
				if (customerGroups.SingleOrDefault(g => g.Title.Equals(user.Group.Title)) != null)
					groupId = customerGroups.SingleOrDefault(g => g.Title.Equals(user.Group.Title)).GroupId;

			//if (groupId != Guid.Empty)
			//	addCommand.SelectedGroupId = groupId;

			foreach (var role in user.Roles) {
				if (role.RoleName.Equals(EnumHelper.GetDescription(UserRole.CustomerAdmin)))
					addCommand.Roles.Add(Role.CustomerAdmin);
				if (role.RoleName.Equals(EnumHelper.GetDescription(UserRole.CustomerStandardUser)))
					addCommand.Roles.Add(Role.StandardUser);
			}

			ExecuteCommand(addCommand);

			ExecuteCommand(new UpdateAssignedTrainingGuideCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateAssignedTrainingTestCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateCorrespondanceLogsCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateUserActivityLogCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateUserLoginStatsCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateTestResultCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateTrainingTestUsageStatsCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateTrainingGuideUsageStatsCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateTrainingGuideUserCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateTrainingTestUserCommand {
				PreviousUserId = user.Id
			});
			ExecuteCommand(new UpdateParentUserIdCommand {
				PreviousUserId = user.Id
			});
		}

		public ActionResult UpdateActivityLogDescriptions() {
			var companies = new MainContext().CompanySet.Select(c => new { CompanyId = c.Id });
			foreach (var company in companies) {
				PortalContext.Override(company.CompanyId);
				ExecuteCommand(new UpdateActivityLogDescriptionsCommand { UpdateActivityIdentity = true });
			}

			return View();
		}

		#endregion DataMigrationToCustomerCompanies

		#region PlayBookImageMigrationWithDb

		public ActionResult PlayBookImageMigrationWithDb() {
			PlayBookImageMigrationWithDbModel objModel = new PlayBookImageMigrationWithDbModel();
			try {
				MainContext mainDb = new MainContext();
				var companyList = mainDb.Company.Where(em => em.CompanyType == CompanyType.CustomerCompany).ToList();
				if (companyList.Count > 0) {
					foreach (var compay in companyList) {
						CustomerCompany CustomerCompany = new CustomerCompany();
						CustomerCompany.CompanyName = compay.CompanyName;
						objModel.dicCompany.Add(compay.Id, compay);
						//string abc = compay.CompanyName;
						var companyConnectionString = compay.CompanyConnectionString;
						CustomerContext customerdb = new CustomerContext(companyConnectionString);
						var trainingGuidList = customerdb.TrainingGuideSet.ToList();
						//get all the tests for the customer
						var trainingTests = customerdb.TrainingTestSet.ToList();

						foreach (var trainingGuid in trainingGuidList) {
							var companyTrainingGuide = new CompanyTrainingGuide();
							companyTrainingGuide.TrainingGuideName = trainingGuid.Title;
							foreach (var chapter in trainingGuid.ChapterList) {
								var companyTrainingGuideChapter = new CompanyTrainingGuideChapter();
								companyTrainingGuideChapter.ChapterName = chapter.ChapterName;
								foreach (var chapterUpload in chapter.ChapterUploads) {
									var uploadedDocuments = new Uploads {
										DocumentName = chapterUpload.DocumentName,
										UploadId = chapterUpload.Id
									};
									companyTrainingGuideChapter.chapterUploads.Add(uploadedDocuments);
								}
								companyTrainingGuide.CompanyTrainingGuideChapters.Add(companyTrainingGuideChapter);
							}
							CustomerCompany.CompanyTrainingGuides.Add(companyTrainingGuide);
							foreach (var test in trainingTests.Where(t => t.TrainingGuideId.Value.ToString().Equals(trainingGuid.Id.ToString()))) {
								moveTestFiles(test, CustomerCompany.CompanyName);
							}
							customerdb.SaveChanges();
						}
						objModel.CustomerCompanies.Add(CustomerCompany);
					}
				}
			}
			catch (Exception ex) {
				//ViewBag.Message = ex.Message;
				ViewBag.Message = ex.Message + "<br>, InnerException: " + ex.InnerException;
				Utility.LogManager.Fatal(ex, Utility.LogManager.LoggerFileName.PlayBookImageMigrationWithDb);
			}
			return View(objModel);
		}

		private void moveTestFiles(TrainingTest test, string companyName) {
			#region set up paths

			Dictionary<FileDirectoryLocations, string> fromPaths1 = new Dictionary<FileDirectoryLocations, string>();
			Dictionary<FileDirectoryLocations, string> fromPaths2 = new Dictionary<FileDirectoryLocations, string>();
			var toPaths = new RampLocationManager(Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]), companyName, test.TrainingGuideId.Value, test.Id);

			//from location 1
			fromPaths1.Add(FileDirectoryLocations.UploadedFiles, Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]));
			fromPaths1.Add(FileDirectoryLocations.CompanyRoot, Path.Combine(fromPaths1[FileDirectoryLocations.UploadedFiles], companyName));
			fromPaths1.Add(FileDirectoryLocations.GuideRoot, Path.Combine(fromPaths1[FileDirectoryLocations.CompanyRoot], test.TrainingGuide.Title));
			fromPaths1.Add(FileDirectoryLocations.TestRoot, Path.Combine(fromPaths1[FileDirectoryLocations.GuideRoot], test.TestTitle));

			//from location 2
			fromPaths2.Add(FileDirectoryLocations.UploadedFiles, Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]));
			fromPaths2.Add(FileDirectoryLocations.CompanyRoot, Path.Combine(fromPaths2[FileDirectoryLocations.UploadedFiles], companyName));
			fromPaths2.Add(FileDirectoryLocations.GuideRoot, Path.Combine(fromPaths2[FileDirectoryLocations.CompanyRoot], test.TrainingGuide.Id.ToString()));
			fromPaths2.Add(FileDirectoryLocations.TestRoot, Path.Combine(fromPaths2[FileDirectoryLocations.GuideRoot], test.Id.ToString()));

			#endregion set up paths

			#region Create the new test directories

			toPaths.doCreateToTestRoot();

			#endregion Create the new test directories

			if (test.QuestionList != null) {
				doCheckAndMove(fromPaths1, test.QuestionList.ToList(), toPaths);
				doCheckAndMove(fromPaths2, test.QuestionList.ToList(), toPaths);
			}
		}

		private void doCheckAndMove(Dictionary<FileDirectoryLocations, string> location, List<TrainingQuestion> questions, RampLocationManager toPaths) {
			try {
				if (Directory.Exists(location[FileDirectoryLocations.CompanyRoot]))
					if (Directory.Exists(location[FileDirectoryLocations.GuideRoot]))
						if (Directory.Exists(location[FileDirectoryLocations.TestRoot])) {
							foreach (TrainingQuestion question in questions) {
								var files = Directory.GetFiles(location[FileDirectoryLocations.TestRoot]);
								foreach (string longfileName in files) {
									var fileName = Path.GetFileName(longfileName);
									if (!string.IsNullOrEmpty(question.ImageName)) {
										if (question.ImageName.Equals(fileName)) {
											string sourceFilePath = Path.Combine(location[FileDirectoryLocations.TestRoot], fileName);
											if (System.IO.File.Exists(sourceFilePath)) {
												string fileExtension = Path.GetExtension(sourceFilePath);
												string fileNameToSave = question.Id.ToString() + fileExtension;
												string detinationFilePath = Path.Combine(toPaths.TestRoot, fileNameToSave);
												System.IO.File.Copy(sourceFilePath, detinationFilePath, true);
												question.ImageName = fileNameToSave;
											}
										}
									}
									if (!string.IsNullOrEmpty(question.VideoName)) {
										if (question.VideoName.Equals(fileName)) {
											string sourceFilePath = Path.Combine(location[FileDirectoryLocations.TestRoot], fileName);
											if (System.IO.File.Exists(sourceFilePath)) {
												string fileExtension = Path.GetExtension(sourceFilePath);
												string fileNameToSave = question.Id.ToString() + fileExtension;
												string detinationFilePath = Path.Combine(toPaths.TestRoot, fileNameToSave);
												System.IO.File.Copy(sourceFilePath, detinationFilePath, true);
												question.VideoName = fileNameToSave;
											}
										}
									}
								}
							}
						}
			}
			catch (IOException) { }
		}

		#endregion PlayBookImageMigrationWithDb

		#region PlayBookImageMigrationWithDb

		[HttpPost]
		public ActionResult PlayBookImageMigrationWithDb(string id) {
			PlayBookImageMigrationWithDbModel objModel = new PlayBookImageMigrationWithDbModel();
			bool isError = false;
			try {
				MainContext mainDb = new MainContext();

				var rootPath = ConfigurationManager.AppSettings["TrainingGuidFilePath"];
				rootPath = Server.MapPath(rootPath);

				List<string> filePathsToDelete = new List<string>();

				var companyList = mainDb.Company.Where(em => em.CompanyType == CompanyType.CustomerCompany).ToList();
				if (companyList.Count > 0) {
					foreach (var compay in companyList) {
						CustomerCompany CustomerCompany = new CustomerCompany();
						CustomerCompany.CompanyName = compay.CompanyName;
						objModel.dicCompany.Add(compay.Id, compay);
						var companyConnectionString = compay.CompanyConnectionString;
						CustomerContext customerdb = new CustomerContext(companyConnectionString);
						var trainingGuidList = customerdb.TrainingGuideSet.ToList();

						foreach (var trainingGuid in trainingGuidList) {
							var companyTrainingGuide = new CompanyTrainingGuide();

							string coverImagePath = Path.Combine(rootPath, compay.CompanyName + "_" + trainingGuid.Id + "_CoverPic");
							string imagePath = coverImagePath + "/" + "CoverPic.jpg";

							if (System.IO.File.Exists(imagePath)) {
								trainingGuid.CoverPicFileContent = System.IO.File.ReadAllBytes(imagePath);
								filePathsToDelete.Add(imagePath);
							} else {
								coverImagePath = Path.Combine(rootPath, compay.CompanyName + "_" + trainingGuid.Title + "_CoverPic");
								imagePath = coverImagePath + "/" + "CoverPic.jpg";

								if (System.IO.File.Exists(imagePath)) {
									trainingGuid.CoverPicFileContent = System.IO.File.ReadAllBytes(imagePath);
									filePathsToDelete.Add(imagePath);
								}
							}

							companyTrainingGuide.TrainingGuideName = trainingGuid.Title;
							foreach (var chapter in trainingGuid.ChapterList) {
								var companyTrainingGuideChapter = new CompanyTrainingGuideChapter();
								companyTrainingGuideChapter.ChapterName = chapter.ChapterName;
								foreach (var chapterUpload in chapter.ChapterUploads) {
									var uploadedDocuments = new Uploads {
										DocumentName = chapterUpload.DocumentName,
										UploadId = chapterUpload.Id
									};
									companyTrainingGuideChapter.chapterUploads.Add(uploadedDocuments);

									try {
										var documentName = chapterUpload.DocumentName;
										var documentType = chapterUpload.DocumentType;
										var documentFileContent = chapterUpload.DocumentFileContent;
										var msgId = "msg" + chapterUpload.Id;
										if (documentFileContent == null) {
											//Checking Company folder is Exists or not
											string companyDirectoryPath = Path.Combine(rootPath, compay.CompanyName.Trim());
											if (System.IO.Directory.Exists(companyDirectoryPath)) {
												//Checking TrainingGuid folder is Exists or not
												string trainingGuidDirectoryPath = Path.Combine(companyDirectoryPath, Convert.ToString(trainingGuid.Id));
												if (System.IO.Directory.Exists(trainingGuidDirectoryPath)) {
													//Checking TraningGuideChapter File is Exists or not
													string traningGuideChapterFile = Path.Combine(trainingGuidDirectoryPath, chapterUpload.DocumentName);
													if (System.IO.File.Exists(traningGuideChapterFile)) {
														var chapterUploadUpdate = customerdb.ChapterUploadSet.FirstOrDefault(em => em.Id == chapterUpload.Id);
														chapterUploadUpdate.DocumentFileContent = ReadByteFile(traningGuideChapterFile);
														//chapterUploadUpdate.DocumentFileContent = ReadByteFile("a");
														customerdb.SaveChanges();

														ViewBag.Message = "PlayBook Image Migration is completed successfully";
														//System.IO.File.Delete(trainingGuidDirectoryPath);
														TempData[msgId] = "  <b style='color: green;'> --> Migration successful<b>";
													} else {
														TempData[msgId] = "  <b style='color: #523E2D;'> --> File does Not Exists<b>";
													}
												}
											}
											//chapterUpload.DocumentName = chapterUpload.DocumentName + "  <b style='color: green;'> --> Migration successful<b>";
											chapterUpload.DocumentName = chapterUpload.DocumentName;
											//TempData[msgId] = "  <b style='color: green;'> --> Migration successful<b>";
										} else {
											//chapterUpload.DocumentName = chapterUpload.DocumentName + " <b style='color: red;'> --> Already exists in database<b>";
											chapterUpload.DocumentName = chapterUpload.DocumentName;
											TempData[msgId] = " <b style='color: red;'> --> Already exists in database<b>";
										}
										objModel.dicChapterUpload.Add(chapterUpload.Id, chapterUpload);
									}
									catch (Exception ex) {
										ViewBag.ErrorMessage = "Error1: " + ex.Message + "<br>, InnerException: " + ex.InnerException;
										isError = true;
										Utility.LogManager.Fatal(ex, Utility.LogManager.LoggerFileName.PlayBookImageMigrationWithDb);
									}
								}
								companyTrainingGuide.CompanyTrainingGuideChapters.Add(companyTrainingGuideChapter);
							}
							CustomerCompany.CompanyTrainingGuides.Add(companyTrainingGuide);
						}
						objModel.CustomerCompanies.Add(CustomerCompany);

						customerdb.SaveChanges();

						try {
							filePathsToDelete.ForEach(System.IO.File.Delete);
						}
						catch { }
					}
				}
			}
			catch (Exception ex) {
				ViewBag.ErrorMessage = "Error2: " + ex.Message + "<br>, InnerException: " + ex.InnerException;
				isError = true;
				Utility.LogManager.Fatal(ex, Utility.LogManager.LoggerFileName.PlayBookImageMigrationWithDb);
			}

			if (isError)
				ViewBag.Message = "Migration is partially done, Please see the error log.";
			else
				ViewBag.Message = "Migration is done successfully.";
			return View(objModel);
		}

		#endregion PlayBookImageMigrationWithDb

		#region TrainingTestUploadMigrationWithDb

		public ActionResult TrainingTestUploadMigrationWithDb() {
			MainContext mainDb = new MainContext();
			var customerCompanies =
				mainDb.CompanySet.Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
			foreach (var company in customerCompanies) {
				PortalContext.Override(company.CompanyId);
				ExecuteCommand(new MigrateTestsToDbCommand {
					CompanyName = PortalContext.Current.UserCompany.CompanyName,
					PathToSaveUploadedFiles = Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"])
				});
			}

			return View();
		}

		#endregion TrainingTestUploadMigrationWithDb

		#region PlaybookMigrationToKnockout

		public ActionResult PlaybookMigrationToKnockout() {
			MainContext mainDb = new MainContext();
			var customerCompanies =
				mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
			foreach (var company in customerCompanies) {
				PortalContext.Override(company.CompanyId);
				ExecuteCommand(new MigrateTrainingGuidesToKnockoutCommand());
			}

			return View();
		}

		#endregion PlaybookMigrationToKnockout

		#region TestMigrationToKnockout

		public ActionResult TestMigrationToKnockout() {
			MainContext mainDb = new MainContext();
			var customerCompanies =
				mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
			foreach (var company in customerCompanies) {
				PortalContext.Override(company.CompanyId);
				ExecuteCommand(new MigrateTestsToKnockoutCommand());
			}
			return View("PlaybookMigrationToKnockout");
		}

		#endregion TestMigrationToKnockout

		#region TestResultMigration

		public ActionResult TestResultMigration() {
			MainContext mainDb = new MainContext();
			var customerCompanies =
				mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
			foreach (var company in customerCompanies) {
				PortalContext.Override(company.CompanyId);
				ExecuteCommand(new MigrateTestResultCommandParameter());
			}
			return View("PlaybookMigrationToKnockout");
		}

		#endregion TestResultMigration

		#region ErrorLogShow

		public ActionResult ErrorLogShow() {
			// ...
			string hostname = HttpContext.Request.Url.AbsoluteUri;
			string urlToRemove = hostname.Substring(hostname.IndexOf("Home/ErrorLogShow"));
			string secondlasturl = hostname.Replace(urlToRemove, String.Empty);
			var url = secondlasturl + "elmah.axd";
			return new RedirectResult(url, true);
			// return Redirect(url);
		}

		#endregion ErrorLogShow

		#region ErrorPage

		// Common Error landing Page
		public ActionResult ErrorPage() {
			return View();
		}

		#endregion ErrorPage

		#region CanvasUpload

		//Canvas upload
		[HttpGet]
		public ActionResult CanvasUpload() {
			string hostname = HttpContext.Request.Url.AbsoluteUri;
			string urlToRemove = hostname.Substring(hostname.IndexOf("Home/CanvasUpload"));
			string secondlasturl = hostname.Replace(urlToRemove, String.Empty);
			ViewBag.root = secondlasturl;

			LoadBackImage();

			return View();
		}

		[HttpPost]
		public ActionResult CanvasUpload(HttpPostedFileBase file) {
			LoadBackImage();
			if (file != null) {
				if (file.ContentType == "image/jpeg") {
					#region File

					if (file.FileName != "Certificate.jpg") {
						if (file != null && file.ContentLength > 0) {
							using (Stream fileStream = file.InputStream) {
								int width = 0;
								int height = 0;
								using (var mStreamer = new MemoryStream()) {
									mStreamer.SetLength(fileStream.Length);
									fileStream.Read(mStreamer.GetBuffer(), 0, (int)fileStream.Length);
									mStreamer.Seek(0, SeekOrigin.Begin);
									byte[] fileBytes = mStreamer.GetBuffer();

									System.Drawing.Image img = System.Drawing.Image.FromStream(mStreamer);

									width = img.Width;
									height = img.Height;

									if (width == 2480 && height == 3508) {
										if (img.HorizontalResolution == 300f && img.VerticalResolution == 300f) {
											ViewBag.Path = file.FileName;
											var customC = new Ramp.Contracts.CommandParameter.Settings.CreatedUpdateCustomConfigurationCommandParameter {
												Cert = new FileUploads {
													Name = file.FileName,
													Description = file.FileName,
													Type = TrainingDocumentTypeEnum.Certificate.ToString(),
													ContentType = file.ContentType,
													Data = fileBytes,
													Id = Guid.NewGuid()
												}
											};
											ExecuteCommand(customC);

											fileStream.Close();
										} else {
											ModelState.AddModelError("", "Uploaded certificate resolution should be 300 dpi.");
											ViewBag.Path = null;
											img.Dispose();
										}
									} else {
										ModelState.AddModelError("", "Uploaded certificate should be A4 size ie 2480x3508.");
										ViewBag.Path = null;
										img.Dispose();
									}

									img.Dispose();
								}
								LoadBackImage();
							}
						}

						string hostname = HttpContext.Request.Url.AbsoluteUri;
						string urlToRemove = hostname.Substring(hostname.IndexOf("Home/CanvasUpload"));
						string secondlasturl = hostname.Replace(urlToRemove, String.Empty);
						ViewBag.root = secondlasturl;
						//ViewBag.Path = Server.MapPath("~/Content/images/" + file.FileName);

						ViewBag.Message1 = "This is first Message.";
						ViewBag.Message2 = "This is second Message.";
						ViewBag.Message3 = "This is third Message.";
					} else {
						ModelState.AddModelError("", "File Name should be different");
						string hostname = HttpContext.Request.Url.AbsoluteUri;
						string urlToRemove = hostname.Substring(hostname.IndexOf("Home/CanvasUpload"));
						string secondlasturl = hostname.Replace(urlToRemove, String.Empty);
						ViewBag.root = secondlasturl;
						ViewBag.Message1 = "This is first Message.";
						ViewBag.Message2 = "This is second Message.";
						ViewBag.Message3 = "This is third Message.";
					}

					#endregion File
				} else {
					ModelState.AddModelError("", "Please upload a JPEG file that is A4 in size (ie. 2480x3508) and has a resolution of 300 dpi.");
					string hostname = HttpContext.Request.Url.AbsoluteUri;
					string urlToRemove = hostname.Substring(hostname.IndexOf("Home/CanvasUpload"));
					string secondlasturl = hostname.Replace(urlToRemove, String.Empty);
					//ViewBag.root = secondlasturl;
					ViewBag.Message1 = "This is first Message.";
					ViewBag.Message2 = "This is second Message.";
					ViewBag.Message3 = "This is third Message.";
				}
			} else {
				ModelState.AddModelError("", "Please select File for Upload.");
				string hostname = HttpContext.Request.Url.AbsoluteUri;
				string urlToRemove = hostname.Substring(hostname.IndexOf("Home/CanvasUpload"));
				string secondlasturl = hostname.Replace(urlToRemove, String.Empty);
				//ViewBag.root = secondlasturl;
				ViewBag.Message1 = "This is first Message.";
				ViewBag.Message2 = "This is second Message.";
				ViewBag.Message3 = "This is third Message.";
			}

			return View();
		}

		private void LoadBackImage() {
			var fileQuery = new GetCustomConfigurationQueryParameter();

			var custom = ExecuteQuery<GetCustomConfigurationQueryParameter, CustomConfigurationViewModel>(fileQuery);
			byte[] data = null;
			string contentType = string.Empty;
			if (custom.Certificate != null) {
				data = custom.Certificate.Data;
				contentType = custom.Certificate.ContentType;
			}
			if (data == null || data.Length < 1) {
				contentType = "image/jpeg";
				data = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/images/Certificate.jpg"));
			}
			string imageBase64 = Convert.ToBase64String(data);
			string imageSrc = string.Format("data:" + contentType + ";base64,{0}", imageBase64);
			ViewBag.BackImage = imageSrc;
		}

		#endregion CanvasUpload

		#region SaveImageCertificate

		[HttpPost]
		public ActionResult ResetImageCertificate() {
			var deleteCommand = new ClearCustomConfigurationCommadParameter {
				Certificate = true
			};

			ExecuteCommand(deleteCommand);

			return null;
		}

		#endregion SaveImageCertificate

		#region UploadCustomCSS

		public ActionResult UploadCustomCSS() {
			return View();
		}

		[HttpPost]
		public ActionResult UploadCustomCSS(HttpPostedFileBase file) {
			if (file != null) {
				#region File

				if (file.FileName == "bootstrap.css" && file.ContentType == "text/css") {
					if (file != null && file.ContentLength > 0) {
						if (PortalContext.Current != null) {
							Guid CompanyId = PortalContext.Current.UserCompany.Id;

							var fileName = CompanyId + "_" + file.FileName;

							file.SaveAs(Server.MapPath("~/Content/" + fileName));

							// save uploaded file in database with status;

							var path = Server.MapPath("~/Content/" + fileName);
							byte[] CSSFile = ReadByteFile(path);

							var uploadCustomCSSCommandParameter = new UploadCustomCSSCommandParameter {
								CSSFile = CSSFile,
								CompanyId = CompanyId,
							};

							ExecuteCommand(uploadCustomCSSCommandParameter);

							ModelState.AddModelError("", "Custom css uploaded sucessfully");

							return View();
						} else {
							ModelState.AddModelError("", "Please login to system");
						}
					}
				} else {
					ModelState.AddModelError("", "File Name should be bootstrap.css and Content type=text/css");
				}

				#endregion File
			} else {
				ModelState.AddModelError("", "Please slect File for Upload.");
			}

			return View();
		}

		#endregion UploadCustomCSS

		#region GetCustomCss

		[HttpPost]
		public string GetCustomCss(Guid companyId) {
			MainContext _db = new MainContext();
			var cssPath = "";
			if (companyId != null) {
				//string hostname = HttpContext.Request.Url.AbsoluteUri;
				//string urlToRemove = hostname.Substring(hostname.IndexOf("/Home/GetCustomCss/"));
				//string secondlasturl = hostname.Replace(urlToRemove, String.Empty);

				var company = _db.Company.Where(c => c.Id == companyId).FirstOrDefault();

				if (company.ApplyCustomCss == true) {
					var FileName = company.Id + "_bootstrap.css";
					var path = Server.MapPath("~/Content/" + FileName);

					if (System.IO.File.Exists(path)) {
						cssPath = VirtualPathUtility.ToAbsolute("~/Content/" + FileName);
					} else {
						var myFile = System.IO.File.Create(Server.MapPath("~/Content/" + FileName));
						myFile.Close();

						if (System.IO.File.Exists(Server.MapPath("~/Content/" + FileName))) {
							bool fileCreate = SaveData(company.Id + "_bootstrap.css", company.customCssFile);

							var pathFile = Server.MapPath("~/Content/" + FileName);

							if (System.IO.File.Exists(pathFile)) {
								cssPath = VirtualPathUtility.ToAbsolute("~/Content/" + FileName);
							}
						}
					}
				} else {
					cssPath = "";
				}
			}

			return cssPath;
		}

		#endregion GetCustomCss

		#region SaveData

		protected bool SaveData(string FileName, byte[] Data) {
			BinaryWriter Writer = null;

			string Name = Server.MapPath("~/Content/" + FileName);

			try {
				// Create a new stream to write to the file
				Writer = new BinaryWriter(System.IO.File.OpenWrite(Name));

				// Writer raw data
				Writer.Write(Data);
				Writer.Flush();
				Writer.Close();
			}
			catch {
				//...
				return false;
			}

			return true;
		}

		#endregion SaveData

		#region ReadByteFile

		private byte[] ReadByteFile(string sPath) {
			//Initialize byte array with a null value initially.
			byte[] data = null;
			//Use FileInfo object to get file size.
			FileInfo fInfo = new FileInfo(sPath);
			long numBytes = fInfo.Length;
			//Open FileStream to read file
			FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);
			//Use BinaryReader to read file stream into byte array.
			BinaryReader br = new BinaryReader(fStream);
			//When you use BinaryReader, you need to supply number of bytes to read from file.
			//In this case we want to read entire file. So supplying total number of bytes.
			data = br.ReadBytes((int)numBytes);
			return data;
		}

		#endregion ReadByteFile

		#region TrophyCabinet

		/// <summary>
		/// Trophy Cabinet for Customer Standered User
		/// </summary>
		/// <returns></returns>
		public ActionResult TrophyCabinet() {
			return View(ExecuteQuery<AllTrophiesByStandardUserQuery, List<TestResultViewModel>>(new AllTrophiesByStandardUserQuery() {
				UserId = SessionManager.GetCurrentlyLoggedInUserId(),
				DefaultTrophyPath = Server.MapPath("~/Content/TrophyPicDir/Temp")
			}));
		}

		#endregion TrophyCabinet

		#region GlobalSearch

		public ActionResult GlobalSearch(string searchText, string searchType) {
			var queryParameter = new GlobalSearchQueryParameter();
			List<GlobalSearchViewModel> globalSearchViewModel = new List<GlobalSearchViewModel>();
			if (Role.IsInAdminRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList())) {
				// code for Customer Admin.
				if (searchType.Equals("0"))
					queryParameter.SearchType = SearchTypes.All;
				queryParameter.SearchText = searchText;
				queryParameter.IsCustAdmin = true;
				queryParameter.LogedInCompanyId = PortalContext.Current.UserCompany.Id;
				globalSearchViewModel =
			   ExecuteQuery<GlobalSearchQueryParameter, List<GlobalSearchViewModel>>(queryParameter);

				//return PartialView("_TrophyCabinet", globalSearchViewModel);
			} else if (Role.IsInStandardUserRole(SessionManager.GetRolesForCurrentlyLoggedInUser().ToList())) {
				// code for Customer Standered User.
				if (searchType.Equals("0"))
					queryParameter.SearchType = SearchTypes.All;
				queryParameter.SearchText = searchText;
				queryParameter.LogedInCompanyId = PortalContext.Current.UserCompany.Id;
				queryParameter.IsCustAdmin = false;
				Guid userId = SessionManager.GetCurrentlyLoggedInUserId();
				queryParameter.UserId = userId;
				globalSearchViewModel =
			  ExecuteQuery<GlobalSearchQueryParameter, List<GlobalSearchViewModel>>(queryParameter);

				// return PartialView("_TrophyCabinet", globalSearchViewModel);
			}
			return PartialView("_GlobalSearch", globalSearchViewModel);
		}

		#endregion GlobalSearch



		#region UploadTrophy

		public ActionResult UploadTrophy() {
			UploadTrophyViewModel model = new UploadTrophyViewModel();
			var subPath = Server.MapPath("~/Content/TrophyPicDir");

			bool exists = System.IO.Directory.Exists(subPath);

			if (!exists) {
				System.IO.Directory.CreateDirectory(subPath);
				string[] files = Directory.GetFiles(subPath);
				foreach (string file in files) {
					string[] name = file.Split('\\');

					var fileName = name[name.Length - 1];
					// File.Copy(file, "....");
					//model.uploadedList.Add(fileName);
					var imageName = fileName.Split('.');
					if (imageName[1] == "jpg" || imageName[1] == "png" || imageName[1] == "JPG" || imageName[1] == "PNG" || imageName[1] == "GIF" || imageName[1] == "gif") {
						model.uploadedList.Add(fileName);
					}
				}
			} else {
				string[] files = Directory.GetFiles(subPath);
				foreach (string file in files) {
					string[] name = file.Split('\\');

					var fileName = name[name.Length - 1];
					// File.Copy(file, "....");
					//model.uploadedList.Add(fileName);
					var imageName = fileName.Split('.');
					if (imageName[1] == "jpg" || imageName[1] == "png" || imageName[1] == "JPG" || imageName[1] == "PNG" || imageName[1] == "GIF" || imageName[1] == "gif") {
						model.uploadedList.Add(fileName);
					}
				}
			}
			//var path = Server.MapPath("~/Content/TrophyPic");
			//string[] files = Directory.GetFiles(path);

			//foreach (string file in files)
			//{
			//    string[] name = file.Split('\\');

			//    var fileName = name[name.Length - 1];
			//    // File.Copy(file, "....");
			//    //model.uploadedList.Add(fileName);
			//    var imageName = fileName.Split('.');
			//    if (imageName[1] == "jpg" || imageName[1] == "png" || imageName[1] == "JPG" || imageName[1] == "PNG" || imageName[1] == "GIF" || imageName[1] == "gif")
			//    {
			//        model.uploadedList.Add(fileName);
			//    }
			//}

			return View(model);
		}

		[HttpPost]
		public ActionResult UploadTrophy(HttpPostedFileBase[] files) {
			var subPath = Server.MapPath("~/Content/TrophyPicDir");
			UploadTrophyViewModel model = new UploadTrophyViewModel();

			bool exists = System.IO.Directory.Exists(subPath);

			if (exists) {
				if (files.Length > 0) {
					foreach (HttpPostedFileBase file in files) {
						if (file != null) {
							var name = file.FileName;
							var uploadedFileName = name.Split('.');

							if (uploadedFileName[1] == "jpg" || uploadedFileName[1] == "png" || uploadedFileName[1] == "JPG" || uploadedFileName[1] == "PNG" || uploadedFileName[1] == "GIF" || uploadedFileName[1] == "gif") //|| uploadedFileName[1] == "GIF" || uploadedFileName[1] == "gif"
							{
								HttpPostedFileBase uploadedFile = file;
								var path = Server.MapPath("~/Content/TrophyPicDir");
								string filePath = Path.Combine(path, file.FileName);
								uploadedFile.SaveAs(filePath);
							} else {
								ModelState.AddModelError("", "Please upload image file with extension as a jpg, png or gif");
							}
						} else {
							ModelState.AddModelError("", "Please upload image file");
						}
					}
				} else {
					ModelState.AddModelError("", "Please upload image file");
				}

				//UploadTrophyViewModel model = new UploadTrophyViewModel();
				var Imagepath = Server.MapPath("~/Content/TrophyPicDir");
				string[] filesGet = Directory.GetFiles(Imagepath);

				foreach (string file in filesGet) {
					string[] name = file.Split('\\');

					var fileName = name[name.Length - 1];
					// File.Copy(file, "....");
					//model.uploadedList.Add(fileName);
					var imageName = fileName.Split('.');
					if (imageName[1] == "jpg" || imageName[1] == "png" || imageName[1] == "JPG" || imageName[1] == "PNG" || imageName[1] == "GIF" || imageName[1] == "gif") {
						model.uploadedList.Add(fileName);
					}
				}
			} else {
				ModelState.AddModelError("", "Directory is created.");
			}

			return View(model);
		}

		#endregion UploadTrophy

		[HttpGet]
		public JsonResult GetUploadTrophy() {
			var subPath = Server.MapPath("~/Content/TrophyPicDir");
			var uploadedTrophyList = new List<TrophyModel>();
			if (Directory.Exists(subPath)) {
				var files = Directory.GetFiles(subPath);
				foreach (var filePath in files) {
					if (System.IO.File.Exists(filePath)) {
						var bytes = System.IO.File.ReadAllBytes(filePath);
						uploadedTrophyList.Add(new TrophyModel { FileName = Path.GetFileNameWithoutExtension(filePath), Content = Convert.ToBase64String(bytes) });
					}
				}
			}

			if (uploadedTrophyList.Count == 0) {
				var defaultTrophyPath = Server.MapPath("~/Content/images/Trophy5.png");
				if (System.IO.File.Exists(defaultTrophyPath)) {
					var bytes = System.IO.File.ReadAllBytes(defaultTrophyPath);
					uploadedTrophyList.Add(new TrophyModel { FileName = Path.GetFileNameWithoutExtension(defaultTrophyPath), Content = Convert.ToBase64String(bytes) });
				}
			}
			return Json(uploadedTrophyList.OrderBy(x => x.FileName).ToArray(), JsonRequestBehavior.AllowGet);
		}
		
	}

	#region ReportData

	public class ReportData {
		public string label { get; set; }
		public int value { get; set; }
	}
}

#endregion ReportData