using Common;
using Common.Query;
using Data.EF;
using Data.EF.Customer;
using Domain.Models;
using Newtonsoft.Json;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.Feedback;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.Query.TrainingGuide;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.CommandHandler.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Ramp.Contracts.Query.Document;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;
using RampController = Web.UI.Controllers.RampController;
using Role = Ramp.Contracts.Security.Role;

namespace Web.UI.Areas.ManageTrainingGuides.Controllers
{
    public class ManageTrainingGuidesController : RampController
    {
        private readonly CustomerContext _customerContext = new CustomerContext();
        private readonly MainContext _mainContext = new MainContext();
        private readonly JsTreeModel _rootTree = new JsTreeModel();
        private List<CategoryViewModel> _categoryList;
        private readonly List<CategoryViewModel> _categoryListReport = new List<CategoryViewModel>();

        private List<TrainingGuideViewModel> FeedbackUpdate(List<TrainingGuideViewModel> list)
        {
            var userId = SessionManager.GetCurrentlyLoggedInUserId();
            foreach (var item in list)
            {
                item.Feedback = ExecuteQuery<GetFeedbackForPlaybookQueryParameter, List<FeedbackViewModel>>(new GetFeedbackForPlaybookQueryParameter()
                {
                    ReferenceId = item.ReferenceId
                });
                item.UnreadFeedback = item.Feedback != null
                                        ? item.Feedback.Sum(s => s.Reads.Any(a => a.UserId == userId) ? 0 : 1)
                                        : 0;
            }
            return list;
        }

