using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using javax.management.relation;
using Newtonsoft.Json;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.Course;
using Ramp.Contracts.Query.Course;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.Query.User;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web.UI.Areas.Reporting.Controllers;
using Web.UI.Code.Extensions;
using Serializer = System.Web.Script.Serialization.JavaScriptSerializer;

namespace Web.UI.Controllers
{
    public class CourseController : RampController
    {

        private readonly ITransientRepository<AssignedCourse> _assignedCourseRepository;
        private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardUserGroupRepository;

        public CourseController(
            ITransientRepository<AssignedCourse> assignedCourseRepository,
            IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardUserGroupRepository
            )
        {
            assignedCourseRepository = _assignedCourseRepository;
            _standardUserGroupRepository = standardUserGroupRepository;
        }

        public ActionResult Index()
        {

            var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();
            ViewBag.Categories = IndentedCategories(categories, "#", 0);
            ViewBag.Admins = ExecuteQuery<EmptyQueryParameter, IEnumerable<UserModelShort>>(new EmptyQueryParameter()).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserData
            {
                Value = x.Name,
                Extra = x.Email,
                Id = x.Id.ToString()
            }).ToList();
            ViewBag.Users = ExecuteQuery<StandardUsersQuery, IEnumerable<UserModelShort>>(new StandardUsersQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserData
            {
                Value = x.Name,
                Extra = x.Email,
                Id = x.Id.ToString()
            }).ToList();

            var groups = ExecuteQuery<CustomerGroup, IEnumerable<JSTreeViewModel>>(new CustomerGroup()).ToList();
            var ng = new List<GroupViewModelShort>();
            foreach (var g in groups)
            {
                ng.Add(new GroupViewModelShort
                {
                    Id = Guid.Parse(g.id),
                    Name = g.text,
                    Selected = false
                });
            }

            ViewBag.Groups = ng;


            //get list of all courses
            var courses = ExecuteQuery<CourseListQuery, IEnumerable<CourseModel>>(new CourseListQuery() { });

            //foreach (var c in courses)
            //{
            //	c.Certificate.Url = Url.ActionLink<UploadController>(x => x.GetThumbnail(c.AchievementType.ToString(), 424, 300));
            //}

            CourseListModel model = new CourseListModel();
            model.AddRange(courses);

            ViewData["CourseModel"] = new CourseModel();

            return View(model);
        }

        /// <summary>
        /// this is used for search the Filter Course 
        /// </summary>
        /// <param name="searchText">search Filter Course based on searchtext</param>
        /// <returns></returns>
        public ActionResult FilterCourse(courseFilter filter)
        {

            var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();
            ViewBag.Categories = IndentedCategories(categories, "#", 0);
            ViewBag.Admins = ExecuteQuery<EmptyQueryParameter, IEnumerable<UserModelShort>>(new EmptyQueryParameter()).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserData
            {
                Value = x.Name,
                Extra = x.Email,
                Id = x.Id.ToString()
            }).ToList();
            ViewBag.Users = ExecuteQuery<StandardUsersQuery, IEnumerable<UserModelShort>>(new StandardUsersQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserData
            {
                Value = x.Name,
                Extra = x.Email,
                Id = x.Id.ToString()
            }).ToList();

            var groups = ExecuteQuery<CustomerGroup, IEnumerable<JSTreeViewModel>>(new CustomerGroup()).ToList();
            var ng = new List<GroupViewModelShort>();
            foreach (var g in groups)
            {
                ng.Add(new GroupViewModelShort
                {
                    Id = Guid.Parse(g.id),
                    Name = g.text,
                    Selected = false
                });
            }

            ViewBag.Groups = ng;


            //get list of all courses
            var courses = ExecuteQuery<CourseListQuery, IEnumerable<CourseModel>>(new CourseListQuery() { });

            var models = new List<CourseModel>();
            CourseListModel model = new CourseListModel();
            if (models.Count <= 0)
            {
                models = courses.ToList();
            }

            if (!string.IsNullOrEmpty(filter.searchText))
            {
                models = courses.Where(z => z.Title.Contains(filter.searchText)).ToList();
            }
            if (!string.IsNullOrEmpty(filter.Status))
            {
                var statuss = filter.Status.Split(',');
                foreach (var s in statuss)
                {
                    models = models.Where(z => z.Status == s).ToList();
                    model.AddRange(models);
                }
            }

            if (models.Count <= 0)
            {
                model.AddRange(models);
            }
            else if(model.Count <= 0)
            {
                model.AddRange(models);
            }

            return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

