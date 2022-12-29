using Common.Query;
using Domain.Customer.Models.Test;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using Ramp.Contracts.Command.TestSession;
using Ramp.Contracts.Query.TestSession;
using Ramp.Security.Authorization;
using Web.UI.Code.Extensions;
using Ramp.Contracts.Query.Label;
using Domain.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.Query.Document;
using Common.Events;
using Ramp.Contracts.Events.DocumentWorkflow;
using Web.UI.Helpers;

namespace Web.UI.Controllers {
	public class TestController : KnockoutDocumentController<TestListQuery, TestListModel, Test, TestModel, CreateOrUpdateTestCommand>
    {
        public override void Edit_PostProcess(TestModel model, string companyId = null, DocumentUsageStatus? status = null, string userid = null)
        {

		
            if (model.CoverPicture != null)
            {
                model.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(model.CoverPicture.Id, 300, 300, companyId));
                model.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.CoverPicture.Id, companyId));
                
            }
            if (model.Certificate != null)
                model.Certificate.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.Certificate.Id, companyId));

            model.ContentModels.ToList().ForEach(content =>
            {
                content.Attachments.ToList().ForEach(attachment =>
                {
                    attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                    attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                });
                content.ContentToolsUploads.ToList().ForEach(attachment =>
                {
                    attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                });
            });
			var trainigLabels = new List<string>();
			foreach (var item in model.TrainingLabels.Split(',')) {
				var label = ExecuteQuery<FetchByNameQuery, TrainingLabelModel>(new FetchByNameQuery() { Name = item });
				trainigLabels.Add(label.Id);
			}
			model.LabelIds = string.Join(",", trainigLabels);
			var labels = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
			ViewBag.Labels = labels.OrderBy(c => c.Name).ToList();
			((IDictionary<string, string>)ViewBag.Links).Add("certificates", Url.ActionLink<AchievementController>(a => a.List(null)));

			//neeraj
			var x = PortalContext.Current.UserCompany.Id;
			var ca = ExecuteQuery<FetchAllRecordsQuery, IEnumerable<StandardUser>>(new FetchAllRecordsQuery());
			var l = new List<StandardUser>();
			foreach (var c in ca.ToList())
			{
				if (c.Id != User.GetId())
				{
					l.Add(c);
				}
			}

			ViewBag.ContentApprovers = l;
			//get all approver name from document Id
			if (model.Approver != null)
			{
				List<string> names = new List<string>();

				string[] Ids = model.Approver.Split(',');

				foreach (var id in Ids)
				{
					var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = id });

					names.Add(userDetail.UserName);
				}

				ViewBag.DocumentApprovers = string.Join(",", names);
			}
		}
        protected override void Preview_PostProcess(TestModel model, string companyId = null, string checkUser = null ,bool isGlobal=false, DocumentUsageStatus? status = null)
        {
			ViewBag.IsGlobalAccessed = model.IsGlobalAccessed;

			Edit_PostProcess(model, companyId);
        }

        protected void Preview_PostProcess(TestResultModel model, string companyId = null)
        {
            model.ContentModels.ToList().ForEach(content =>
            {
                content.Attachments.ToList().ForEach(attachment =>
                {
                    attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                    attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                });
                content.ContentToolsUploads.ToList().ForEach(attachment =>
                {
                    attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                });
            });
            if (model.RandomizeQuestions)
            {
                var originalQuestions = model.ContentModels.OrderBy(x => x.Number).ToList();
                var questions = new List<TestQuestionResultModel>(originalQuestions.Count);
                var newQuestionIndexes = new Dictionary<int,int>();
                var newQuestionAnswerIndexes = new Dictionary<int,List<Tuple<int, int>>>();
                var randomNumbergenerator = new Random();
                for (var i = 0; i < originalQuestions.Count; i++)
                {
                    var newIndex = randomNumbergenerator.Next(originalQuestions.Count);
                    while (newQuestionIndexes.Values.Contains(newIndex))
                        newIndex = randomNumbergenerator.Next(originalQuestions.Count);
                    newQuestionIndexes.Add(i, newIndex);
                    var q = originalQuestions[i];
                    if (q.Answers != null)
                    {
                        newQuestionAnswerIndexes.Add(i, new List<Tuple<int, int>>());
                        for (var j = 0; j < q.Answers.Count(); j++)
                        {
                            var newAnswerIndex = randomNumbergenerator.Next(q.Answers.Count());
                            while (newQuestionAnswerIndexes[i].Any(x => x.Item2 == newAnswerIndex))
                                newAnswerIndex = randomNumbergenerator.Next(q.Answers.Count());
                            newQuestionAnswerIndexes[i].Add(new Tuple<int, int>(j, newAnswerIndex));
                        }
                    }
                }

                newQuestionIndexes.Values.OrderBy(x => x).ToList().ForEach(i =>
                {
                    var originalIndex = newQuestionIndexes.First(x => x.Value == i).Key;
                    var originalQuestion = originalQuestions.ElementAt(originalIndex);
                    var originalAnswers = originalQuestion.Answers.ToList();
                    var answers = new List<TestQuestionAnswerResultModel>();
                    var answerIndexes = newQuestionAnswerIndexes.ContainsKey(originalIndex) ? newQuestionAnswerIndexes[originalIndex] : new List<Tuple<int, int>>();
                    answerIndexes.OrderBy(x => x.Item2).ToList().ForEach(j =>
                    {
                        var originalAnswer = originalAnswers.ElementAt(j.Item1);
                        answers.Add(originalAnswer);
                    });
                    originalQuestion.Answers = answers;
                    questions.Add(originalQuestion);
                });
                model.ContentModels = questions;
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [System.Web.Mvc.HttpGet]
        public ActionResult Start(object id, bool isGlobal=false)
        {
            if (Thread.CurrentPrincipal.IsInStandardUserRole() && !ExecuteQuery<UserCanTakeTestQuery, bool>(new UserCanTakeTestQuery
            {
                UserId = SessionManager.GetCurrentlyLoggedInUserId(),
                TestId = id.ToString(),
				IsGlobalAccessed=isGlobal
            }))
            {
                return RedirectToAction("MyDocuments", "Document");
            }

            ViewBag.Links = new Dictionary<string, string>()
            {
                {"index", Url.Action("MyDocuments", "Document", new {Area = ""})},
                {"test:timeleft", Url.Action("TimeLeft", "Test", new {Area = ""})},
                {"test:ended", Url.Action("Ended", "Test", new {Area = ""})},
                {"test:review", Url.Action("Review", "Test", new {Area = ""})},
                {"userFeedback:save", Url.Action("Save", "UserFeedback", new {Area = ""})},
                {"userFeedback:types", Url.Action("Types", "UserFeedback", new {Area = ""})},
                {"userFeedback:contentTypes", Url.Action("ContentTypes", "UserFeedback", new {Area = ""})}
            };
            var test = ExecuteQuery<FetchByIdQuery<Test>, TestResultModel>(new FetchByIdQuery<Test> { Id = id.ToString() });
			test.IsGlobalAccessed = isGlobal;
			if (test == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Preview_PostProcess(test);

            TestSessionViewModel testSession = null;
            if (Thread.CurrentPrincipal.IsInStandardUserRole())
            {
                testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
                {
                    UserId = Thread.CurrentPrincipal.GetId().ToString()
                });

                if (testSession?.CurrentTestId == null)
                {
                    ExecuteCommand(new TestSessionStartCommand
                    {
                        UserId = Thread.CurrentPrincipal.GetId().ToString(),
                        CurrentTestId = test.Id,
						IsGlobalAccessed = isGlobal
				});
                }
            }
            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            ViewBag.TestTimeLeftInterval = ConfigurationManager.AppSettings["TestTimeLeftInterval"];
            ViewBag.Duration = testSession?.TimeLeft.ToString(@"h\:mm\:ss") ?? TimeSpan.FromMinutes(test.Duration).ToString(@"h\:mm\:ss");

            return View(test);
        }
        [System.Web.Mvc.HttpGet]
        public ActionResult TimeLeft()
        {
            var testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
            {
                UserId = Thread.CurrentPrincipal.GetId().ToString()
            });
            if (testSession != null)
            {
                if (testSession.TimeLeft.TotalMilliseconds > 0)
                {
                    Response.StatusCode = (int) HttpStatusCode.OK;
                    return new ContentResult { Content = testSession.TimeLeft.ToString(@"h\:mm\:ss") };
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        [System.Web.Mvc.HttpPost]
        [ValidateInput(false)]
        public ActionResult Ended(CreateTestResultCommand command)
        {
            TestSessionViewModel testSession = null;
            var userId = Thread.CurrentPrincipal.GetId().ToString();
            if (Thread.CurrentPrincipal.IsInStandardUserRole())
            {
                testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
                {
                    UserId = userId
                });
                if (testSession == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                ExecuteCommand(new TestSessionEndCommand
                {
                    UserId = userId,
					IsGlobalAccessed=command.IsGlobalAccessed
				});
            }
            command.UserId = userId;
            command.TimeLeft = testSession?.TimeLeft ?? TimeSpan.Zero;
            command.PortalContext = PortalContext.Current;
            var result = ExecuteCommand(command);
            if (!result.Validation.Any())
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                return new ContentResult { Content = command.ResultId };
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        public ActionResult Review(object id)
        {
            ViewBag.Links = new Dictionary<string, string>()
            {
                { "index" ,Url.Action("MyDocuments","Document",new { Area = ""})},
                { "userFeedback:save" ,Url.Action("Save","UserFeedback",new {Area = "" })},
                { "userFeedback:types" ,Url.Action("Types","UserFeedback",new {Area = "" })},
                { "userFeedback:contentTypes" ,Url.Action("ContentTypes","UserFeedback",new {Area = "" })},
            };
            var result = ExecuteQuery<FetchByIdQuery, TestResultModel>(new FetchByIdQuery { Id = id });
            if (result == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Edit_PostProcess(result);
            return View(result);

        }
        public ActionResult InProgress()
        {
            var userId = Thread.CurrentPrincipal.GetId().ToString();
            var testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
            {
                UserId = userId
            });
            if (testSession == null || (testSession != null && string.IsNullOrWhiteSpace(testSession.CurrentTestId)))
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            return View(ExecuteQuery<FetchByIdQuery, TestModel>(new FetchByIdQuery { Id = testSession.CurrentTestId }));
        }

		[HttpPost]
		[ValidateInput(false)]
		public object DocumentWorkFlowMessageSave(TestModel model, string message, bool creator, bool? approver, bool? admin, string[] approvers, string action)
		{
			var userId = Thread.CurrentPrincipal.GetId();
			if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
			{
				approver = false;
				creator = false;
				admin = true;
			}

			if (creator)
			{
				model.CreatedBy = userId.ToString();
			}

			if (admin != true && message != "")
			{
				var cs = new SaveOrUpdateDocumentWorkflowAuditMessagesCommand
				{
					DocumentId = model.Id == null ? null : model.Id,
					CreatorId = model.CreatedBy == "null" ? userId : Guid.Parse(model.CreatedBy),
					ApproverId = creator ? Guid.Empty : userId,
					Message = message
				};

				ExecuteCommand(cs);
			}
			if (approvers != null && message != "")
			{
				sendMail(model.CreatedBy, message, model.Id, approvers, creator, approver, admin, action, model);
			}
			else
			{
				string[] approverIds = model.Approver.Split(',');
				sendMail(model.CreatedBy, message, model.Id, approverIds, creator, approver, admin, action, model);
			}

			return null;
		}
		private void sendMail(string creatorId, string message, string documentId, string[] approverIds, bool? isCreator, bool? isApprover, bool? isAdmin, string action, TestModel model)
		{
			string subject = null;
			//if admin is delining or accepting document
			if (isAdmin == true)
			{
				//send notification to all approver and content creator
				message = "A document has been " + action + " approved by the global administrator";

				subject = "A document has been Approved";
				addNotification(documentId, creatorId, "A document has been Approved", DocumentNotificationType.Accepted.GetDescription());


				//send notification to all approvaers
				foreach (var a in approverIds)
				{
					mail(a, documentId, message, subject, model);

					addNotification(documentId, a, "A document has been Approved", DocumentNotificationType.Accepted.GetDescription());
				}

				//send mail to creator
				mail(creatorId, documentId, message, subject, model);

			}
			//if creator submits document for approval
			else if (isCreator == true)
			{

				subject = "A document has been submitted for approval";
				//send notification to all approvers
				foreach (var a in approverIds)
				{
					mail(a, documentId, message, subject, model);
					addNotification(documentId, a, "A document has been submitted for approval", DocumentNotificationType.Assign.GetDescription());
				}

			}
			//if approver accepts or declines document
			else if (isApprover == true)
			{

				if (action == "decline")
				{
					subject = "A document has been declined";
					addNotification(documentId, creatorId, "A document has been declined", DocumentNotificationType.Declined.GetDescription());
				}
				else if (action == "accept")
				{
					subject = "A document has been approved";
					addNotification(documentId, creatorId, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
				}

				//send notification to all approvaers
				foreach (var a in approverIds)
				{
					mail(a, documentId, message, subject, model);
					if (action == "decline")
					{
						addNotification(documentId, a, "A document has been declined", DocumentNotificationType.Declined.GetDescription());
					}
					else if (action == "accept")
					{
						addNotification(documentId, a, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
					}
				}

				//send mail to creator
				mail(creatorId, documentId, message, subject, model);

			}
		}

		private void mail(string userId, string documentId, string message, string subject, TestModel model)
		{
			var approver = new UserViewModel();
			var creator = new UserViewModel();
			var userQueryParameter = new UserQueryParameter
			{
				UserId = Guid.Parse(userId)
			};

			var user = ExecuteQuery<UserQueryParameter, UserViewModel>(userQueryParameter);
			var name = user.FirstName + " " + user.LastName;

			var author = ExecuteQuery<UserQueryParameter, UserViewModel>(new UserQueryParameter
			{
				UserId = Guid.Parse(model.CreatedBy)
			});

			var documentTitles = new List<DocumentTitlesAndTypeQuery>();
			documentTitles.Add(new DocumentTitlesAndTypeQuery
			{
				DocumentTitle = model.Title,
				AdditionalMsg = model.Description,
				DocumentType = model.DocumentType,
				DocumentId = model.Id,
				Author = author.FirstName + " " + author.LastName,
				Points = model.Points,
				Passmark = model.PassMarks
			});

			var eventPublisher = new EventPublisher();
			eventPublisher.Publish(new DocumentWorkflowEvent
			{
				UserViewModel = new UserViewModel() { FirstName = name, EmailAddress = user.EmailAddress },
				CompanyViewModel = PortalContext.Current.UserCompany,
                DocumentTitles = documentTitles,
				Subject = subject,
				Message = message
			});
		}

		private void addNotification(string DocumentId, string UserId, string AdditionalMsg, string notiType)
		{
			var documentNotifications = new List<DocumentNotificationViewModel>();


			var notificationModel = new DocumentNotificationViewModel
			{
				AssignedDate = DateTime.Now,
				IsViewed = false,
				DocId = DocumentId,
				UserId = UserId,
				NotificationType = notiType,
				Message = AdditionalMsg
			};


			documentNotifications.Add(notificationModel);

			ExecuteCommand(documentNotifications);
		}
	}
}