        public ActionResult Index()
        {
            var query = new AllTrainingGuideQueryParameter();
            if (Thread.CurrentPrincipal.IsUserInRole(Role.ContentAdmin))
                query.CollaboratorId = SessionManager.GetCurrentlyLoggedInUserId();
            var trainingGuideViewModelList = ExecuteQuery<AllTrainingGuideQueryParameter, List<TrainingGuideViewModel>>(query);

            var result = new TrainingGuideViewModelLong { TraningGuideViewModelList = trainingGuideViewModelList };
            var resultAddChapter =
                ExecuteQuery<TrainingGuideBarometerQueryParameter, TrainingGuideViewModelLong>(
                    new TrainingGuideBarometerQueryParameter
                    {
                        UserId = SessionManager.GetCurrentlyLoggedInUserId()
                    });

            trainingGuideViewModelList = FeedbackUpdate(trainingGuideViewModelList);

            result.MaxGuide = resultAddChapter.MaxGuide;
            int maxGuide = resultAddChapter.MaxGuide;
            if (result.TraningGuideViewModelList.Count == maxGuide)
            {
                TempData["NewGuideCreate"] = "no";
            }
            ViewBag.Usage = ExecuteQuery<TotalCreatedTrainingGuidesInSystemQuery, AllTrainingGuidesInSystemViewModel>(
                                                         new TotalCreatedTrainingGuidesInSystemQuery()).Count;
            ViewBag.Total = maxGuide;
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
                        DateTime CreatedDate = company.CreatedOn;
                        DateTime addYear = CreatedDate.AddMonths(1);
                        DateTime ExpireDate = addYear.AddDays(1);
                        ViewBag.ExpiryDate = ExpireDate.ToString("dd-MMMM-yyyy");
                        ViewBag.ProvisionalCompany = provisionalCompany.CompanyName;
                        ViewBag.ProvCompanyCntNo = provisionalCompany.TelephoneNumber;
                    }
                    if (company.YearlySubscription != null)
                    {
                        if (company.YearlySubscription == true)
                        {
                            //  not self provision company
                            DateTime CreatedDate = company.CreatedOn;
                            DateTime addYear = CreatedDate.AddYears(1);
                            DateTime ExpireDate = addYear.AddDays(1);
                            ViewBag.ExpiryDate = ExpireDate.ToString("dd-MMMM-yyyy");
                            ViewBag.ProvisionalCompany = provisionalCompany.CompanyName;
                            ViewBag.ProvCompanyCntNo = provisionalCompany.TelephoneNumber;
                        }
                        else
                        {
                            DateTime CreatedDate = company.CreatedOn;
                            DateTime addYear = CreatedDate.AddMonths(1);
                            DateTime ExpireDate = addYear.AddDays(1);
                            ViewBag.ExpiryDate = ExpireDate.ToString("dd-MMMM-yyyy");
                            ViewBag.ProvisionalCompany = provisionalCompany.CompanyName;
                            ViewBag.ProvCompanyCntNo = provisionalCompany.TelephoneNumber;
                        }
                    }
                }
            }
            var allCollaboratorForCompany =
                ExecuteQuery<AllContentAdminsForCustomerCompanyQuery, List<UserViewModel>>(
                    new AllContentAdminsForCustomerCompanyQuery());

            result.AllCollaborators = allCollaboratorForCompany;
            var allCategories =
                ExecuteQuery<AllCategoriesQueryParameter, List<CategoryViewModel>>(new AllCategoriesQueryParameter
                {
                    CompanyId = PortalContext.Current.UserCompany.Id
                });
            if (allCategories.Count == 0)
            {
                ViewBag.CategoryError = true;
            }
            return View(result);
        }

        [HttpPost]
        public JsonResult AddUsersToCollaborationList(AddUserToColaboratorsCommand command)
        {
            if (!command.UserViewModelList.Any(c => c.Id == Thread.CurrentPrincipal.GetId()) && Thread.CurrentPrincipal.IsUserInRole(Role.ContentAdmin))
                command.UserViewModelList.Add(new UserViewModel { Id = Thread.CurrentPrincipal.GetId() });
            var response = ExecuteCommand(command);
            if (response.Validation.Any())
            {
                return Json(new { Success = "False" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = "True" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ChangeTrainingGuideStatus(Guid trainingGuidId, string status)
        {
            var updateTrainingGuideStatus = new UpdateTrainingGuideStatusCommand
            {
                TrainingGuidId = trainingGuidId,
                IsActive = status == "true"
            };
            ExecuteCommand(updateTrainingGuideStatus);
            NotifyInfo("Status successfully changed");

            return null;
        }

        public ActionResult Details(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var trainingguide = _customerContext.TrainingGuideSet.Find(id);
            if (trainingguide == null)
                return HttpNotFound();

            return View(trainingguide);
        }

        public ActionResult AssignTrainingGuideToUsersOrGroups()
        {
            var model = ExecuteQuery<AssignTrainingGuideQuery, AssignTrainingGuideToUsersOrGroupsViewModel>(new AssignTrainingGuideQuery());
            return View(model);
        }

        [HttpGet]
        public ActionResult Assign_UnAssignPlaybooks(FunctionalMode? Mode)
        {
            var model = ExecuteQuery<ManageableUsersForCompanyQueryParameter, Assign_UnAssignPlaybooksAndTests>(new ManageableUsersForCompanyQueryParameter
            {
                FunctionalMode = Mode.HasValue ? Mode.Value : FunctionalMode.Playbooks
            });
            model.FunctionalMode = Mode.HasValue ? Mode.Value : FunctionalMode.Playbooks;
            return View(model);
        }

        [HttpPost]
        public ActionResult Assign_UnAssignPlaybooks(ManageableUsersForCompanyQueryParameter query)
        {
            var model = ExecuteQuery<ManageableUsersForCompanyQueryParameter, Assign_UnAssignPlaybooksAndTests>(query);
            return new JsonResult() { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult Assign_Unassign(Assign_UnassignCommand command)
        {
            command.CurrentlUserId = SessionManager.GetCurrentlyLoggedInUserId();
            var result = ExecuteCommand(command);
            if (result.Validation.Any())
                return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult() { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult Assign_UnAssignPlaybookToUser(Assign_UnAssignPlaybookToUserCommand command)
        {
            command.CurrentlyLoggedInUser = SessionManager.GetCurrentlyLoggedInUserId();
            var result = ExecuteCommand(command);
            if (result.Validation.Any())
                return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult() { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult GetUserInformation(UserFilterOnAssignPlaybooksViewModel filterModel)
        {
            List<object> response = new List<object>();
            Dictionary<Guid, List<Dictionary<Guid, bool>>> usersInformation = new Dictionary<Guid, List<Dictionary<Guid, bool>>>();

            var queryCompanyGroups = new Ramp.Contracts.QueryParameter.Group.AllGroupsByCustomerAdminQueryParameter()
            {
                CompanyId = PortalContext.Current.UserCompany.Id
            };
            var companyGroups = ExecuteQuery<Ramp.Contracts.QueryParameter.Group.AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(queryCompanyGroups);

            var queryAllCompanyUsers = new Ramp.Contracts.QueryParameter.CustomerManagement.AllStandardUserQueryParameter()
            {
                CompanyId = PortalContext.Current.UserCompany.Id
            };
            var allCompanyUsers = ExecuteQuery<Ramp.Contracts.QueryParameter.CustomerManagement.AllStandardUserQueryParameter, List<UserViewModel>>(queryAllCompanyUsers);

            if (filterModel.UserIds != null && filterModel.UserIds.Count > 0)
            {
                foreach (var userId in filterModel.UserIds)
                {
                    usersInformation.Add(userId, new List<Dictionary<Guid, bool>>());

                    var queryAssignedTrainingGuides = new AllAssignedGuidesToStandardUserQueryParameter()
                    {
                        UserId = userId
                    };
                    var allAssignedTrainingGuidesForUser = ExecuteQuery<AllAssignedGuidesToStandardUserQueryParameter, List<TrainingGuideViewModel>>(queryAssignedTrainingGuides);
                    var GuideAssigned_GroupAssigned = true;
                    if (allAssignedTrainingGuidesForUser != null && allAssignedTrainingGuidesForUser.Count > 0)
                    {
                        foreach (var guide in allAssignedTrainingGuidesForUser)
                        {
                            var temp = new Dictionary<Guid, bool>();
                            temp.Add(guide.TrainingGuidId, GuideAssigned_GroupAssigned);
                            usersInformation[userId].Add(temp);
                        }
                    }
                    if (allCompanyUsers != null)
                    {
                        var user = allCompanyUsers.SingleOrDefault(u => u.Id.Equals(userId));
                        if (user != null)
                        {
                            if (companyGroups != null && companyGroups.Count > 0)
                            {
                                var userGroup = companyGroups.SingleOrDefault(group => group.GroupId.Equals(user.SelectedGroupId));
                                if (userGroup != null)
                                {
                                    var temp = new Dictionary<Guid, bool>();
                                    temp.Add(userGroup.GroupId, !GuideAssigned_GroupAssigned);
                                    usersInformation[userId].Add(temp);
                                }
                            }
                        }
                    }
                }
            }
            if (usersInformation.Count > 0)
            {
                foreach (var userI in usersInformation.Keys)
                {
                    object[] item = new object[3];
                    if (usersInformation[userI].Count > 0)
                    {
                        foreach (var entry in usersInformation[userI])
                        {
                            item[0] = new { userId = userI.ToString() };
                            foreach (var article in entry.Keys)
                            {
                                item[1] = new { itemId = article.ToString() };
                                item[2] = new { GuideAssigned_GroupAssigned = entry[article] };
                                response.Add((object)item.ToArray());
                            }
                        }
                    }
                }
                return Json((object)response.ToArray(), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        [HttpPost]
        public JsonResult AssignTrainingGuideToStandardUserOrGroup(AssignTrainingGuideToStandardUsersViewModel model)
        {
            ExecuteCommand(new AssignTrainingGuideToUsersCommand
            {
                AssignedBy = SessionManager.GetCurrentlyLoggedInUserId(),
                AssignTrainingGuideToStandardUsersViewModel = model,
            });
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MyTrainingGuides(Guid userId)
        {
            var model =
                ExecuteQuery<AllAssignedGuidesToStandardUserQueryParameter, List<TrainingGuideViewModel>>(
                    new AllAssignedGuidesToStandardUserQueryParameter
                    {
                        UserId = SessionManager.GetCurrentlyLoggedInUserId()
                    });

            return View(model);
        }

        public ActionResult PreviewByReferenceId(string id)
        {
            var view = "PreviewByReferenceId_K";
            ViewBag.Action = Url.Action("SaveFeedback", "Feedback").Replace("/ManageTrainingGuides", string.Empty);
            var trainingGuide = ExecuteQuery<GetTrainingGuideByReferenceIdQueryParameter,
                TrainingGuideViewModel>(new GetTrainingGuideByReferenceIdQueryParameter
                {
                    RefrenceId = id,
                    UploadDeleteUrlBase = Url.ActionLink<UploadController>(a => a.Delete(Guid.Empty.ToString(),null)).Replace("/ManageTrainingGuides", string.Empty),
                    UploadUrlBase = Url.ActionLink<UploadController>(a => a.Get(Guid.Empty.ToString(),false)).Replace("/ManageTrainingGuides", string.Empty),
                    UploadThumbnailUrlBase = Url.ActionLink<UploadController>(a => a.GetThumbnail(Guid.Empty.ToString(), 115, 115)).Replace("/ManageTrainingGuides", string.Empty),
                    CoverPictureThumbnailUrlBase = Url.ActionLink<UploadController>(a => a.GetThumbnail(Guid.Empty.ToString(), 300, 300)).Replace("/ManageTrainingGuides", string.Empty),
                    CompanyId = PortalContext.Current.UserCompany.Id,
                    UploadPreviewUrlBase = Url.ActionLink<UploadController>(a => a.Preview(Guid.Empty.ToString(), null)).Replace("/ManageTrainingGuides", string.Empty),
                    CurrentlyLoggedInUserId = Thread.CurrentPrincipal.IsInStandardUserRole() ? Thread.CurrentPrincipal.GetId() : new Guid?()
                });
            if (trainingGuide == null)
            {
                return new HttpNotFoundResult();
            }
            //trainingGuide.TraningGuideChapters.SelectMany(x => x.Attachments).Where(x => x.Type.Contains("video")).ToList().ForEach(delegate (FileUploadResultViewModel model)
            //{
            //    var file = ExecuteQuery<Ramp.Contracts.QueryParameter.Upload.FetchUploadQueryParameter, FileUploadViewModel>(new Ramp.Contracts.QueryParameter.Upload.FetchUploadQueryParameter
            //    {
            //        Id = model.Id
            //    });
            //    var videoPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.{System.IO.Path.GetExtension(model.Name)}");
            //    var stream = System.IO.File.Open(videoPath, System.IO.FileMode.OpenOrCreate);
            //    stream.Write(file.Data, 0, file.Data.Length);
            //    stream.Close();

            //    var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            //    ffMpeg.GetVideoThumbnail(videoPath, System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.jpg"),7);
                
            //});
            if (Thread.CurrentPrincipal.IsInGlobalAdminRole() || Thread.CurrentPrincipal.IsInResellerRole())
            {
                if (Thread.CurrentPrincipal.IsInResellerRole())
                {
                    ExecuteCommand(new AddUserActivityCommand
                    {
                        ActivityDescription = trainingGuide.Title.Trim(),
                        ActivityType = UserActivityEnum.ViewTrainingGuide,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    });
                }
                return View(view, trainingGuide);
            }
            else
            {
                ExecuteCommand(new AddUserActivityCommand
                {
                    ActivityDescription = trainingGuide.Title.Trim(),
                    ActivityType = UserActivityEnum.ViewTrainingGuide,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                });
                if (Thread.CurrentPrincipal.IsInStandardUserRole())
                {
                    Guid userId = SessionManager.GetCurrentlyLoggedInUserId();
                    var incrementTrainingGuidePreviewCount = new IncrementTrainingGuideViewCommand
                    {
                        TrainingGuidId = trainingGuide.TrainingGuidId,
                        UserId = userId,
                    };
                    ExecuteCommand(incrementTrainingGuidePreviewCount);
                }
                return View(view, trainingGuide);
            }
        }
        public ActionResult GetAllAdminCustomerCompanyTrainingGuide()
        {
            var companyModel = new CompanyViewModel();
            if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                companyModel =
                    ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(new AllCustomerUserQueryParameter
                    {
                        LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList(),
                        LoginUserCompanyId = PortalContext.Current.UserCompany.Id
                    });
            }
            else
            {
                companyModel =
                    ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(new AllCustomerUserQueryParameter
                    {
                        LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList()
                    });
            }

            companyModel.Companies =
                companyModel.CompanyList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            return View(companyModel);
        }

        public JsonResult TrainingGuideUsageReportView(String companyId)
        {
            try
            {
                string guideId = companyId;
                string[] idSplit = guideId.Split('_');
                Guid customerCompanyId = Guid.Parse(idSplit[0]);
                string fromDate = idSplit[1];
                string lastDate = idSplit[2];

                var trainingGuideUsageReport = ExecuteQuery<GetAllTrainingGuideUsageReportParameter, TrainingGuideusageStatsViewModel>(new GetAllTrainingGuideUsageReportParameter
                {
                    CompanyId = customerCompanyId,
                    FromDate = Convert.ToDateTime(fromDate),
                    LastDate = Convert.ToDateTime(lastDate)
                });

                return Json(new { GuideUserUsesList = trainingGuideUsageReport.TrainingGuidUsageList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TrainingGuideUsageReportThreshold()
        {
            var companyModel = new CompanyViewModel();

            if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                companyModel = ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(new AllCustomerUserQueryParameter
                {
                    LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList(),
                    LoginUserCompanyId = PortalContext.Current.UserCompany.Id
                });
            }
            else
            {
                companyModel =
              ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(new AllCustomerUserQueryParameter
              {
                  LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList()
              });
            }

            companyModel.Companies =
                companyModel.CompanyList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
            return View(companyModel);
        }

        public JsonResult TrainingGuideUsageReportViewThreshold(String companyId)
        {
            try
            {
                Guid customerCompanyId = Guid.Parse(companyId);
                var trainingGuideUsageReport = ExecuteQuery<GetAllTrainingGuideUsageReportThresholdParameter, TrainingGuideusageStatsViewModel>(new GetAllTrainingGuideUsageReportThresholdParameter
                {
                    CompanyId = customerCompanyId,
                });

                return Json(new { GuideUserUsesList = trainingGuideUsageReport.TrainingGuidUsageList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Feedback(string id)
        {
            var trainingGuide = ExecuteQuery<GetTrainingGuideByReferenceIdQueryParameter,
                TrainingGuideViewModel>(new GetTrainingGuideByReferenceIdQueryParameter
                {
                    RefrenceId = id,
                    UploadDeleteUrlBase = Url.ActionLink<UploadController>(a => a.Delete(Guid.Empty.ToString(),null)).Replace("/ManageTrainingGuides", string.Empty),
                    UploadUrlBase = Url.ActionLink<UploadController>(a => a.Get(Guid.Empty.ToString(),false)).Replace("/ManageTrainingGuides", string.Empty),
                    UploadThumbnailUrlBase = Url.ActionLink<UploadController>(a => a.GetThumbnail(Guid.Empty.ToString(), 300, 300)).Replace("/ManageTrainingGuides", string.Empty),
                    CompanyId = PortalContext.Current.UserCompany.Id,
                    UploadPreviewUrlBase = Url.ActionLink<UploadController>(a => a.Preview(Guid.Empty.ToString(), null)).Replace("/ManageTrainingGuides", string.Empty)
                });

            ExecuteCommand(new ReadPlaybookFeedbackCommandParameter()
            {
                ReferenceId = trainingGuide.ReferenceId,
                UserId = SessionManager.GetCurrentlyLoggedInUserId()
            });

            ViewBag.Description = "Feedback for Playbook: '" + trainingGuide.Title + "'";

            return View(trainingGuide.Feedback.ToList());
        }

        public ActionResult Create()
        {
            var trainingGuide = new TrainingGuideViewModel();
            var allCategories =
                ExecuteQuery<AllCategoriesQueryParameter, List<CategoryViewModel>>(new AllCategoriesQueryParameter
                {
                    CompanyId = PortalContext.Current.UserCompany.Id
                });
            if (allCategories.Count == 0)
            {
                return RedirectToAction("Index", "ManageTrainingGuides");
            }
            allCategories.ForEach(c => trainingGuide.TrainingGuideCategoryDropDown.Add(new JSTreeViewModel
            {
                text = c.CategorieTitle.Length > 100 ? c.CategorieTitle.Substring(0, 100) : c.CategorieTitle,
                id = c.Id.ToString(),
                parent = c.ParentCategoryId.HasValue ? c.ParentCategoryId.Value.ToString() : "#"
            }));
            var resultAll = ExecuteQuery<AllTrainingGuideQueryParameter, List<TrainingGuideViewModel>>(new AllTrainingGuideQueryParameter());
            trainingGuide.TraningGuideDropDownForLinking = resultAll.Select(c => new SerializableSelectListItem
            {
                Text = c.Title,
                Value = c.TrainingGuidId.ToString()
            });
            ViewBag.PageTitle = "Create Playbook";
            var models = new object[]
            {
                new { trainingGuideChapter =  new TraningGuideChapterViewModel()},
                new { fileUploadResult = new FileUploadResultViewModel()},
                new { chapterLink = new ChapterLinkViewModel()}
            };
            ViewBag.Models = models;
            ViewBag.Action = Url.Action("Create");
            return View(trainingGuide);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(TrainingGuideViewModel model)
        {
            var id = Guid.NewGuid();
            var response = ExecuteCommand(new SaveTrainingGuideCommand
            {
                CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                TrainingGuide = model,
                NewTrainingGuideId = id
            });
            if (response.Validation.Any())
            {
                return new JsonResult() { Data = new { Created = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                var addActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription = SaveGuideAndChaptersCommandHandler.DefaultPlaybookTitle,
                    ActivityType = UserActivityEnum.CreateTrainingGuide,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };
                ExecuteCommand(addActivityCommand);
            }
            return new JsonResult() { Data = new { Created = true, Id = id }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult EditTrainingGuide(Guid? trainingGuideId)
        {
            if (trainingGuideId.HasValue)
            {
                var model = ExecuteQuery<TrainingGuideQueryParameter, TrainingGuideViewModel>(new TrainingGuideQueryParameter
                {
                    Id = trainingGuideId.Value,
                    UploadDeleteUrlBase = Url.ActionLink<UploadController>(a => a.Delete(Guid.Empty.ToString(),null)).Replace("/ManageTrainingGuides", string.Empty),
                    UploadUrlBase = Url.ActionLink<UploadController>(a => a.Get(Guid.Empty.ToString(),false)).Replace("/ManageTrainingGuides", string.Empty),
                    UploadThumbnailUrlBase = Url.ActionLink<UploadController>(a => a.GetThumbnail(Guid.Empty.ToString(), 300, 300)).Replace("/ManageTrainingGuides", string.Empty),
                    CoverPictureThumbnailUrlBase = Url.ActionLink<UploadController>(a => a.GetThumbnail(Guid.Empty.ToString(), 300, 300)).Replace("/ManageTrainingGuides", string.Empty),
                    UploadPreviewUrlBase = Url.ActionLink<UploadController>(a => a.Preview(Guid.Empty.ToString(), null)).Replace("/ManageTrainingGuides", string.Empty),
                    CompanyId = PortalContext.Current.UserCompany.Id
                });
                if (model == null)
                {
                    return new HttpNotFoundResult();
                }

                ViewBag.PageTitle = "Update Playbook";
                var models = new object[]
                {
                new { trainingGuideChapter =  new TraningGuideChapterViewModel()},
                new { fileUploadResult = new FileUploadResultViewModel()},
                new { chapterLink = new ChapterLinkViewModel()}
                };
                ViewBag.Models = models;
                ViewBag.Action = Url.Action("EditTrainingGuide");
                return View("Create", model);
            }
            return new HttpNotFoundResult();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditTrainingGuide(TrainingGuideViewModel trainingGuideViewModel)
        {
            if (trainingGuideViewModel != null)
            {
                var command = ExecuteCommand(new UpdateTrainingGuideCommand
                {
                    CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    TrainingGuide = trainingGuideViewModel
                });
                if (command.Validation.Any())
                {
                    return new JsonResult() { Data = new { Created = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                {
                    var addActivityCommand = new AddUserActivityCommand
                    {
                        ActivityDescription = trainingGuideViewModel.Title.Trim(),
                        ActivityType = UserActivityEnum.EditTrainingGuide,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    };
                    ExecuteCommand(addActivityCommand);
                }
                return new JsonResult() { Data = new { Created = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new JsonResult() { Data = new { Created = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult Delete(Guid id)
        {
            var deleteTrainingTestCommand = new DeleteTrainingGuideCommandParameter
            {
                TrainingGuidId = id
            };
            ExecuteCommand(deleteTrainingTestCommand);
            if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                //Insert User Activity for Delete Training Guide
                var addActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription = "Deleted Playbook",
                    ActivityType = UserActivityEnum.DeleteTrainingGuide,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };

                ExecuteCommand(addActivityCommand);
            }
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public HttpStatusCodeResult Duplicate(Guid id)
        {
            var command = ExecuteCommand(new DuplicateTrainingGuideCommand { Id = id ,CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()});
            if (!command.Validation.Any())
                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            else
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetCategories()
        {
            var result = ExecuteQuery<CategoriesQueryParameter, CategoryViewModelLong>(new CategoriesQueryParameter
            {
                CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            });

            _categoryList = result.CategoryViewModelList.OrderBy(o => o.CategorieTitle).ToList();
            _rootTree.data = "Category";
            _rootTree.attr.id = Guid.Empty;
            PopulateTree(_rootTree, null);
            var model = _rootTree;

            return PartialView("_CategoryMenu", model);
        }

        public void PopulateTree(JsTreeModel parentNode, Guid? parentId)
        {
            List<CategoryViewModel> children = _categoryList.Where(c => c.ParentCategoryId == parentId).ToList();

            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    JsTreeModel node = new JsTreeModel
                    {
                        attr =
                        {
                            id = child.Id
                        },
                        data = (child.CategorieTitle.Length > 50) ? child.CategorieTitle.Substring(0, 50) + "..." : child.CategorieTitle
                    };
                    parentNode.children.Add(node);
                    PopulateTree(node, child.Id);
                }
            }
        }

        [HttpGet]
        public ActionResult MyPlaybookCategories()
        {
            var result = ExecuteQuery<MyPlaybookCategoryQueryParameter, CategoryViewModelLong>(new MyPlaybookCategoryQueryParameter
            {
                UserId = SessionManager.GetCurrentlyLoggedInUserId()
            });

            _categoryList = result.CategoryViewModelList;
            _rootTree.data = "All Categories";
            _rootTree.attr.id = Guid.Empty;
            PopulateTree(_rootTree, null);
            var model = _rootTree;

            string jsonModel = new JavaScriptSerializer().Serialize(model);

            return PartialView("_MyPlaybookCategory", jsonModel);
        }
        public void PopulateTreeReport(JsTreeModel parentNode, Guid? parentId)
        {
            List<CategoryViewModel> children = _categoryList.Where(c => c.ParentCategoryId == parentId).ToList();

            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    JsTreeModel node = new JsTreeModel
                    {
                        attr =
                        {
                            id = child.Id
                        },
                        data = child.CategorieTitle
                    };
                    parentNode.children.Add(node);
                    _categoryListReport.Add(child);
                    PopulateTreeReport(node, child.Id);
                }
            }
        }
        [HttpPost]
        public ActionResult MyTrainingGuidesWithCategory(Guid catId)
        {
            var result1 = ExecuteQuery<CategoriesQueryParameter, CategoryViewModelLong>(new CategoriesQueryParameter
            {
                Id = catId,
                CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            });

            _categoryList = result1.CategoryViewModelList;
            var category = _categoryList.FirstOrDefault(c => c.Id == catId);
            _categoryListReport.Add(category);

            var parameter = new AllAssignedGuidesToStandardUserWithCategoryQueryParameter();
            PopulateTreeReport(_rootTree, catId);
            parameter.CatList = _categoryListReport;
            parameter.UserId = SessionManager.GetCurrentlyLoggedInUserId();
            parameter.CatId = catId;
            var model = ExecuteQuery<AllAssignedGuidesToStandardUserWithCategoryQueryParameter, List<TrainingGuideViewModel>>(parameter);

            return PartialView("_GetMyPlaybook", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetUpload(Guid id,Guid? companyId = null)
        {
            return RedirectPermanent(Url.ActionLink<UploadController>(a => a.Get(id.ToString(),false)));
        }
    }
}