           // return PartialView("_Courses", model);
        }

        //[HttpPost]
        //public ActionResult Index(CourseModel model)
        //{
        //	var x = model;

        //	List <AssociatedDocument> documents = new List<AssociatedDocument>();

        //	foreach(var document in model.Documents)
        //	{
        //		documents.Add(new AssociatedDocument {
        //			DocumentId = Guid.Parse(document.Id),
        //			Type = document.Type
        //		});
        //	}

        //	//call command handler
        //	ExecuteCommand(new AddOrUpdateCourseCommand
        //	{
        //		Title = model.Title,
        //		Description = model.Description,
        //		GlobalAccessEnabled = model.IsGlobalEnabled,
        //		ExpiryEnabled = model.IsCourseExpiryEnabled,
        //		ExpiryInDays = model.ExpiryInDays,
        //		Documents = documents
        //	});

        //	return null;
        //}

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var result = ExecuteCommand(new DeleteByIdCommand<Course> { Id = id });

            return null;
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return null;
        }


        private IEnumerable<SerializableSelectListItem> IndentedCategories(List<JSTreeViewModel> categories, string parentId, int indent)
        {
            var result = new List<SerializableSelectListItem>();
            var children = categories.Where(c => c.parent == parentId).ToList();
            if (!children.Any())
            {
                return Enumerable.Empty<SerializableSelectListItem>();
            }

            foreach (var child in children)
            {
                result.Add(new SerializableSelectListItem
                {
                    Text = $"{new string('\u00a0', indent * 2)}{child.text}",
                    Value = child.id
                });
                result.AddRange(IndentedCategories(categories, child.id, indent + 1));
            }

            return result;
        }

        [HttpPost]
        public ActionResult UsersNotAssignedCourse(string[] groupIds, string[] allDocumentId)
        {
            if (!groupIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var users = ExecuteQuery<UsersNotAssignedDocumentQuery, IEnumerable<UserModelShort>>(
                new UsersNotAssignedDocumentQuery
                {
                    GroupIds = groupIds
                });

            var assignedCourses = _assignedCourseRepository.List.Where(x => !x.Deleted && allDocumentId.Contains(x.CourseId.ToString())).ToList();

            //assignedDocuments = await Task.FromResult(assignedDocuments.ToList());

            var assignedUsers = new List<AssignedCourse>();

            var result = new List<DocumentListModel>();

            //var memoList = ExecuteQuery<MemoListQuery, IEnumerable<DocumentListModel>>(new MemoListQuery()).Where(c => !c.Deleted).ToList();

            ////memoList = await Task.FromResult(memoList.ToList());

            //var manualList = ExecuteQuery<TrainingManualListQuery, IEnumerable<DocumentListModel>>(new TrainingManualListQuery()).Where(c => !c.Deleted).ToList();

            ////manualList = await Task.FromResult(manualList.ToList());

            //var testList = ExecuteQuery<TestListQuery, IEnumerable<DocumentListModel>>(new TestListQuery()).Where(c => !c.Deleted).ToList();

            ////testList = await Task.FromResult(testList.ToList());

            //var policyList = ExecuteQuery<PolicyListQuery, IEnumerable<DocumentListModel>>(new PolicyListQuery()).Where(c => !c.Deleted).ToList();

            ////policyList = await Task.FromResult(policyList.ToList());

            //var checklistList = ExecuteQuery<CheckListQuery, IEnumerable<DocumentListModel>>(new CheckListQuery()).Where(c => !c.Deleted).ToList();

            ////checklistList = await Task.FromResult(checklistList.ToList());

            //result.AddRange(memoList);
            //result.AddRange(manualList);
            //result.AddRange(testList);
            //result.AddRange(policyList);
            //result.AddRange(checklistList);
            foreach (var user in users)
            {
                foreach (var assign in assignedCourses)
                {
                    var assignedDocumentUsers = new AssignedDocumentInfo();
                    if (user.Id == assign.UserId)
                    {
                        assignedDocumentUsers.DocumentId = assign.CourseId.ToString();
                        assignedDocumentUsers.IsDocumentAssign = true;
                        user.AssignedDocumentUsers.Add(assignedDocumentUsers);
                    }
                }
            }

            foreach (var user in users)
            {
                if (user.AssignedDocumentUsers.Count > 0)
                {
                    foreach (var assigned in user.AssignedDocumentUsers.ToList())
                    {
                        foreach (var a in allDocumentId)
                        {
                            var assignedDocumentUsers = new AssignedDocumentInfo();
                            if (assigned.DocumentId != a)
                            {
                                assignedDocumentUsers.DocumentId = a;
                                assignedDocumentUsers.IsDocumentAssign = false;
                                user.AssignedDocumentUsers.Add(assignedDocumentUsers);
                            }
                        }
                    }
                }
            }

            foreach (var user in users)
            {
                foreach (var docs in user.AssignedDocumentUsers)
                {
                    foreach (var doc in result)
                    {
                        if (docs.DocumentId == doc.Id)
                        {
                            docs.Title = doc.Title;
                        }
                    }
                }
            }

            foreach (var usr in assignedCourses)
            {
                var user = assignedCourses.Where(x => x.UserId == usr.UserId).ToList();
                if (user.Count == allDocumentId.Count())
                {
                    assignedUsers.Add(usr);
                }
            }

            var userIds = assignedUsers.Select(x => x.UserId).Distinct().ToList();
            //userIds = await Task.FromResult(userIds.ToList());

            var matchUsers = users.Where(y => userIds.Any(x => x == y.Id)).ToList();
            //matchUsers = await Task.FromResult(matchUsers.ToList());

            users = users.Except(matchUsers);

            var groupList = _standardUserGroupRepository.List.ToList();

            var n = new List<Domain.Customer.Models.Groups.StandardUserGroup>();

            foreach (var gl in groupList)
            {
                foreach (var g in groupIds)
                {
                    if (gl.GroupId.ToString() == g)
                    {
                        n.Add(gl);
                    }
                }
            }
            var newResult = new List<UserModelShort>();
            foreach (var u in users)
            {
                foreach (var g in n)
                {
                    if (g.UserId == u.Id)
                    {
                        newResult.Add(u);
                    }
                }
            }

            return new JsonResult { Data = newResult.Distinct(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult UsersAssignedCourse(string[] groupIds, string[] allDocumentId)
        {
            if (!groupIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var users = ExecuteQuery<UsersAssignedDocumentQuery, IEnumerable<UserModelShort>>(
                new UsersAssignedDocumentQuery
                {
                    GroupIds = groupIds,
                    AllDocumentId = allDocumentId
                });

            var groupList = _standardUserGroupRepository.List.ToList();

            var n = new List<Domain.Customer.Models.Groups.StandardUserGroup>();

            foreach (var gl in groupList)
            {
                foreach (var g in groupIds)
                {
                    if (gl.GroupId.ToString() == g)
                    {
                        n.Add(gl);
                    }
                }
            }
            var newResult = new List<UserModelShort>();
            foreach (var u in users)
            {
                foreach (var g in n)
                {
                    if (g.UserId == u.Id)
                    {
                        newResult.Add(u);
                    }
                }
            }
            return new JsonResult { Data = newResult, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult CourseWorkFlowMessageSave(CourseModel model)
        {

            var uId = SessionManager.GetCurrentlyLoggedInUserId();
            //call command handler save to database
            DocumentStatus documentStatus = DocumentStatus.Draft;
            if (model.Status == "1")
            {
                documentStatus = DocumentStatus.Draft;
            }
            else if (model.Status == "2")
            {
                documentStatus = DocumentStatus.Published;
            }

            ExecuteCommand(new AddOrUpdateCourseCommand
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                GlobalAccessEnabled = model.IsGlobalEnabled,
                ExpiryEnabled = model.IsCourseExpiryEnabled,
                ExpiryInDays = model.ExpiryInDays,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                AllocatedAdmins = String.Join(",", model.AllocatedAdmins),
                CategoryId = Guid.Parse(model.CategoryId),
                AchievementId = model.AchievementType,
                AssignedUsers = model.AssignedUsers,
                WorkflowEnabled = model.WorkflowEnabled,
                CourseStatus = model.CourseStatus,
                Documents = model.Documents,
                Points = model.Points,
                Status = documentStatus,
                CreatedBy = SessionManager.GetCurrentlyLoggedInUserId()
            });
            return null;
        }

        [HttpGet]
        public ActionResult MyCourses()
        {
            var courses = ExecuteQuery<CourseListQuery, IEnumerable<CourseModel>>(new CourseListQuery() {userID= SessionManager.GetCurrentlyLoggedInUserId().ToString() });

            CourseListModel model = new CourseListModel();
            model.AddRange(courses);

            return View(model);
        }

        [HttpGet]
        public CourseModel Edit(string id)
        {

            var model = ExecuteQuery<FetchByIdQuery, CourseModel>(new FetchByIdQuery() { }); ;

            return model;

            //var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();
            //ViewBag.Categories = IndentedCategories(categories, "#", 0); ;

            //ViewBag.Admins = ExecuteQuery<EmptyQueryParameter, IEnumerable<UserModelShort>>(new EmptyQueryParameter()).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserData
            //{
            //	Value = x.Name,
            //	Extra = x.Email,
            //	Id = x.Id.ToString()
            //}).ToList();



            //ViewBag.Users = ExecuteQuery<StandardUsersQuery, IEnumerable<UserModelShort>>(new StandardUsersQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserData
            //{
            //	Value = x.Name,
            //	Extra = x.Email,
            //	Id = x.Id.ToString()
            //}).ToList();

            //var groups = ExecuteQuery<CustomerGroup, IEnumerable<JSTreeViewModel>>(new CustomerGroup()).ToList();
            //var ng = new List<GroupViewModelShort>();
            //foreach (var g in groups)
            //{
            //	ng.Add(new GroupViewModelShort
            //	{
            //		Id = Guid.Parse(g.id),
            //		Name = g.text,
            //		Selected = false
            //	});
            //}

            //ViewBag.Groups = ng;


        }


        public ActionResult ViewAdmins(string id)
        {
            var data = ExecuteQuery<FetchByIdQuery, IEnumerable<UserModelShort>>(new FetchByIdQuery { Id = id }).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserModelShort
            {
                Name = x.Name,
                Email = x.Email,
                Id = x.Id
            }).ToList();

            return PartialView("_ViewAdmins", data);
        }
    }
}