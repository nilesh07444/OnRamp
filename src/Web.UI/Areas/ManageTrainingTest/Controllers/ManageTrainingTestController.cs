using Common.Events;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.Feedback;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Events.TestManagement;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.QueryParameter.FileUploads;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.Reporting;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;
using Role = Ramp.Contracts.Security.Role;

namespace Web.UI.Areas.ManageTrainingTest.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class ManageTrainingTestController : RampController
    {
        private List<TrainingTestViewModel> FeedbackUpdate(List<TrainingTestViewModel> list)
        {
            var userId = SessionManager.GetCurrentlyLoggedInUserId();
            foreach (var item in list)
            {
                item.Feedback = ExecuteQuery<GetFeedbackForTestQueryParameter, List<FeedbackViewModel>>(new GetFeedbackForTestQueryParameter()
                {
                    ReferenceId = item.LastPublishedVersionId?.ToString()
                });
                item.UnreadFeedback = item.Feedback != null
                                        ? item.Feedback.Sum(s => s.Reads.Any(a => a.UserId == userId) ? 0 : 1)
                                        : 0;
            }
            return list;
        }

        public ActionResult Feedback(string referenceId)
        {
            var trainingTest = ExecuteQuery<GetTrainingTestByReferenceIdQueryParameter,
                TrainingTestViewModel>(new GetTrainingTestByReferenceIdQueryParameter
                {
                    ReferenceId = referenceId
                }
            );

            ExecuteCommand(new ReadTestFeedbackCommandParameter()
            {
                ReferenceId = trainingTest.TrainingTestId.ToString(),
                UserId = SessionManager.GetCurrentlyLoggedInUserId()
            });

            ViewBag.Description = "Feedback for Test: '" + trainingTest.TrainingGuideName + "'";

            return View(trainingTest.Feedback.ToList());
        }

        // GET: /ManageTrainingTest/ManageTrainingTest/
        public ActionResult Index()
        {
            var trainingTestQueryParameter = new AllTrainingTestQueryParameter();
            if (Thread.CurrentPrincipal.IsUserInRole(Role.ContentAdmin))
                trainingTestQueryParameter.CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId();

            List<TrainingTestViewModel> result =
                ExecuteQuery<AllTrainingTestQueryParameter, List<TrainingTestViewModel>>(trainingTestQueryParameter);
            return View(FeedbackUpdate(result));
        }

        [HttpGet]
        public  ActionResult FocusAreaReport()
        {
            var model = ExecuteQuery<FocusAreaReportQuery, FocusAreaReportDataSources>(new FocusAreaReportQuery());
            return View(model);
        }
        [HttpGet]
        public ActionResult FocusAreaReportQuery(string TestId)
        {
            var model = ExecuteQuery<FocusAreaReportQuery, FocusAreaReportDataSources>(new FocusAreaReportQuery { TestId = TestId });
            return View("FocusAreaReport", model);

        }
        //Send mail to admins about test expired and tests Stats
        public ActionResult SendMailToAllAdminsAndAuthorOfTest()
        {
            //Get All Customer companies
            CompanyViewModel allCustomerCompanies =
                ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(new AllCustomerUserQueryParameter
                {
                    LoginUserRole =  Thread.CurrentPrincipal.GetRoles().ToList()
                });

            foreach (CompanyModelShort company in allCustomerCompanies.CompanyList)
            {
                //Get All expired tests for a company
                var allTrainingTestExpiringTodayQueryParameter = new AllTrainingTestExpiringTodayQueryParameter();
                List<TrainingTestViewModel> allTestExpiringToday =
                    ExecuteQuery<AllTrainingTestExpiringTodayQueryParameter, List<TrainingTestViewModel>>(
                        allTrainingTestExpiringTodayQueryParameter);

                foreach (TrainingTestViewModel trainingTestViewModel in allTestExpiringToday)
                {
                    var allAssignedTestUsersByTestIdQueryParameter = new AllAssignedTestUsersByTestIdQueryParameter
                    {
                        TestId = trainingTestViewModel.TrainingTestId
                    };

                    TestAssignedUsersAndNotAppearedUsersViewModel allTestAssignedUsersAndNotAppearedUsers =
                        ExecuteQuery
                            <AllAssignedTestUsersByTestIdQueryParameter, TestAssignedUsersAndNotAppearedUsersViewModel>(
                                allAssignedTestUsersByTestIdQueryParameter);

                    IEnumerable<UserViewModel> alladminsAndAuthor =
                        ExecuteQuery<GetAllAdminsFromSameCompanyQueryParameter, IEnumerable<UserViewModel>>(
                            new GetAllAdminsFromSameCompanyQueryParameter
                            {
                                CompanyId = company.Id
                            });

                    foreach (UserViewModel userViewModel in alladminsAndAuthor)
                    {
                        new NotificationService(CommandDispatcher).ComposeAndSendEmailWithAttachments(new NotificationService.EmailTemplate
                        {
                            ControllerContext = ControllerContext,
                            TempData = TempData,
                            ViewData = ViewData,
                            Type = NotificationType.NotifyAboutTestExpiryToAllAdmins,
                            Subject = "OnRamp - Test Expire Alert",
                            Model = new TestAssignedUsersAndNotAppearedUsersViewModel
                            {
                                TestAppearedUsers = allTestAssignedUsersAndNotAppearedUsers.TestAppearedUsers,
                                TestNotAppearedUsers = allTestAssignedUsersAndNotAppearedUsers.TestNotAppearedUsers,
                                RecipentUserName = userViewModel.FirstName,
                                TrainingTestViewModel = trainingTestViewModel
                            },
                            EmailAddress = userViewModel.EmailAddress
                        });
                    }

                    ExecuteCommand(new MarkTrainingTestExpirySentCommandParameter { CustomerId = company.Id, TestId = trainingTestViewModel.TrainingTestId });
                }
            }

            return null;
        }

        public ActionResult TestNotApperUsers()
        {
            var testNotApperUsersParameter = new TestNotApperUsersParameter();
            var result = ExecuteQuery<TestNotApperUsersParameter, TestNotApperUsersViewModel>(testNotApperUsersParameter);

            result.DropDownForTest =
                result.TestList.Select(c => new SerializableSelectListItem
                {
                    Text = c.TestTitle,
                    Value = c.TrainingTestId.ToString()
                });
            return View(result);
        }

        [HttpPost]
        public PartialViewResult TestNotApperUsersData(Guid testId)
        {
            var testNotApperUsersParameter = new TestNotApperUsersParameter
            {
                TestId = testId
            };
            var result =
                ExecuteQuery<TestNotApperUsersParameter, TestNotApperUsersViewModel>(testNotApperUsersParameter);
            return PartialView(result);
        }

        [HttpPost]
        public JsonResult ChangeTrainingTestStatus(Guid trainingTestId, string status)
        {
            var updateTrainingTestStatus = new UpdateTrainingTestStatusCommand
            {
                TrainingTestId = trainingTestId,
                ActiveStatus = status == "true"
            };
            var response = ExecuteCommand(updateTrainingTestStatus);
            NotifyInfo("Training status successfully changed");
            return null;
        }

        [HttpGet]
        public JsonResult IsUserEligibleToTakeTest(Guid trainingTestId)
        {
            var checkUserHasAlreadyAppearedForTestQueryParameter =
                new CheckUserHasAlreadyAppearedForTestQueryParameter
                {
                    CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    TrainingTestId = trainingTestId
                };

            CheckUserHasAlreadyAppearedForTestViewModel checkUserHasAlreadyAppearedForTestViewModel =
                ExecuteQuery<CheckUserHasAlreadyAppearedForTestQueryParameter,
                    CheckUserHasAlreadyAppearedForTestViewModel>(checkUserHasAlreadyAppearedForTestQueryParameter);

            if (checkUserHasAlreadyAppearedForTestViewModel.IsUserEligibleToTakeTest)
            {
                return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = 'F', Model = checkUserHasAlreadyAppearedForTestViewModel }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ChangeTrainingTestDraftStatus(Guid trainingTestId, string status)
        {
            var updateTrainingTestDraftStatus = new UpdateTrainingTestDraftStatusCommand
            {
                TrainingTestId = trainingTestId,
                DraftStatus = status == "true"
            };
            var response = ExecuteCommand(updateTrainingTestDraftStatus);
            NotifyInfo("Training test draft status successfully changed");
            return null;
        }

        [HttpPost]
        public ActionResult ChangeTrainingDraftPublishTest(Guid trainingTestId)
        {
            var updateTrainingTestDraftPublishTest = new PublishTrainingTestCommandParameter
            {
                Company = PortalContext.Current.UserCompany,
                CurrentlyLoggedInUser = SessionManager.GetCurrentlyLoggedInUserId(),
                TrainingTestId = trainingTestId,
            };
            ExecuteCommand(updateTrainingTestDraftPublishTest);
            NotifyInfo("Test draft published successfully");
            return null;
        }

        [HttpGet]
        public JsonResult ForcePeopletoRetakeTest(Guid trainingTestId, string status)
        {
            var testResultQueryParameter = new ForcePeopletoRetakeTestQueryParameter
            {
                TrainingTestId = trainingTestId,
                Company = PortalContext.Current.UserCompany,
                CurrentlyLoggedInUser = SessionManager.GetCurrentlyLoggedInUserId()
            };

            ExecuteQuery<ForcePeopletoRetakeTestQueryParameter, SendMailToUsersToReTakeTest>(
                testResultQueryParameter);
            ExecuteCommand(new PublishTrainingTestCommandParameter
            {
                Company = PortalContext.Current.UserCompany,
                CurrentlyLoggedInUser = SessionManager.GetCurrentlyLoggedInUserId(),
                TrainingTestId  = trainingTestId,
                DoNotAssignTests = true
            });

            return null;
        }

        public ActionResult MyTests(Guid userId)
        {
            var allAssignedTestsForAUserQueryParameter =
                new AllAssignedTestsForAUserQueryParameter
                {
                    UserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    CertificateUrlbase = Url.ActionLink<UploadController>(a => a.Preview(Guid.Empty.ToString(), null)).Replace("/ManageTrainingTest", string.Empty),
                    InMyTests = true
                };

            List<TrainingTestViewModel> model =
                ExecuteQuery<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>(
                    allAssignedTestsForAUserQueryParameter);

            return View(model);
        }

        [HttpPost]
        public JsonResult Delete(Guid id)
        {
            var deleteTrainingTestCommand = new DeleteTrainingTestCommandParameter
            {
                TrainingTestId = id
            };
            ExecuteCommand(deleteTrainingTestCommand);
            if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                //Insert User Activity for Delete Training Test
                var addActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription =
                        "Deleted Training Test ",
                    ActivityType = UserActivityEnum.DeleteTrainingTest,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };
                ExecuteCommand(addActivityCommand);
            }
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditTrainingTest(Guid trainingTestId)
        {
            SessionInformation.IsTestEdited = true;

            var editTrainingTestQueryParameter = new EditTrainingTestQueryParameter
            {
                TrainingTestId = trainingTestId,
                UploadDeleteUrlBase = Url.ActionLink<UploadController>(a => a.Delete(Guid.Empty.ToString(),null)).Replace("/ManageTrainingTest", string.Empty),
                UploadUrlBase = Url.ActionLink<UploadController>(a => a.Get(Guid.Empty.ToString(),false)).Replace("/ManageTrainingTest", string.Empty),
                UploadThumbnailUrlBase = Url.ActionLink<UploadController>(a => a.GetThumbnail(Guid.Empty.ToString(), 300, 300)).Replace("/ManageTrainingTest", string.Empty)
            };

            TrainingTestViewModel trainingTestViewModel =
                ExecuteQuery<EditTrainingTestQueryParameter, TrainingTestViewModel>(editTrainingTestQueryParameter);

            var trainingGuideQueryParameter = new AllTrainingGuideToCreateTestQueryParameter
            {
                Id = trainingTestId,
                CompanyId = PortalContext.Current.UserCompany.Id,
                ShowTrainingGuideForEdit = true
            };

            List<TrainingGuideViewModel> trainingGuideViewModelList =
                ExecuteQuery<AllTrainingGuideToCreateTestQueryParameter, List<TrainingGuideViewModel>>(
                    trainingGuideQueryParameter);

            trainingTestViewModel.CompanyName = PortalContext.Current.UserCompany.CompanyName;
            trainingTestViewModel.PageTitle = "Edit Training Test";
            trainingTestViewModel.ViewMode = ViewMode.Edit;

            foreach (var question in trainingTestViewModel.QuestionsList)
            {
                var list = new List<TestAnswerViewModel>();
                foreach (TestAnswerViewModel testAnswerViewModel in question.TestAnswerList)
                {
                    if (testAnswerViewModel.Option != null)
                    {
                        list.Add(testAnswerViewModel);
                    }
                }

                question.AnswerList = list.Select(c => new SerializableSelectListItem
                {
                    Text = c.Option,
                    Value = c.Option
                });
            }

            if (trainingTestViewModel.ActivePublishDate.HasValue)
            {
                var cloneTestCommand = new CloneTestCommand
                {
                    TrainingTestViewModel = trainingTestViewModel,
                    CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
                };
                ExecuteCommand(cloneTestCommand);
                editTrainingTestQueryParameter.TrainingTestId = cloneTestCommand.newDraftId.Value;
                trainingTestViewModel = ExecuteQuery<EditTrainingTestQueryParameter, TrainingTestViewModel>(editTrainingTestQueryParameter);
            }

            IEnumerable<SerializableSelectListItem> trainingGuideList =
             trainingGuideViewModelList.Select(c => new SerializableSelectListItem
             {
                 Text = c.Title,
                 Value = c.TrainingGuidId.ToString(),
                 Selected = c.TrainingGuidId.Equals(trainingTestViewModel.SelectedTrainingGuideId)
             });
            trainingTestViewModel.TrainingGuideList = trainingGuideList;

            return View("Create", trainingTestViewModel);
        }
        
        public ActionResult CheckTrainingGuideTestTitle(TrainingTestViewModel model)
        {
            if (!SessionInformation.IsTestEdited)
            {
                var checkForTrainingTestTitleExistQueryParameter = new CheckForTrainingTestTitleExistQueryParameter
                {
                    TestName = model.TestTitle
                };
                RemoteValidationResponseViewModel result =
                    ExecuteQuery<CheckForTrainingTestTitleExistQueryParameter, RemoteValidationResponseViewModel>(
                        checkForTrainingTestTitleExistQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult EditTrainingTest(TrainingTestViewModel model)
        {
            SessionInformation.IsTestEdited = false;

            var path = Server.MapPath("~/Content/TrophyPicDir/" + model.TrophyName);
            if (System.IO.File.Exists(path))
            {
                model.TestTrophy = ReadByteFile(path);
            }
            else
            {
                model.TestTrophy = ReadByteFile(Server.MapPath("~/Content/images/Trophy5.png"));
            }
            var saveDraftVersionOfTrainingTestCommand = new SaveDraftVersionOfTrainingTestCommand
            {
                TrainingTestViewModel = model,
                CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };
            var commandResponse = ExecuteCommand(saveDraftVersionOfTrainingTestCommand);
            if (commandResponse.Validation.Any())
            {
                return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                //Insert User Activity for Update Training Test
                var addActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription =
                        model.TestTitle,
                    ActivityType = UserActivityEnum.EditTrainingTest,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };
                ExecuteCommand(addActivityCommand);
            }
            if (saveDraftVersionOfTrainingTestCommand.newDraftId.HasValue)
            { 
                return new JsonResult() { Data = new { DraftCreated = true, Model = model }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new JsonResult() { Data = new { Updated = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult TestPublishPopUp()
        {
            return PartialView("_TestPublishPartial");
        }

        public ActionResult Create(Guid? trainingGuideId)
        {
            var trainingGuideQueryParameter = new AllTrainingGuideToCreateTestQueryParameter();
            if (Thread.CurrentPrincipal.IsUserInRole(Role.ContentAdmin))
                trainingGuideQueryParameter.ColaboratorId = SessionManager.GetCurrentlyLoggedInUserId();

            List<TrainingGuideViewModel> trainingGuideViewModelList =
                ExecuteQuery<AllTrainingGuideToCreateTestQueryParameter, List<TrainingGuideViewModel>>(
                    trainingGuideQueryParameter);

            var trainingTestViewModel = new TrainingTestViewModel
            {
                TrainingGuideList = trainingGuideViewModelList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Title,
                    Value = c.TrainingGuidId.ToString()
                }),
                SelectedTrainingGuideId = trainingGuideId
            };
            trainingTestViewModel.ViewMode = ViewMode.Create;
            trainingTestViewModel.Mode = FunctionalMode.Tests;
            trainingTestViewModel.PageTitle = "Create Training Test";
            return View(trainingTestViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(TrainingTestViewModel model)
        {
            model.TestTrophy = Utility.Convertor.BytesFromFilePath(Server.MapPath("~/Content/TrophyPicDir/" + model.TrophyName));
            model.TrainingTestId = Guid.NewGuid();
            var saveTrainingTestCommand = new SaveTrainingTestCommand
            {
                TrainingTestViewModel = model,
                CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };
            saveTrainingTestCommand.TrainingTestViewModel.CompanyName = PortalContext.Current.UserCompany.CompanyName;
            var result = ExecuteCommand(saveTrainingTestCommand);
            if (result.Validation.Any())
            {
                return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                {
                    //Insert User Activity for Created Training Test
                    var addActivityCommand = new AddUserActivityCommand
                    {
                        ActivityDescription = model.TestTitle,
                        ActivityType = UserActivityEnum.CreateTrainingTest,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    };
                    ExecuteCommand(addActivityCommand);
                }
                return new JsonResult() { Data = new { Created = true , TrainingTestId = model.TrainingTestId.ToString() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        [HttpGet]
        public JsonResult GetGuid()
        {
            return new JsonResult { Data = Guid.NewGuid(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult AutoSave(TrainingTestViewModel model)
        {
            var result = ExecuteCommand(new UpdateTrainingTestCommand { model = model });
            if (result.Validation.Any())
                return new JsonResult { Data = new { Saved = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult { Data = new { Saved = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private byte[] ReadByteFile(string sPath)
        {
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
        public ActionResult IsTestValid(Guid trainingTestId)
        {
            var model = ExecuteQuery<TakeTrainingTestQueryParameter, TrainingTestViewModel>(new TakeTrainingTestQueryParameter { TrainingTestId = trainingTestId });
            if (model != null)
                if (model.QuestionsList.Count > 0 && model.QuestionsList.All(x => x.TestAnswerList.Count() > 0))
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

        }
        public ActionResult TakeTrainingTest(Guid trainingTestId)
        {
            var takeTrainingTestQueryParameter = new TakeTrainingTestQueryParameter
            {
                TrainingTestId = trainingTestId
            };
            TrainingTestViewModel trainingTestViewModel =
                ExecuteQuery<TakeTrainingTestQueryParameter, TrainingTestViewModel>(takeTrainingTestQueryParameter);

            trainingTestViewModel.CompanyName = PortalContext.Current.UserCompany.CompanyName;
            if (IsUserEligible(trainingTestId) || !Thread.CurrentPrincipal.IsInStandardUserRole())
                return View(trainingTestViewModel);
            return RedirectToAction("MyTests", "ManageTrainingTest", new { Area = "ManageTrainingTest",UserId = SessionManager.GetCurrentlyLoggedInUserId() });
        }
        private bool IsUserEligible(Guid trainingTestId)
        {
            return ExecuteQuery<UserCanTakeTrainingTestQuery, bool>(new UserCanTakeTrainingTestQuery
            {
                TestId = trainingTestId,
                UserId = SessionManager.GetCurrentlyLoggedInUserId()
            });
        }
        public ActionResult StartTest(Guid trainingTestId)
        {
            SessionManager.SetCustomerSurveyVisibility(false);
            var incrementTrainingTestTakenCommand = new IncrementTrainingTestTakenCommand
            {
                TrainingTestId = trainingTestId,
                UserId = SessionManager.GetCurrentlyLoggedInUserId()
            };
            ExecuteCommand<IncrementTrainingTestTakenCommand>(incrementTrainingTestTakenCommand);

            Session["TEST_STARTED"] = true;
            var startTestQueryParameter = new StartTestQueryParameter
            {
                TrainingTestId = trainingTestId,
                UploadThumbnailUrlBase = Url.ActionLink<UploadController>(a => a.GetThumbnail(Guid.Empty.ToString(), null, null)).Replace("/ManageTrainingTest", string.Empty),
                UploadUrlBase = Url.ActionLink<UploadController>(a => a.Get(Guid.Empty.ToString(),false)).Replace("/ManageTrainingTest", string.Empty)
            };
            TestViewModel trainingTestViewModel =
                ExecuteQuery<StartTestQueryParameter, TestViewModel>(startTestQueryParameter);

            foreach (QuestionsViewModel question in trainingTestViewModel.QuestionsList)
            {
                var list = new List<TestAnswerViewModel>();
                foreach (TestAnswerViewModel testAnswerViewModel in question.TestAnswerList)
                {
                    if (testAnswerViewModel.Option != null)
                    {
                        list.Add(testAnswerViewModel);
                    }
                }

                question.AnswerList = list.Select(c => new SerializableSelectListItem
                {
                    Text = c.Option,
                    Value = c.TestAnswerId.ToString()
                });
            }
            if (Session["CURRENT_TEST_ID"] == null)
            {
                Session["TestDuration"] = trainingTestViewModel.TestDuration;
                Session["StartTime"] = DateTime.Now;
                Session["CURRENT_TEST_ID"] = trainingTestViewModel.TrainingTestId;
            }
            else
            {
                if (Session["CURRENT_TEST_ID"].ToString() != trainingTestViewModel.TrainingTestId.ToString())
                {
                    Session["TestDuration"] = trainingTestViewModel.TestDuration;
                    Session["StartTime"] = DateTime.Now;
                    Session["CURRENT_TEST_ID"] = trainingTestViewModel.TrainingTestId;
                }
                else if (Session["CURRENT_TEST_ID"].ToString().Equals(trainingTestViewModel.TrainingTestId.ToString()) && Thread.CurrentPrincipal.IsUserInRole("CustomerAdmin"))
                {
                    Session["TestDuration"] = trainingTestViewModel.TestDuration;
                    Session["StartTime"] = DateTime.Now;
                    Session["CURRENT_TEST_ID"] = trainingTestViewModel.TrainingTestId;
                }
            }
            trainingTestViewModel.CompanyName = PortalContext.Current.UserCompany.CompanyName;

            if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                //Insert User Activity for Take Test
                var addActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription =
                        trainingTestViewModel.TestTitle.Trim(),
                    ActivityType = UserActivityEnum.TakeTest,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };
                ExecuteCommand(addActivityCommand);
            }

            return View("StartTest_K",trainingTestViewModel);
        }

        [HttpGet]
        public ActionResult TestEnded()
        {
            var testResultViewModel = (TestResultViewModel)Session["testResultViewModel"];
            if (testResultViewModel.TestReview)
                return View("TestEndedWithReview", testResultViewModel);
            return View(testResultViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult TestEnded(TestViewModel model)
        {
            Session["TEST_STARTED"] = false;
            Session["TestDuration"] = null;
            Session["StartTime"] = null;
            Session["CURRENT_TEST_ID"] = null;
            if (SessionManager.GetUserRoleOfCurrentlyLoggedInUser().Contains(UserRole.Admin) ||
                SessionManager.GetCustomerRolesOfCurrentlyLoggedInUser().Contains(Role.ContentAdmin) ||
                SessionManager.GetUserRoleOfCurrentlyLoggedInUser().Contains(UserRole.CustomerAdmin) ||
                SessionManager.GetCustomerRolesOfCurrentlyLoggedInUser().Contains(Role.CustomerAdmin))
            {
                Session["TestEnded"] = true;
                return RedirectToAction("Index");
            }
            else
            {
                SessionInformation sessionInfo = SessionManager.GetSessionInformation();
                if (sessionInfo != null)
                {
                    //sessionInfo.ShowSurveyModel = true;
                    sessionInfo.TestReferenceId = model.ReferenceId;
                    sessionInfo.ShowTestFeedbackModal = true;
                }
                var testResultViewModel =
                    ExecuteQuery<TestResultQueryParameter, TestResultViewModel>(new TestResultQueryParameter
                    {
                        TestViewModel = model,
                        TestTakenByUserId = SessionManager.GetCurrentlyLoggedInUserId()
                    });

                if (testResultViewModel != null)
                {
                    var resultId = Guid.NewGuid();
                    ExecuteCommand(new SaveTestResultCommand
                    {
                        TestResultViewModel = testResultViewModel,
                        TestTakenByUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        Id = resultId,
                        TestViewModel = model,
                        PortalContext = PortalContext.Current,
                        BasePreviewPath = Url.ActionLink<UploadController>(a => a.Get(Guid.Empty.ToString(), false)).Replace("/ManageTrainingTest", string.Empty)
                    });
                    var cert = ExecuteQuery<TestCertificateForResultQueryParameter, FileUploadResultViewModel>(
                    new TestCertificateForResultQueryParameter
                    {
                        PortalContext = PortalContext.Current,
                        UserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ResultVm = testResultViewModel,
                        BasePreviewPath = Url.ActionLink<UploadController>(a => a.Get(Guid.Empty.ToString(), false)).Replace("/ManageTrainingTest", string.Empty),
                        DefaultCertPath = Server.MapPath("~/Content/images/Certificate.jpg"),
                        ResultId = resultId
                    });
                    if (cert != null)
                        testResultViewModel.CertificateUrl = cert.PreviewPath;
                }

                Session["TestEnded"] = true;
                Session["testResultViewModel"] = testResultViewModel;
                return Json(new { url = Url.Action("TestEnded", "ManageTrainingTest", new { Area = "ManageTrainingTest" }) }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TestHistoryReport(TestHistoryReportParameter query)
        {
            if (query.CompanyId != null)
                PortalContext.Override(query.CompanyId.Value);

            query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole() ? (Guid?)null : PortalContext.Current.UserCompany.Id;

            var vm = ExecuteQuery<TestHistoryReportParameter, TestHistoryReportViewModel>(query);

            return View(vm);
        }

        public void TestHistoryReportToCSV(TestHistoryReportParameter query)
        {
            if (query.CompanyId != null)
                PortalContext.Override(query.CompanyId.Value);
            query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole() ? (Guid?)null : PortalContext.Current.UserCompany.Id;

            var vm = ExecuteQuery<TestHistoryReportParameter, TestHistoryReportViewModel>(query);

            StringWriter sw = new StringWriter();

            sw.WriteLine("\"Full Name\",\"Employee Number\",\"Group\",\"Playbook Name\",\"Test Name\",\"Percentage\",\"Test Result\",\"Test Score\"");

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment;filename=Exported_Users.csv");
            Response.ContentType = "text/csv";

            foreach (var line in vm.Data.Items)
            {
                sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"",
                                           line.User.Name.Trim(),
                                           line.User.EmployeeNo,
                                           line.User.GroupName.Trim(),
                                           line.PlaybookName.Trim(),
                                           line.TestName.Trim(),
                                           line.Result.ToString() + "/" + line.MaxResult.ToString() + " (" + (line.Result / line.MaxResult * 100).ToString() + "%)",
                                           line.Passed ? "Passed" : "Failed",
                                           line.MarksObtain.ToString()));
            }

            Response.Write(sw.ToString());

            Response.End();
        }

        public ActionResult AssignTestToUserOrGroup()
        {
            return View("AssignTrainingTestToUsersOrGroups", ExecuteQuery<AssignTrainingTestQuery, AssignTrainingTestToUsersOrGroupsViewModel>(new AssignTrainingTestQuery()));
        }

        [HttpPost]
        public JsonResult AssignTestsToUsersOrGroup(AssignTestToUsersOrGroupViewModel model)
        {
            var command = new AssignTestsToUsersOrGroupsCommand
            {
                AssignedBy = SessionManager.GetCurrentlyLoggedInUserId(),
                AssignTestToUsersOrGroupViewModel = model
            };
            var response = ExecuteCommand(command);
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Assign_UnAssignTestToUser(Assign_UnAssignTestToUserCommand command)
        {
            command.CurrentlyLoggedInUser = SessionManager.GetCurrentlyLoggedInUserId();
            var result = ExecuteCommand(command);
            if (result.Validation.Any())
                return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult() { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult Assign_UnAssignTests()
        {
            return RedirectToAction("Assign_UnAssignPlaybooks", "ManageTrainingGuides", new { Area = "ManageTrainingGuides" ,Mode = FunctionalMode.Tests });
        }

        [HttpPost]
        public ActionResult Assign_UnAssignTests(ManageableUsersForCompanyQueryParameter query)
        {
            var model = ExecuteQuery<ManageableUsersForCompanyQueryParameter, Assign_UnAssignPlaybooksAndTests>(query);
            return new JsonResult() { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult TimeLeft()
        {
            if (Session["TestDuration"] != null && Session["StartTime"] != null)
            {
                DateTime startTime = Convert.ToDateTime(Session["StartTime"]);
                int testDuration = int.Parse(Session["TestDuration"].ToString());
                DateTime endTime = startTime.AddMinutes(testDuration);
                TimeSpan timeLeft = endTime.Subtract(DateTime.Now);
                string result;
                if (timeLeft.TotalMilliseconds > 0)
                {
                    result = timeLeft.ToString(@"h\:mm\:ss");
                }
                else
                {
                    result = "ExamEnded";
                    Session["TestDuration"] = null;
                    Session["StartTime"] = null;
                    Session["ExamStarted"] = null;
                }
                return Json(new { timeleft = result });
            }
            return Json(new { error = true });
        }

       
    }
}