using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using Data.EF;
using Domain.Customer;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.UserFeedback;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.Security;
using VirtuaCon;
using Web.UI.Code.Extensions;
using Ramp.Contracts.Query.TestSession;
using Ramp.Contracts.Query.CheckList;
using Ramp.Services.Projection;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.Query.TrainingManual;
using Domain.Customer.Models;
using Web.UI.Models;
using Common.Query;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.Query.Reporting;
using Domain.Enums;
using Ramp.Contracts.Query.CustomDocument;

namespace Web.UI.Controllers
{
	//dummy 2 commit to test devop working
	public class DocumentController : KnockoutPagedListController<DocumentListQuery, DocumentListModel>
	{

		private readonly MainContext _mainContext = new MainContext();

		public override void Index_PostProcess(IEnumerable<DocumentListModel> listModel)
		{
			var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();

			categories.ForEach(x => {
				if (x.parent == "#")
					x.parent = $"{Guid.Empty}";
			});

			(categories as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Category" });

			ViewBag.Categories = categories;

			ViewBag.DocumentsRemaining = ExecuteQuery<DocumentsRemainingQuery, int>(new DocumentsRemainingQuery
			{
				CompanyId = PortalContext.Current.UserCompany.Id
			});

			ViewBag.BundleSize = PortalContext.Current.UserCompany.BundleSize;

			var collaborators = ExecuteQuery<AllContentAdminsForCustomerCompanyQuery, List<UserModelShort>>(new AllContentAdminsForCustomerCompanyQuery());

			if (Thread.CurrentPrincipal.IsInRole(Role.ContentAdmin) || Thread.CurrentPrincipal.IsInRole(Role.ContentCreator) || Thread.CurrentPrincipal.IsInRole(Role.ContentApprover))
			{
				var userId = Thread.CurrentPrincipal.GetId();
				collaborators.Remove(collaborators.Find(x => x.Id == userId));
			}
			ViewBag.Collaborators = collaborators;

			Guid companyId = PortalContext.Current.UserCompany.Id;
			var company = _mainContext.Company.FirstOrDefault(c => c.Id == companyId);

			if (company != null)
			{
				var provisionalCompany = _mainContext.Company.FirstOrDefault(c => c.Id == company.ProvisionalAccountLink);
				if (provisionalCompany != null)
				{
					if (company.IsSelfCustomer == true)
					{
						// self provision company
						DateTime createdDate = company.CreatedOn;
						DateTime addYear = createdDate.AddMonths(1);
						DateTime expireDate = addYear.AddDays(1);
						ViewBag.ExpiryDate = expireDate.ToString("dd-MMMM-yyyy");
						ViewBag.ProvisionalCompany = provisionalCompany.CompanyName;
						ViewBag.ProvCompanyCntNo = provisionalCompany.TelephoneNumber;
					}
					if (company.YearlySubscription != null)
					{
						if (company.YearlySubscription == true)
						{
							//  not self provision company
							DateTime createdDate = company.CreatedOn;
							DateTime addYear = createdDate.AddYears(1);
							DateTime expireDate = addYear.AddDays(1);
							ViewBag.ExpiryDate = expireDate.ToString("dd-MMMM-yyyy");
							ViewBag.ProvisionalCompany = provisionalCompany.CompanyName;
							ViewBag.ProvCompanyCntNo = provisionalCompany.TelephoneNumber;
						}
						else
						{
							DateTime createdDate = company.CreatedOn;
							DateTime addYear = createdDate.AddMonths(1);
							DateTime expireDate = addYear.AddDays(1);
							ViewBag.ExpiryDate = expireDate.ToString("dd-MMMM-yyyy");
							ViewBag.ProvisionalCompany = provisionalCompany.CompanyName;
							ViewBag.ProvCompanyCntNo = provisionalCompany.TelephoneNumber;
						}
					}
				}
			}

			#region chart

			var memoList = ExecuteQuery<MemoListQuery, IEnumerable<DocumentListModel>>(new MemoListQuery()).Where(c => !c.Deleted);
			var manualList = ExecuteQuery<TrainingManualListQuery, IEnumerable<DocumentListModel>>(new TrainingManualListQuery()).Where(c => !c.Deleted);
			var testList = ExecuteQuery<TestListQuery, IEnumerable<DocumentListModel>>(new TestListQuery()).Where(c => !c.Deleted);
			var policyList = ExecuteQuery<PolicyListQuery, IEnumerable<DocumentListModel>>(new PolicyListQuery()).Where(c => !c.Deleted);
			var checklistList = ExecuteQuery<CheckListQuery, IEnumerable<DocumentListModel>>(new CheckListQuery()).Where(c => !c.Deleted);
			var CustomDocList = ExecuteQuery<CustomDocumentListQuery, IEnumerable<DocumentListModel>>(new CustomDocumentListQuery()).Where(c => !c.Deleted);

			var assignedDocs = ExecuteQuery<FetchByIdQuery, List<AssignedDocumentModel>>(new FetchByIdQuery());

			var chartModel = new ChartViewModel
			{
				Categories = new List<int> { assignedDocs.Where(c => c.DocumentType == DocumentType.TrainingManual).Count(), assignedDocs.Where(c => c.DocumentType == DocumentType.Test).Count(), assignedDocs.Where(c => c.DocumentType == DocumentType.Policy).Count(), assignedDocs.Where(c => c.DocumentType == DocumentType.Memo).Count() },
				Name = new List<string> { DocumentType.TrainingManual.ToString(), DocumentType.Test.ToString(), DocumentType.Policy.ToString(), DocumentType.Memo.ToString(), DocumentType.custom.ToString() }
			};

			if (PortalContext.Current.UserCompany.EnableChecklistDocument)
			{
				chartModel.Categories.Add(assignedDocs.Where(c => c.DocumentType == DocumentType.Checklist).Count());
				//chartModel.Name.Add(DocumentType.Checklist.ToString());
				chartModel.Name.Add("Activity Book");
			}

			chartModel.Type = new List<string> { DocumentType.TrainingManual.ToString(), DocumentType.Test.ToString(), DocumentType.Policy.ToString(), DocumentType.Memo.ToString(), DocumentType.custom.ToString() };

			chartModel.Count = new List<int> { manualList.Count(), testList.Count(), policyList.Count(), memoList.Count(), CustomDocList.Count() };

			if (PortalContext.Current.UserCompany.EnableChecklistDocument)
			{
				//chartModel.Type.Add(DocumentType.Checklist.ToString());
				chartModel.Type.Add("Activity Book");
				chartModel.Count.Add(checklistList.Count());
			}

			var draftCount = 0;
			var publishedCount = 0;
			var recalledCount = 0;

			if (!PortalContext.Current.UserCompany.EnableChecklistDocument)
			{
				draftCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count();
				publishedCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count();
				recalledCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count();
			}
			else
			{
				draftCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + checklistList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count();
				publishedCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + checklistList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count();
				recalledCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + checklistList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count();
			}

			chartModel.Status = new List<string> { DocumentStatus.Draft.ToString(), DocumentStatus.Published.ToString(), DocumentStatus.Recalled.ToString() };
			chartModel.StatusCount = new List<int> { draftCount, publishedCount, recalledCount };

			ViewData["Chart"] = chartModel;

			//neeraj
			var contentCreatorChartModel = new ChartViewModel()
			{

				Name = new List<string> {
					DocumentPublishWorkflowStatus.Draft.ToString(),
					DocumentPublishWorkflowStatus.Submitted.ToString(),
					DocumentPublishWorkflowStatus.Approved.ToString(),
					DocumentPublishWorkflowStatus.Declined.ToString() }
			};

			var contentApproverChartModel = new ChartViewModel()
			{

				Name = new List<string> {
					DocumentPublishWorkflowStatus.Submitted.ToString(),
					DocumentPublishWorkflowStatus.Approved.ToString(),
					DocumentPublishWorkflowStatus.Declined.ToString() }
			};


			//neeraj
			#endregion

			foreach (var model in listModel)
			{
				if (Thread.CurrentPrincipal.IsInRole(Role.ContentCreator))
				{
					contentCreatorChartModel.Categories = new List<int> {
					listModel.Where(x=>x.PublishStatus == DocumentPublishWorkflowStatus.Draft).Count(),
					listModel.Where(x=>x.PublishStatus == DocumentPublishWorkflowStatus.Submitted).Count(),
					listModel.Where(x=>x.PublishStatus == DocumentPublishWorkflowStatus.Approved).Count(),
					listModel.Where(x=>x.PublishStatus == DocumentPublishWorkflowStatus.Declined).Count()};
				}
				if (Thread.CurrentPrincipal.IsInRole(Role.ContentApprover))
				{
					contentApproverChartModel.Categories = contentCreatorChartModel.Categories = new List<int> {
					listModel.Where(x=>x.PublishStatus == DocumentPublishWorkflowStatus.Submitted).Count(),
					listModel.Where(x=>x.PublishStatus == DocumentPublishWorkflowStatus.Approved).Count(),
					listModel.Where(x=>x.PublishStatus == DocumentPublishWorkflowStatus.Declined).Count()};
				}

				if (model.DocumentType == DocumentType.Test && model.Certificate != null)
				{
					model.Certificate.ThumbnailUrl =
					Url.ActionLink<UploadController>(a =>
						a.GetThumbnail(model.Certificate.Id, 842, 595));
				}
			}

			ViewData["contentCreatorChart"] = contentCreatorChartModel;

			ViewData["contentApproverChart"] = contentApproverChartModel;

			base.Index_PostProcess(listModel);
		}

		public ActionResult CollaboratorsForDocument(string documentId, DocumentType documentType)
		{
			var collaborators = ExecuteQuery<DocumentCollaboratorsQuery, IEnumerable<UserModelShort>>(
				new DocumentCollaboratorsQuery
				{
					DocumentId = documentId,
					DocumentType = documentType
				});

			return new JsonResult { Data = collaborators };
		}

		[HttpPost]
		public ActionResult UpdateCollaboratorsOfDocument(string documentId, DocumentType documentType, string[] userIds)
		{
			if (ExecuteCommand(new UpdateDocumentCollaboratorsCommand
			{
				DocumentId = documentId,
				DocumentType = documentType,
				UserIds = userIds,
				CurrentUser = Thread.CurrentPrincipal.IsInRole(Role.ContentAdmin) ? Thread.CurrentPrincipal.GetId().ToString() : null
			}).ErrorMessage == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Accepted);
			}

			return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
		}

		[ActionName("MyAchievements")]
		public ActionResult MyCertifiecates()
		{

			var result = ExecuteQuery<TestResultCertificateByUserQuery, List<TestResultModel>>(new TestResultCertificateByUserQuery { UserId = Thread.CurrentPrincipal.GetId().ToString() });

			var query = new PointsStatementQuery { UserIds = new List<string> { Thread.CurrentPrincipal.GetId().ToString() }, CompanyId = PortalContext.Current.UserCompany.Id };

			var GlobalAccess = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
			query.EnableGlobalAccessDocuments = GlobalAccess;
			var vm = ExecuteQuery<PointsStatementQuery, PointsStatementViewModel>(query);

			result.ToList().ForEach(model => {
				model.Certificate.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.Certificate.Id, Thread.CurrentPrincipal.GetCompanyId().ToString()));

				model.Certificate.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetFromCompany(string.IsNullOrEmpty(model.CertificateThumbnailId) ? model.Certificate.Id : model.CertificateThumbnailId, Thread.CurrentPrincipal.GetCompanyId().ToString()));

				//model.Certificate.ThumbnailUrl =
				//	Url.ActionLink<UploadController>(a =>
				//		a.GetThumbnail(string.IsNullOrEmpty(model.CertificateThumbnailId)?model.Certificate.Id: model.CertificateThumbnailId, 842, 595));
			});
			ViewBag.TotalAchieved = result.Count() + "/" + vm.Data.Where(c => c.DocumentType == DocumentType.Test && c.IsCertificate).Count();
			ViewBag.Data = result;
			ViewBag.TotalScore = vm.Data.Where(c => c.Result == PointsStatementResult.Passed).Count() * vm.Data.Where(c => c.Result == PointsStatementResult.Passed).Sum(c => c.Points) + "/" + vm.Data.Count() * vm.Data.Sum(c => c.Points);
			return View("MyCertifiecates", vm.Data.Take(10).ToList());
		}

		[HttpGet]
		public JsonResult GetCertificateList(int startIndex, int pageSize)
		{
			var query = new PointsStatementQuery { UserIds = new List<string> { Thread.CurrentPrincipal.GetId().ToString() }, CompanyId = PortalContext.Current.UserCompany.Id };

			var GlobalAccess = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
			query.EnableGlobalAccessDocuments = GlobalAccess;
			var vm = ExecuteQuery<PointsStatementQuery, PointsStatementViewModel>(query);
			var skipRecords = (startIndex - 1) * pageSize;
			var model = vm.Data.Skip(skipRecords).Take(pageSize).ToList();
			return Json(model, JsonRequestBehavior.AllowGet);

		}

		[OutputCache(NoStore = true, Duration = 0)]

		public ActionResult MyDocuments()
		{
			var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();

			categories.ForEach(x => {
				if (x.parent == "#")
					x.parent = $"{Guid.Empty}";
			});

			(categories as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Category" });

			//var r = Thread.CurrentPrincipal.GetRoles();
			if (Thread.CurrentPrincipal.IsInGlobalAdminRole() || Thread.CurrentPrincipal.IsInResellerRole())
			{
				return RedirectToAction("ViewDocuments", "Admin", new { Area = "" });
			}
			else if (Thread.CurrentPrincipal.IsInAdminRole() && !Thread.CurrentPrincipal.IsInStandardUserRole())
			{
				return RedirectToAction("Index", "Document", new { Area = "" });
			}

			var start = DateTime.Now;

			var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>(new DocumentsAssignedToUserQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).Where(z=>z.AssignedDate<=DateTime.Now).ToList();

			var sec = DateTime.Now.Second - start.Second;

			#region chart
			var status = Enum.GetNames(typeof(AssignedDocumentStatus)).Where(c => c != AssignedDocumentStatus.CheckList.ToString()).ToList();

			var chartModel = new ChartViewModel
			{
				Status = new List<string>(),
				StatusCount = new List<int>()
			};
			foreach (var item in status)
			{

				chartModel.Status.Add(item);
				chartModel.StatusCount.Add(model.Where(c => c.Status == (AssignedDocumentStatus)Enum.Parse(typeof(AssignedDocumentStatus), item)).Count());

			}

			chartModel.Type = new List<string> { DocumentType.TrainingManual.ToString(), DocumentType.Test.ToString(), DocumentType.Policy.ToString(), DocumentType.Memo.ToString(), DocumentType.custom.ToString() };
			chartModel.Count = new List<int> { model.Where(c => c.DocumentType == DocumentType.TrainingManual).Count(), model.Where(c => c.DocumentType == DocumentType.Test).Count(), model.Where(c => c.DocumentType == DocumentType.Policy).Count(), model.Where(c => c.DocumentType == DocumentType.Memo).Count(), model.Where(c => c.DocumentType == DocumentType.custom).Count() };

			if (PortalContext.Current.UserCompany.EnableChecklistDocument)
			{
				//chartModel.Type.Add(DocumentType.Checklist.ToString());
				chartModel.Type.Add("Activity Book");
				chartModel.Count.Add(model.Where(c => c.DocumentType == DocumentType.Checklist).Count());
			}

			ViewData["Chart"] = chartModel;

			#endregion

			var pending = model.Where(c => c.Status == AssignedDocumentStatus.Pending).ToList();
			var incomplete = model.Where(c => c.Status == AssignedDocumentStatus.UnderReview).ToList();
			var other = model.Where(c => c.Status != AssignedDocumentStatus.UnderReview && c.Status != AssignedDocumentStatus.Pending).ToList();
			var result = new List<AssignedDocumentListModel>();
			result.AddRange(pending);
			result.AddRange(incomplete);
			result.AddRange(other);
			model = result;

			#region Filter Categories As per Mydocuments
			var categoryList = new List<JSTreeViewModel>();

			foreach (var data in model)
			{

				var category = categories.Where(c => c.id == data.CategoryId).FirstOrDefault();
				if (category != null)
				{
					categoryList.Add(category);
					if (!string.IsNullOrEmpty(category.parent))
					{
						var checkParent = CheckParentCategory(categoryList, category.parent, categories);
					}
				}
			}

			(categoryList as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Category" });

			ViewBag.Categories = categoryList.GroupBy(c => c.id).Select(f => f.FirstOrDefault());

			#endregion

			#region Added by ashok
			var userId = Thread.CurrentPrincipal.GetId().ToString();
			var testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
			{
				UserId = userId
			});
			ViewBag.CurrentTestId = "Not";
			if (testSession == null)
			{
				ViewBag.CurrentTestId = "Not";
				ViewBag.TestTitle = "No";
			}
			else
			{
				ViewBag.CurrentTestId = testSession.CurrentTestId.ToString();
				ViewBag.IsOpenTest = testSession.OpenTest ? testSession.OpenTest : false;
				ViewBag.TestTitle = testSession.Title;
			}
			#endregion

			model.Where(x => x.CertificateUrl != null).ForEach(x => x.CertificateUrl = Url.ActionLink<UploadController>(a => a.Get(x.CertificateUrl, false)));
			if (!PortalContext.Current.UserCompany.EnableChecklistDocument)
			{
				model = model.Where(c => c.DocumentType != DocumentType.Checklist).ToList();
			}

			Response.Cache.AppendCacheExtension("no-store, must-revalidate");
			return View(model);
		}

		public JSTreeViewModel CheckParentCategory(List<JSTreeViewModel> jSTreeViewModels, string parentId, List<JSTreeViewModel> categories)
		{

			var parentCategory = categories.Where(c => c.id == parentId).FirstOrDefault();
			if (parentCategory == null)
				return null;
			jSTreeViewModels.Add(parentCategory);
			return CheckParentCategory(jSTreeViewModels, parentCategory.parent, categories);//Recursive call    

		}

		public ActionResult GlobalDocuments()
		{
			var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();
			categories.ForEach(x => {
				if (x.parent == "#")
					x.parent = $"{Guid.Empty}";
			});
			(categories as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Category" });

			var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<DocumentListModel>>(new DocumentsAssignedToUserQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).ToList();


			#region Filter Categories As per Mydocuments
			var categoryList = new List<JSTreeViewModel>();
			var catg = model.GroupBy(c => c.CategoryId).Select(f => f.FirstOrDefault()).ToList();

			foreach (var data in catg)
			{
				var category = categories.Where(c => data.CategoryId == c.id).FirstOrDefault();
				if (category != null)
				{
					categoryList.Add(category);
					if (!string.IsNullOrEmpty(category.parent))
					{
						var checkParent = CheckParentCategory(categoryList, category.parent, categories);
					}
				}
			}

			(categoryList as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Category" });
			ViewBag.Categories = categoryList.GroupBy(c => c.id).Select(f => f.FirstOrDefault());
			#endregion
			model = model.OrderBy(c => c.DocumentType).ToList();
			return View(model);
		}

		/// <summary>
		/// this one is used to get the Virtual Classroom list with respect to the login standard user
		/// </summary>
		/// <returns></returns>
		public ActionResult MyVirtualMeetings()
		{

			var model = ExecuteQuery<VirtualClassroomQuery, IEnumerable<VirtualClassModel>>(new VirtualClassroomQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).ToList();

			ViewBag.InprogressMeetings = model.Where(c => c.StartDateTime <= DateTime.Now && c.EndDateTime >= DateTime.Now).ToList();

			var meetingList = GetMeetingList(model);
			return View(meetingList);
		}

		/// <summary>
		/// this is used for search the meeting 
		/// </summary>
		/// <param name="searchText">search meeting based on searchtext</param>
		/// <returns></returns>
		public ActionResult FilterMeeting(string filters, string searchText, int pageIndex, int pageSize, int startPage, int endPage)
		{
			var model = ExecuteQuery<VirtualClassroomQuery, IEnumerable<VirtualClassModel>>(new VirtualClassroomQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString(),
				Filters = filters,
				SearchText = searchText
			}).Where(c => c.VirtualClassRoomName.ToLower().Contains(searchText.ToLower())).ToList();
			var meetingList = GetMeetingList(model, pageIndex, pageSize, startPage, endPage);
			return PartialView("_VirtualClassList", meetingList);
		}

		[NonAction]
		public MeetingViewModel GetMeetingList(List<VirtualClassModel> model, int pageIndex, int pageSize, int startPage, int endPage)
		{
			var meetingList = new MeetingViewModel();
			meetingList.Paginate.TotalItems = model.Count;

			meetingList.Paginate.Page = (model.Count / pageSize) == 0 ? (model.Count / pageSize) : (model.Count / pageSize) + 1;
			meetingList.VirtualClassrooms = model.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

			meetingList.Paginate.PageIndex = pageIndex;
			meetingList.Paginate.IsFirstPage = pageIndex == 1 ? true : false;

			meetingList.Paginate.PageSize = pageSize;
			if (pageIndex > endPage && meetingList.Paginate.Page != pageIndex)
			{
				meetingList.Paginate.StartPage = startPage + 1;
				meetingList.Paginate.EndPage = endPage + 1;
				meetingList.Paginate.IsLastPage = false;
			}
			else if (meetingList.Paginate.Page == pageIndex)
			{
				meetingList.Paginate.StartPage = startPage + 1;
				meetingList.Paginate.EndPage = endPage + 1;
				meetingList.Paginate.IsLastPage = true;
			}
			else if (pageIndex == startPage && pageSize == 1)
			{
				meetingList.Paginate.StartPage = startPage;
				meetingList.Paginate.EndPage = endPage;
			}
			else if (pageIndex < startPage)
			{
				meetingList.Paginate.StartPage = startPage - 1;
				meetingList.Paginate.EndPage = endPage - 1;
			}
			else
			{
				meetingList.Paginate.StartPage = startPage;
				meetingList.Paginate.EndPage = endPage;
				meetingList.Paginate.IsLastPage = false;
				meetingList.Paginate.IsFirstPage = false;
			}
			if (meetingList.Paginate.Page < 7 && meetingList.Paginate.Page > 1)
			{
				meetingList.Paginate.StartPage = meetingList.Paginate.Page >= 1 ? 1 : 1;
				meetingList.Paginate.EndPage = meetingList.Paginate.Page >= 7 ? 7 : meetingList.Paginate.Page;

			}
			else if (meetingList.Paginate.Page == 0 || meetingList.Paginate.Page == 1)
			{
				meetingList.Paginate.StartPage = 1;
				meetingList.Paginate.EndPage = 1;
				meetingList.Paginate.IsLastPage = true;
				meetingList.Paginate.IsFirstPage = true;
			}
			if (meetingList.Paginate.PageIndex == 1)
			{
				meetingList.Paginate.FirstPage = 1;
				var records = meetingList.Paginate.PageIndex * meetingList.Paginate.PageSize;
				if (records <= meetingList.Paginate.TotalItems)
				{
					meetingList.Paginate.LastPage = records;
				}
				else
				{
					meetingList.Paginate.LastPage = meetingList.Paginate.TotalItems;
				}
			}
			else
			{
				var records = meetingList.Paginate.PageIndex * meetingList.Paginate.PageSize;
				meetingList.Paginate.FirstPage = ((meetingList.Paginate.PageIndex - 1) * meetingList.Paginate.PageSize) + 1;
				if (records <= meetingList.Paginate.TotalItems)
				{
					meetingList.Paginate.LastPage = records;
				}
				else
				{
					meetingList.Paginate.LastPage = meetingList.Paginate.TotalItems;
				}
			}
			return meetingList;
		}

		[NonAction]
		public MeetingViewModel GetMeetingList(List<VirtualClassModel> model)
		{
			var meetingList = new MeetingViewModel();
			meetingList.Paginate.IsFirstPage = true;
			meetingList.Paginate.IsLastPage = false;

			meetingList.Paginate.Page = (model.Count / meetingList.Paginate.PageSize) < 0 ? (model.Count / meetingList.Paginate.PageSize) : (model.Count / meetingList.Paginate.PageSize) + 1;
			meetingList.Paginate.TotalItems = model.Count;
			meetingList.VirtualClassrooms = model.Skip((meetingList.Paginate.PageIndex - 1) * meetingList.Paginate.PageSize).Take(meetingList.Paginate.PageSize).ToList();

			meetingList.Paginate.StartPage = meetingList.Paginate.Page >= 1 ? 1 : 0;
			meetingList.Paginate.EndPage = meetingList.Paginate.Page >= 7 ? 7 : meetingList.Paginate.Page;
			if (meetingList.Paginate.PageIndex == 1)
			{
				meetingList.Paginate.FirstPage = 1;
				var records = meetingList.Paginate.PageIndex * meetingList.Paginate.PageSize;
				if (records <= meetingList.Paginate.TotalItems)
				{
					meetingList.Paginate.LastPage = records;
				}
				else
				{
					meetingList.Paginate.LastPage = meetingList.Paginate.TotalItems;
				}
			}
			return meetingList;
		}

		public JsonResult GlobalDocumentsByCategory(string categoryId)
		{
			var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<DocumentListModel>>(new DocumentsAssignedToUserQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).Where(c => c.DocumentType != DocumentType.VirtualClassRoom).ToList();
			if (categoryId == "00000000-0000-0000-0000-000000000000")
			{
				return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
			var documents = model.Where(c => c.CategoryId == categoryId && c.Deleted != true);
			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public JsonResult GlobalDocumentsByFilter(string searchText)
		{
			var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<DocumentListModel>>(new DocumentsAssignedToUserQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).Where(c => c.DocumentType != DocumentType.VirtualClassRoom).ToList();
			var filter = searchText.ToLower();
			var documents = model.Where(c => c.Title.ToLower().Contains(filter) || c.TrainingLabels.ToLower().Contains(filter) && c.Deleted != true);
			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public ActionResult GetCategoriesChart()
		{

			var memoList = ExecuteQuery<MemoListQuery, IEnumerable<DocumentListModel>>(new MemoListQuery()).Where(c => !c.Deleted);
			var manualList = ExecuteQuery<TrainingManualListQuery, IEnumerable<DocumentListModel>>(new TrainingManualListQuery()).Where(c => !c.Deleted);
			var testList = ExecuteQuery<TestListQuery, IEnumerable<DocumentListModel>>(new TestListQuery()).Where(c => !c.Deleted);
			var policyList = ExecuteQuery<PolicyListQuery, IEnumerable<DocumentListModel>>(new PolicyListQuery()).Where(c => !c.Deleted);
			var checklistList = ExecuteQuery<CheckListQuery, IEnumerable<DocumentListModel>>(new CheckListQuery()).Where(c => !c.Deleted);

			var assignedDocs = ExecuteQuery<FetchByIdQuery, List<AssignedDocumentModel>>(new FetchByIdQuery());

			var chartModel = new ChartViewModel
			{
				Categories = new List<int> { assignedDocs.Where(c => c.DocumentType == DocumentType.TrainingManual).Count(), assignedDocs.Where(c => c.DocumentType == DocumentType.Test).Count(), assignedDocs.Where(c => c.DocumentType == DocumentType.Policy).Count(), assignedDocs.Where(c => c.DocumentType == DocumentType.Memo).Count(), assignedDocs.Where(c => c.DocumentType == DocumentType.Checklist).Count() },
				Name = new List<string> { DocumentType.TrainingManual.ToString(), DocumentType.Test.ToString(), DocumentType.Policy.ToString(), DocumentType.Memo.ToString(), DocumentType.Checklist.ToString() }
			};

			chartModel.Type = new List<string> { DocumentType.TrainingManual.ToString(), DocumentType.Test.ToString(), DocumentType.Policy.ToString(), DocumentType.Memo.ToString(), DocumentType.Checklist.ToString() };
			chartModel.Count = new List<int> { manualList.Count(), testList.Count(), policyList.Count(), memoList.Count(), checklistList.Count() };

			var draftCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count() + checklistList.Where(c => c.DocumentStatus == DocumentStatus.Draft).Count();
			var publishedCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count() + checklistList.Where(c => c.DocumentStatus == DocumentStatus.Published).Count();
			var recalledCount = manualList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + testList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + policyList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + memoList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count() + checklistList.Where(c => c.DocumentStatus == DocumentStatus.Recalled).Count();

			chartModel.Status = new List<string> { DocumentStatus.Draft.ToString(), DocumentStatus.Published.ToString(), DocumentStatus.Recalled.ToString() };
			chartModel.StatusCount = new List<int> { draftCount, publishedCount, recalledCount };

			return PartialView("_CatergoriesChart", chartModel);
		}

		public JsonResult DocumentsByCategory(string categoryId)
		{
			var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>(new DocumentsAssignedToUserQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).Where(c => c.DocumentType != DocumentType.VirtualClassRoom).ToList();
			if (categoryId == "00000000-0000-0000-0000-000000000000")
			{
				return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
			var documents = model.Where(c => c.CategoryId == categoryId);
			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}
		public JsonResult DocumentsByFilter(string searchText)
		{
			var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>(new DocumentsAssignedToUserQuery()
			{
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
			}).Where(c => c.DocumentType != DocumentType.VirtualClassRoom).ToList();
			var filter = searchText.ToLower();
			var documents = model.Where(c => c.Title.ToLower().Contains(filter) || c.TrainingLabels.Contains(filter));
			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public ActionResult Feedback(string documentId, DocumentType documentType)
		{
			var document = ExecuteQuery<DocumentQuery, DocumentListModel>(new DocumentQuery
			{
				Id = documentId,
				DocumentType = documentType
			});

			if (document != null)
			{
				ViewBag.Description = $"Feedback for {VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(document.DocumentType)}: {document.Title}";

				var model = ExecuteQuery<UserFeedbackListQuery, IEnumerable<UserFeedbackListModel>>(
					new UserFeedbackListQuery
					{
						DocumentId = documentId,
						DocumentType = documentType
					});
				return View(model);
			}

			return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
		}

		public JsonResult DocumentsByType(DocumentType[] documentTypes)
		{
			if (documentTypes == null)
			{
				return new JsonResult { Data = new object[] { new object() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}

			var documentFilters = new List<string>
			{
				"Status:Published",
				"Status:Recalled"
			};

			foreach (var type in documentTypes)
			{
				documentFilters.Add($"Type:{type.ToString()}");
			}

			var documents = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery
			{
				DocumentFilters = documentFilters,
				EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument
			});

			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public JsonResult DocumentFeedback(string documentId, string creatorId)
		{
			var feedbacks = ExecuteQuery<FetchAllQuery, List<DocumentWorkflowAuditMessagesViewModel>>(
				new FetchAllQuery
				{
					Id = documentId,
					Filters = creatorId
				});

			return new JsonResult { Data = feedbacks, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

	}
}