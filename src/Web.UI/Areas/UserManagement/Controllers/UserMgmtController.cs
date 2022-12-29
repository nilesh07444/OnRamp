
using Common.Query;
using Domain.Customer.Models;
using Domain.Customer.Models.CustomRole;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using Ramp.Contracts.CommandParameter.Settings;
using Ramp.Contracts.Query.Bundle;

using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.CustomerRoles;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Web.Mvc;

using System.Web.Script.Serialization;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;
using Role = Ramp.Contracts.Security.Role;

namespace Web.UI.Areas.UserManagement.Controllers
{

    public class Test
    {
        public string MyProperty { get; set; }
    }

    public class UserMgmtController : RampController
    {
        // GET: UserManagement/UserMgmt
        public ActionResult Index(Guid companyId, Guid userId, String companyName, Guid? groupId)
        {

            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = companyId,
                CompanyName = companyName,
                UserId = userId,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };

            var groups = ExecuteQuery<CustomerGroup, IEnumerable<JSTreeViewModel>>(new CustomerGroup()).ToList();
            groups.ForEach(x =>
            {
                if (x.parent == "#")
                    x.parent = $"{Guid.Empty}";
            });
            (groups as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Groups" });
            ViewBag.Groups = groups;

            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);
            var type = companyUserViewModel.FilterCompanyCustomer.Where(c => c.Type == "Role").FirstOrDefault().FilterData.Where(c => c.Name == "Standard User" || c.Name == "Global Admin").ToList();
            type.Add(new FilterData { Name = "Managing Admin", Id = Guid.Empty.ToString() });
            var cur = ExecuteQuery<FetchAllRecordsQuery, List<CustomUserRoles>>(
                new FetchAllRecordsQuery
                { });
            foreach (var c in cur)
            {
                type.Add(new FilterData { Name = c.Title, Id = c.Id.ToString() });
            }

            companyUserViewModel.FilterCompanyCustomer = companyUserViewModel.FilterCompanyCustomer.Where(c => c.Type != "Role").ToList();

            companyUserViewModel.FilterCompanyCustomer.Add(new FilterCompanyCustomer { Type = "Role", FilterData = type });

            //Get data to bind group drop-down
            List<GroupViewModel> groupViewModelList =
                ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter
                {
                    CompanyId = companyId,
                });

            companyUserViewModel.CompanyName = companyName;
            companyUserViewModel.CompanyId = companyId;
            if (TempData["UserSaved"] != null)
                ViewData["Message"] = TempData["UserSaved"].ToString();
            else
                ViewData["Message"] = null;
            companyUserViewModel.Paginate.IsFirstPage = true;
            companyUserViewModel.Paginate.IsLastPage = false;

            companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.Id != customerCompanyUserQueryParameter.LoggedInUserId).ToList();

            #region for chart 

            var model = new UserChartViewModel();

            //var roles = ExecuteQuery<FetchByCategoryIdQuery, List<CustomerRole>>(new FetchByCategoryIdQuery()).Where(c => c.RoleName == "StandardUser" || c.RoleName == "CustomerAdmin");
            var roles = ExecuteQuery<FetchByCategoryIdQuery, List<CustomerRole>>(new FetchByCategoryIdQuery());
            foreach (var item in roles.Distinct())
            {
                model.RoleCount.Add(companyUserViewModel.UserList.Where(c => c.RoleName == item.Description).Count());

                var roleName = "";
                switch (item.Description)
                {
                    case "Push content to users":
                        roleName = "Document Sender";
                        break;
                    case "Manage ActivityLog":
                        roleName = "Training Manager";
                        break;
                    case "Manage Users":
                        roleName = "User Manager";
                        break;
                    case "Manage OnRamp Portal Settings":
                        roleName = "Settings Manager";
                        break;
                    case "Reports":
                        roleName = "Report Manager";
                        break;
                    case "Manage Virtual Meetings":
                        roleName = "Virtual Meetings Manager";
                        break;
                    case "Manage Tags":
                        roleName = "Tag Manager";
                        break;
                    case "Global Admin":
                        roleName = "Global Administrator";
                        break;
                    case "Manage Documents":
                        roleName = "Document Manager";
                        break;
                    case "Standard User":
                        roleName = "Standard User";
                        break;

                    default:
                        roleName = "CustomRole";
                        break;
                }


                model.RoleName.Add(roleName);
            }
            model.RoleName = model.RoleName.Distinct().ToList();
            //model.RoleCount = model.RoleName
            model.Status = new List<string> { "Active", "Inactive" };
            model.StatusCount = new List<int> { companyUserViewModel.UserList.Where(c => c.IsActive).Count(), companyUserViewModel.UserList.Where(c => !c.IsActive).Count() };
            model.TypeName = new List<string> { "Self-Signup", "Other" };
            model.TypeCount = new List<int> { companyUserViewModel.UserList.Where(c => c.IsFromSelfSignUp).Count(), companyUserViewModel.UserList.Where(c => !c.IsFromSelfSignUp).Count() };

            companyUserViewModel.UserChartViewModel = model;

            #endregion


            companyUserViewModel.Paginate.Page = (companyUserViewModel.UserList.Count / companyUserViewModel.Paginate.PageSize) < 0 ? (companyUserViewModel.UserList.Count / companyUserViewModel.Paginate.PageSize) : (companyUserViewModel.UserList.Count / companyUserViewModel.Paginate.PageSize) + 1;
            companyUserViewModel.Paginate.TotalItems = companyUserViewModel.UserList.Count;
            companyUserViewModel.UserList = companyUserViewModel.UserList.Skip((companyUserViewModel.Paginate.PageIndex - 1) * companyUserViewModel.Paginate.PageSize).Take(companyUserViewModel.Paginate.PageSize).ToList();

            companyUserViewModel.Paginate.StartPage = companyUserViewModel.Paginate.Page >= 1 ? 1 : 0;
            companyUserViewModel.Paginate.EndPage = companyUserViewModel.Paginate.Page >= 7 ? 7 : companyUserViewModel.Paginate.Page;
            if (companyUserViewModel.Paginate.PageIndex == 1)
            {
                companyUserViewModel.Paginate.FirstPage = 1;
                var records = companyUserViewModel.Paginate.PageIndex * companyUserViewModel.Paginate.PageSize;
                if (records <= companyUserViewModel.Paginate.TotalItems)
                {
                    companyUserViewModel.Paginate.LastPage = records;
                }
                else
                {
                    companyUserViewModel.Paginate.LastPage = companyUserViewModel.Paginate.TotalItems;
                }
            }

            foreach (var x in companyUserViewModel.UserList)
            {
                //added by neeraj
                var standardUserGroups = ExecuteQuery<StandardUserGroupByUserIdQuery, StandardUserGroupViewModel>(new StandardUserGroupByUserIdQuery() { UserId = x.Id.ToString() });

                string gr = null;

                foreach (var g in standardUserGroups.GroupList)
                {
                    if (gr == null)
                    {
                        gr = gr + g.Title;

                    }
                    else
                    {
                        gr = gr + ", " + g.Title;

                    }
                }

                x.GroupName = gr;
            }

            return View(companyUserViewModel);
        }

        /// <summary>
        /// This one used to apply the search filter over the company user list
        /// </summary>
        /// <param name="companyName">this is for company name of user</param>
        /// <param name="companyId">this is for company id</param>
        /// <param name="filters">this contains the filter those need to apply over the list</param>
        /// <param name="searchText">this contains the searchText to filter the list</param>
        /// <param name="groupId">this contains the groupId to filter the list</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SeachFilterUsers(string companyName, Guid companyId, string filters, string searchText, string groupId, int pageIndex, int pageSize, int startPage, int endPage)
        {

            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = companyId,
                CompanyName = companyName,
                UserId = Guid.Empty,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };
            var result = FilterUserList(customerCompanyUserQueryParameter, groupId, filters, searchText, pageIndex, pageSize, startPage, endPage);

            //List<UserViewModel> x = new List<UserViewModel>();

            //foreach(var r in result.UserList) {
            //	if (filters.Contains(r.GroupList.ToString())) {
            //		x.Add(r);
            //	}
            //}

            //result.UserList = x;

            return PartialView("_UserList", result);
        }


        [HttpGet]
        public ActionResult DoesEmailAlreadyPresent(string EmailAddress)
        {
            var userViewModel =
                    ExecuteQuery<UserSearchQueryParameter, UserViewModel>(new UserSearchQueryParameter
                    {
                        Email = EmailAddress
                    });
            var checkEmail = userViewModel == null ? true : false;
            return Json(checkEmail, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// this is one used to the Company user Add/Edit form
        /// </summary>
        /// <param name="companyName">this is for company name of user</param>
        /// <param name="companyId">this is for company id</param>
        /// <param name="userId">this is for the user's user id</param>
        /// <param name="groupId">this is for group by which user is belongs</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddOrEditUser(String company, Guid companyId, Guid? userId, Guid? groupId)
        {

            var model = new UserViewModel();

            var userQueryParameter = new UserQueryParameter
            {
                CompanyId = companyId,
                CompanyName = company,
                UserId = userId == null ? Guid.Empty : userId.Value,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };

            var standardUserGroups = new StandardUserGroupViewModel();
            if (userId != null)
            {
                //added by neeraj
                standardUserGroups = ExecuteQuery<StandardUserGroupByUserIdQuery, StandardUserGroupViewModel>(new StandardUserGroupByUserIdQuery() { UserId = userId.ToString() });
            }
            model =
                ExecuteQuery<UserQueryParameter, UserViewModel>(userQueryParameter);

            var tempCheck = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery()
            {
                Id = model.Id.ToString()
            });


            var roleName = "StandardUser";
            if (!string.IsNullOrEmpty(model.SelectedCustomerType))
            {
                if (model.SelectedCustomerType.Equals("StandardUser"))
                    roleName = "StandardUser";
                else if (tempCheck.CustomUserRoleId != null)
                { roleName = "Custom"; model.SelectedCustomUserRole = tempCheck.CustomUserRoleId; }
                else  //if (model.CustomerAdmin || model.SelectedCustomerType.Equals("CustomerAdmin"))
                    roleName = "Administrator";
            }
            ViewBag.Role = roleName;

            List<GroupViewModel> groupViewModelList =
                ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter
                {
                    CompanyId = companyId,
                });

            model.DropDownForGroup = groupViewModelList.Select(c => new SelectListItem
            {
                Text = c.Title,
                Value = c.GroupId.ToString()
            });

            var customerTypes = new List<SelectListItem>
            {
                new SelectListItem {Value = "StandardUser", Text = "Standard User"},
                new SelectListItem {Value = "Administrator", Text = "Administrator"},
                new SelectListItem {Value = "Custom", Text = "Custom"}
            };

            var csr = ExecuteQuery<FetchAllRecordsQuery, List<CustomUserRoles>>(
                new FetchAllRecordsQuery
                { });


            var customUserRoles = new List<SelectListItem>();
            //var selectedCustomRoles = new List<>
            foreach (var r in csr)
            {
                customUserRoles.Add(new SelectListItem { Value = r.Id.ToString(), Text = r.Title });
            }

            model.CustomUserRoleSelectList = customUserRoles;

            model.CompanyId = companyId;
            model.CompanyName = company;
            foreach (var item in model.Roles)
            {
                switch (item.RoleName)
                {
                    case "StandardUser":
                        model.StandardUser = true;
                        break;
                    case "ContentAdmin":
                        model.ContentAdmin = true;
                        break;
                    case "CustomerAdmin":
                        model.CustomerAdmin = true;
                        break;
                    case "Reporter":
                        model.Reporter = true;
                        break;
                    case "PortalAdmin":
                        model.PortalAdmin = true;
                        break;
                    case "Publisher":
                        model.Publisher = true;
                        break;
                    case "UserAdmin":
                        model.UserAdmin = true;
                        break;
                    case "TrainingActivityAdmin":
                        model.TrainingActivityAdmin = true;
                        break;
                    case "TrainingActivityReporter":
                        model.TrainingActivityReporter = true;
                        break;
                    case "ManageVirtualMeetings":
                        model.ManageVirtualMeetings = true;
                        break;
                    case "ManageTags":
                        model.ManageTags = true;
                        break;
                    case "ManageActivityLog":
                        model.ManageActivityLog = true;
                        break;
                    case "ManageAutoWorkflow":
                        model.ManageAutoWorkflow = true;
                        break;
                    case "ManageReportSchedule":
                        model.ManageReportSchedule = true;
                        break;
                    default:
                        break;
                }

            }
            var trainigLabels = new List<string>();

            if (model.TrainingLabels != null)
            {
                foreach (var item in model.TrainingLabels.Split(','))
                {
                    var label = ExecuteQuery<FetchByNameQuery, TrainingLabelModel>(new FetchByNameQuery() { Name = item });
                    trainigLabels.Add(label.Name);
                }
                model.SelectedLabel = trainigLabels;
            }

            //to fill selected groups
            List<string> selectedGroups = new List<string>();
            foreach (var group in standardUserGroups.GroupList)
            {
                selectedGroups.Add(group.GroupId.ToString());
            }
            model.SelectedGroups = selectedGroups;


            model.CustomerTypesSelectList = customerTypes;
            model.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();
            model.RaceCodes = ExecuteQuery<GetAllRaceCodesQuery, List<RaceCodeViewModel>>(new GetAllRaceCodesQuery());
            var labels = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
            ViewBag.Labels = labels.OrderBy(c => c.Name).ToList();



            return PartialView("_AddNewUser", model);
        }


        [HttpPost]
        public ActionResult SaveCSVGroupUser(CompanyUserViewModel customerCompanyUserViewModel)
        {
            var csvFilePath = Server.MapPath(ConfigurationManager.AppSettings["UserCSVFile"]);
            if (!Directory.Exists(csvFilePath))
                Directory.CreateDirectory(csvFilePath);
            var command = new SaveCsvCustomerCompanyUserCommand
            {
                CompanyId = PortalContext.Current.UserCompany.Id,
                CsvFilePath = csvFilePath,
                CsvHttpPostedFile = customerCompanyUserViewModel.UserViewModel.UserCVSFile,
                UserId = customerCompanyUserViewModel.UserViewModel.Id,
                LastName = customerCompanyUserViewModel.UserViewModel.LastName,
                ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                PortalContext = PortalContext.Current
            };
            if (SessionManager.GetRolesForCurrentlyLoggedInUser().Count() > 0)
            {
                SessionManager.GetUserRoleOfCurrentlyLoggedInUser().ForEach(r => command.UserRoles.Add(r.ToString()));
                PortalContext.Override(customerCompanyUserViewModel.CompanyId);
            }
            else
            {
                command.UserRoles.AddRange(SessionManager.GetCustomerRolesOfCurrentlyLoggedInUser());
            }
            if (command.CsvHttpPostedFile == null)
            {
                NotifyError(string.Format("Uploading the CSV File has Failed", Environment.NewLine), "csvuploads");
            }
            else
            {

                var validation = ExecuteCommand(command);


                if (validation.Validation.Any())
                {
                    var error = "Please ensure that the format of your CSV file matches the following{0}" +
                                "-First name must be appropriate{0}" +
                                "-ID Number is optional {0}" +
                                "-Email address must be unique{0}" +
                                "-Password must be valid{0}" +
                                "-Mobile number must contain numbers ONLY{0}" +
                                "-Group name is required {0}" +
                                "-Employee number is optional {0}" +
                                "-Gender (male/female accepted) {0}";
                    if (PortalContext.Current.UserCompany.EnableRaceCode)
                    {
                        error += "-Race code (code/description accepted) {0}";
                    }
                    NotifyError(string.Format(error, Environment.NewLine), "csvuploads");
                }
            }
            var previousUrl = new Url(Request.UrlReferrer.ToString());
            ModelState.Clear();
            return Redirect(previousUrl.Value);
        }

        /// <summary>
        /// this is one is used to change the company user Status
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChangeCompanyUserStatus(Guid userId, string status, string type)
        {
            var updateProvisionalCompanyStatus = new UpdateProvisionalCompanyUserStatusCommand
            {
                ProvisionalCompanyUserId = userId,
                ProvisionalCompanyUserStatus = status == "true"
            };
            if (status == "true")
            {


                ExecuteCommand(new ChangeCompanyUserExpiryStatusCommandParameter
                {
                    UserId = userId,
                    IsExpiryStatus = false,
                    IsSelfSignUp = type == "Self-signup" ? true : false
                });

            }
            var response = ExecuteCommand(updateProvisionalCompanyStatus);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// this is used to delete the company user 
        /// </summary>
        /// <param name="userId">this is for the user's user id</param>
        /// <param name="companyId">this is for company id</param>
        /// <param name="companyName">this is for company name of user</param>
        /// <param name="groupId">this is for group by which user is belongs</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteCustomerCompanyUser(Guid userId, Guid companyId, String companyName, string groupId, string filters, string searchText, int pageIndex, int pageSize, int startPage, int endPage)
        {

            var deleteProvisionalCompanyCommand = new DeleteProvisionalCompanyUserCommandParameter
            {
                ProvisionalComapanyUserId = userId
            };
            var response = ExecuteCommand(deleteProvisionalCompanyCommand);
            if (response.Validation.Any())
            {
                NotifyError(response.Validation.First().Message);
                return Json(new { Status = 'F' }, JsonRequestBehavior.AllowGet);
            }
            if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                //Insert User Activity for Customer company User Delete
                var addActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription = "",
                    ActivityType = UserActivityEnum.DeleteCustomerUser,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };
                ExecuteCommand(addActivityCommand);
            }



            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = companyId,
                CompanyName = companyName,
                UserId = userId,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };
            var DeleteStandardUserGroupCommand = new DeleteStandardUserGroupCommand
            {
                UserId = userId
            };

            ExecuteCommand(DeleteStandardUserGroupCommand);

            var result = FilterUserList(customerCompanyUserQueryParameter, null, filters, searchText, pageIndex, pageSize, startPage, endPage);


            //CompanyUserViewModel result =
            //	ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);
            return PartialView("_UserList", result);

            //return RedirectToAction("Index");

        }
        /// <summary>
        /// this is is used to update or change the password
        /// </summary>
        /// <param name="userId">this is for the user's user id</param>
        /// <param name="companyId">this is for company id</param>
        /// <param name="companyName">this is for company name of user</param>
        /// <param name="groupId">this is for group by which user is belongs</param>
        /// <param name="passWord">this one for passwork that need to change</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdatePassword(Guid userId, Guid companyId, string companyName, string groupId, string passWord, string filters, string searchText, int pageIndex, int pageSize, int startPage, int endPage)
        {
            var changePasswordCommandParameter = new ChangePasswordCommandParameter
            {
                Id = userId,
                Password = passWord
            };
            var response = ExecuteCommand(changePasswordCommandParameter);

            if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                //Insert User Activity for Update Customer company
                var addActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription =
                        "Updated Password",
                    ActivityType = UserActivityEnum.ResetPassword,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };
                ExecuteCommand(addActivityCommand);
            }

            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = companyId,
                CompanyName = companyName,
                UserId = userId,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };

            var result = FilterUserList(customerCompanyUserQueryParameter, groupId, filters, searchText, pageIndex, pageSize, startPage, endPage);

            return PartialView("_UserList", result);
        }
        /// <summary>
        /// this contain the all logic related the pagination and all filtering
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filters"></param>
        /// <param name="searchText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="startPage"></param>
        /// <param name="endPage"></param>
        /// <returns></returns>

        [NonAction]
        public CompanyUserViewModel FilterUserList(CustomerCompanyUserQueryParameter query, string groupId, string filters, string searchText, int pageIndex, int pageSize, int startPage, int endPage)
        {

            var cus = ExecuteQuery<FetchAllRecordsQuery, List<CustomUserRoles>>(
                new FetchAllRecordsQuery
                { });

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string[] filterList = (string[])jss.Deserialize(filters, typeof(string[]));
            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(query);
            var type = companyUserViewModel.FilterCompanyCustomer.Where(c => c.Type == "Role").FirstOrDefault().FilterData.Where(c => c.Name == "Standard User" || c.Name == "Global Admin").ToList();
            type.Add(new FilterData { Name = "Managing Admin", Id = Guid.Empty.ToString() });
            companyUserViewModel.FilterCompanyCustomer = companyUserViewModel.FilterCompanyCustomer.Where(c => c.Type != "Role").ToList();

            companyUserViewModel.FilterCompanyCustomer.Add(new FilterCompanyCustomer { Type = "Role", FilterData = type });

            var roles = new List<string>();
            var status = new List<string>();
            var Departments = new List<string>();
            var selfSignup = new List<string>();
            var groups = new List<string>();
            var tags = new List<string>();
            if (filterList != null && filterList.Any())
            {
                foreach (var item in filterList)
                {
                    var t = item.Split('_')[1];
                    if (item.Split('_')[0] == "Role")
                    {
                        if (t == Guid.Empty.ToString())
                        {
                            roles.Add("Managing Admin");
                        }
                        else
                        {
                            try
                            {
                                roles.Add(ExecuteQuery<FindRoleByIdQuery, CustomerRole>(new FindRoleByIdQuery { Id = t }).Description);
                            }
                            catch (Exception ex)
                            {

                                foreach (var x in cus)
                                {
                                    if (t == x.Id.ToString())
                                    {
                                        roles.Add(x.Title);
                                    }
                                }
                            }
                        }
                    }
                    //if (item.Split('_')[0] == "Group") {
                    //	groups.Add(ExecuteQuery<FindGroupByIdQuery, CustomerGroup>(new FindGroupByIdQuery { Id = t }).Title);
                    //}

                    if (item.Split('_')[0] == "Status")
                    {
                        status.Add(t);
                    }
                    if (item.Split('_')[0] == "Sign Up Type")
                    {
                        selfSignup.Add(t);
                    }
                   
                    if (item.Split('_')[0] == "Tags")
                    {
                        tags.Add(ExecuteQuery<FetchByIdQuery, TrainingLabelModel>(new FetchByIdQuery() { Id = t }).Name);
                    }


                }
            }
            var userList = companyUserViewModel.UserList.Where(c => c.Id != query.LoggedInUserId).ToList();
            companyUserViewModel.UserList = userList;
            //if (roles.Count == 2) {

            //	companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.RoleName == "Global Admin" || c.RoleName == "Standard User").ToList();
            //} else if (roles.Count == 1) {
            //	if (roles.FirstOrDefault() == "Administrator") {
            //		companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.RoleName == "Global Admin").ToList();
            //	} else {
            //		companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.RoleName == "Standard User").ToList();
            //	}
            //}
            if (roles.Any())
            {
                var temp = companyUserViewModel.UserList.Where(c => roles.Contains(c.RoleName)).ToList();

                if (temp.Count == 0)
                {
                    foreach (var u in companyUserViewModel.UserList.Where(c => roles.Contains(c.RoleName)).ToList())
                    {
                        foreach (var r in cus)
                        {
                            if (u.RoleName == r.Title)
                            {
                                temp.Add(u);
                            }
                        }
                    }
                }
                companyUserViewModel.UserList = temp;
            }
            if (groups.Any())
            {
                companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => groups.Contains(c.GroupName)).ToList();
            }
            if (status.Count == 1)
            {
                if (status.FirstOrDefault() == "Enabled")
                    companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.IsActive).ToList();
                else
                    companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => !c.IsActive).ToList();
            }

    
            if (selfSignup.Count == 1)
            {
                //earlier 0 was "Other"
                if (selfSignup.FirstOrDefault() == "0")
                    companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => !c.IsFromSelfSignUp).ToList();
                else
                    companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.IsFromSelfSignUp).ToList();
            }
            if (tags.Any())
            {
                var ss = String.Join(",", tags);
                companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.TrainingLabels != null && c.TrainingLabels.Contains(ss)).ToList();

                //foreach (var user in companyUserViewModel.UserList.ToList()) {
                //	if (user.TrainingLabels != null) {
                //		var trainningLabels = user.TrainingLabels.Split(',');
                //		foreach (var trainningLabel in trainningLabels) {
                //			foreach (var tag in tags) {
                //				if (trainningLabel.Contains(tag)) {
                //					if (companyUserViewModel.UserList.Contains(user)) {
                //						companyUserViewModel.UserList.Remove(user);
                //					}
                //					companyUserViewModel.UserList.Add(user);
                //				}
                //			}
                //		}
                //	}
                //}
            }
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                if (!string.IsNullOrEmpty(searchText))
                {
                    companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.FullName.ToLower().Contains(searchText) || c.GroupName.ToLower().Contains(searchText) || c.EmailAddress.ToLower().Contains(searchText) || c.RoleName.ToLower().Contains(searchText) || c.SignupType.ToLower().Contains(searchText) || (c.TrainingLabels != null && c.TrainingLabels.ToLower().Contains(searchText))).ToList();
                }
            }
            //if (!string.IsNullOrEmpty(groupId)) {
            //	var group = ExecuteQuery<FindGroupByIdQuery, CustomerGroup>(new FindGroupByIdQuery { Id = groupId.Replace("_anchor", "").Trim() });
            //	if (group != null)
            //		companyUserViewModel.UserList = companyUserViewModel.UserList.Where(c => c.GroupName.Equals(group.Title)).ToList();
            //}


            foreach (var x in companyUserViewModel.UserList)
            {
                //added by neeraj
                var standardUserGroups = ExecuteQuery<StandardUserGroupByUserIdQuery, StandardUserGroupViewModel>(new StandardUserGroupByUserIdQuery() { UserId = x.Id.ToString() });

                string gr = null;
                List<StandardUserGroupModel> newList = new List<StandardUserGroupModel>();
                foreach (var g in standardUserGroups.GroupList)
                {
                    if (gr == null)
                    {
                        gr = gr + g.Title;

                    }
                    else
                    {
                        gr = gr + ", " + g.Title;

                    }
                    x.GroupList.Add(new GroupViewModelShort
                    {
                        Id = g.GroupId,
                        Name = g.Title,
                        Selected = false

                    });
                }

                x.GroupName = gr;

            }

            

            if (filterList != null && groups.Count>0)
            {
                List<UserViewModel> newUserList = new List<UserViewModel>();
                foreach (var x in companyUserViewModel.UserList.ToList())
                {
                    foreach (var g in x.GroupList)
                    {

                        foreach (var r in filterList)
                        {

                            if (r.Split('_')[0] == "Group")
                            {
                                if (r.Split('_')[1] == g.Id.ToString())
                                {
                                    newUserList.Add(x);
                                }
                            }
                            else
                            {
                                newUserList.Add(x);
                            }
                        }
                    }
                }
                companyUserViewModel.UserList = newUserList;
            }


          

            if (groupId != null && groupId != "")
            {
                List<UserViewModel> newUserList = new List<UserViewModel>();
                foreach (var x in companyUserViewModel.UserList.ToList())
                {
                    foreach (var g in x.GroupList)
                    {


                        if (groupId.Contains("anchor"))
                        {
                            if (groupId.Split('_')[1] == "anchor")
                            {
                                if (groupId.Split('_')[0] == Guid.Empty.ToString())
                                {
                                    newUserList = companyUserViewModel.UserList.ToList();
                                }
                                else if (groupId.Split('_')[0] == g.Id.ToString())
                                {
                                    newUserList.Add(x);
                                }
                            }
                        }
                    }
                }
                companyUserViewModel.UserList = newUserList;
            }

            ////code writte by neeraj end
            ///


            companyUserViewModel.Paginate.TotalItems = companyUserViewModel.UserList.Count;

            companyUserViewModel.Paginate.Page = (companyUserViewModel.UserList.Count / pageSize) == 0 ? (companyUserViewModel.UserList.Count / pageSize) : (companyUserViewModel.UserList.Count / pageSize) + 1;
            companyUserViewModel.UserList = companyUserViewModel.UserList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            companyUserViewModel.CompanyName = query.CompanyName;
            companyUserViewModel.CompanyId = query.CompanyId;
            companyUserViewModel.Paginate.PageIndex = pageIndex;
            companyUserViewModel.Paginate.IsFirstPage = pageIndex == 1 ? true : false;

            companyUserViewModel.Paginate.PageSize = pageSize;
            if (pageIndex > endPage && companyUserViewModel.Paginate.Page != pageIndex)
            {
                companyUserViewModel.Paginate.StartPage = startPage + 1;
                companyUserViewModel.Paginate.EndPage = endPage + 1;
                companyUserViewModel.Paginate.IsLastPage = false;
            }
            else if (companyUserViewModel.Paginate.Page == pageIndex)
            {
                companyUserViewModel.Paginate.StartPage = startPage + 1;
                companyUserViewModel.Paginate.EndPage = endPage + 1;
                companyUserViewModel.Paginate.IsLastPage = true;
            }
            else if (pageIndex == startPage && pageSize == 1)
            {
                companyUserViewModel.Paginate.StartPage = startPage;
                companyUserViewModel.Paginate.EndPage = endPage;
            }
            else if (pageIndex < startPage)
            {
                companyUserViewModel.Paginate.StartPage = startPage - 1;
                companyUserViewModel.Paginate.EndPage = endPage - 1;
            }
            else
            {
                companyUserViewModel.Paginate.StartPage = startPage;
                companyUserViewModel.Paginate.EndPage = endPage;
                companyUserViewModel.Paginate.IsLastPage = false;
                companyUserViewModel.Paginate.IsFirstPage = false;
            }
            if (companyUserViewModel.Paginate.Page < 7 && companyUserViewModel.Paginate.Page > 1)
            {
                companyUserViewModel.Paginate.StartPage = companyUserViewModel.Paginate.Page >= 1 ? 1 : 1;
                companyUserViewModel.Paginate.EndPage = companyUserViewModel.Paginate.Page >= 7 ? 7 : companyUserViewModel.Paginate.Page;

            }
            else if (companyUserViewModel.Paginate.Page == 0 || companyUserViewModel.Paginate.Page == 1)
            {
                companyUserViewModel.Paginate.StartPage = 1;
                companyUserViewModel.Paginate.EndPage = 1;
                companyUserViewModel.Paginate.IsLastPage = true;
                companyUserViewModel.Paginate.IsFirstPage = true;
            }
            if (companyUserViewModel.Paginate.PageIndex == 1)
            {
                companyUserViewModel.Paginate.FirstPage = 1;
                var records = companyUserViewModel.Paginate.PageIndex * companyUserViewModel.Paginate.PageSize;
                if (records <= companyUserViewModel.Paginate.TotalItems)
                {
                    companyUserViewModel.Paginate.LastPage = records;
                }
                else
                {
                    companyUserViewModel.Paginate.LastPage = companyUserViewModel.Paginate.TotalItems;
                }
            }
            else
            {
                var records = companyUserViewModel.Paginate.PageIndex * companyUserViewModel.Paginate.PageSize;
                companyUserViewModel.Paginate.FirstPage = ((companyUserViewModel.Paginate.PageIndex - 1) * companyUserViewModel.Paginate.PageSize) + 1;
                if (records <= companyUserViewModel.Paginate.TotalItems)
                {
                    companyUserViewModel.Paginate.LastPage = records;
                }
                else
                {
                    companyUserViewModel.Paginate.LastPage = companyUserViewModel.Paginate.TotalItems;
                }
            }
            if (filters.Length > 0)
            {
                filters.Replace("/", "");
            }



            return companyUserViewModel;
        }

        /// <summary>
        /// this is one is used to change the user detail in database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveOrUpdateUser(UserViewModel model)
        {

            if (model.SelectedCustomerType.Equals("Administrator") || model.SelectedCustomerType.Equals("StandardUser") && !model.SelectedCustomerType.Equals("Custom"))
            {
                model.SelectedCustomUserRole = null;
            }

            if (model.SelectedCustomerType.Equals("Administrator"))
            {
                model.GroupName = null;
                model.SelectedGroups = null;
            }


            Session["NEW_COMPANY_USER"] = true;
            if (model.SelectedLabel != null)
            {
                model.TrainingLabels = string.Join(",", model.SelectedLabel);
            }
            if (model.SelectedGroups != null)
            {
                model.SelectedGroupId = Guid.Parse(model.SelectedGroups[0]);
            }


            var standardUsergroup = new SaveOrUpdateStandardUserGroupCommand();

            //if (model.SelectedGroups != null) {
            //	var selectedGroups = string.Join(",", model.SelectedGroups);
            //}
            model.FullName = model.FullName.Replace("\"", string.Empty);
            var customerCompanyUserCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
            {
                UserId = model.Id,
                CompanyId = PortalContext.Current.UserCompany.Id,
                SelectedGroupId = model.SelectedGroups,
                EmailAddress = model.EmailAddress?.TrimAllCastToLowerInvariant(),
                FirstName = model.FullName.GetFirstName(),
                LastName = model.FullName.GetLastName(),
                MobileNumber = model.MobileNumber,
                ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                Password = model.Password,
                EmployeeNo = model.EmployeeNo,
                IsConfirmEmail = true,
                IDNumber = model.IDNumber,
                IsActive=model.IsActive,
                Gender = model.Gender,
                RaceCodeId = model.RaceCodeId,
                TrainingLabels = model.TrainingLabels,
                SelectedCustomUserRole = model.SelectedCustomUserRole
            };
            customerCompanyUserCommand.Roles = GetRolesFromViewModel(model);

            if (model.SelectedCustomerType.Equals("Custom") && !customerCompanyUserCommand.Roles.Contains("StandardUser"))
            {
                model.GroupName = null;
                customerCompanyUserCommand.SelectedGroupId = null;
            }

            if (model.Id != Guid.Empty)
            {
                Session["NEW_COMPANY_USER"] = false;

                if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                {
                    //Insert User Activity for update Customer company user
                    var addActivityCommand = new AddUserActivityCommand
                    {
                        ActivityDescription =
                            model.FullName,
                        ActivityType = UserActivityEnum.UpdateUserProfile,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    };
                    ExecuteCommand(addActivityCommand);

                    standardUsergroup = new SaveOrUpdateStandardUserGroupCommand
                    {
                        UserId = model.Id,
                        GroupId = model.SelectedGroups,
                        DateCreated = DateTime.Now
                    };

                    ExecuteCommand(standardUsergroup);

                    TempData["UserSaved"] = true;
                }
            }
            else
            {

                if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                {
                    //Insert User Activity for Create Customer company user
                    var addActivityCommand = new AddUserActivityCommand
                    {
                        ActivityDescription =
                            model.FullName,
                        ActivityType = UserActivityEnum.CreateCustomerUser,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    };
                    ExecuteCommand(addActivityCommand);
                    TempData["UserSaved"] = false;
                }
            }

            ExecuteCommand(customerCompanyUserCommand);




            //if (model.Id != null) {
            //	standardUsergroup = new SaveOrUpdateStandardUserGroupCommand {
            //		UserId = model.Id,
            //		GroupId = model.SelectedGroups,
            //		DateCreated = DateTime.Now
            //	};

            //	ExecuteCommand(standardUsergroup);
            //}

            ModelState.Clear();

            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = model.CompanyId,
                CompanyName = model.CompanyName,
                UserId = Guid.Empty,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };

            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);

            return RedirectToAction("index", new { companyId = model.CompanyId, userId = Guid.Empty, companyName = model.CompanyName, groupId = Guid.Empty });

        }

        private List<string> GetRolesFromViewModel(UserViewModel model)
        {
            var result = new List<string>();


            //  else if (model.CustomerAdmin || model.SelectedCustomerType.Equals("Administrator"))
            //	result.Add(Role.CustomerAdmin);
            if (model.SelectedCustomerType.Equals("Administrator"))
            {

                ////result.Add(Role.CustomerAdmin);
                //result.Add(Role.ContentAdmin);

                //if (model.CustomerAdmin)
                result.Add(Role.CustomerAdmin);
                //if (model.CategoryAdmin)
                result.Add(Role.CategoryAdmin);
                //if (model.ContentAdmin)
                result.Add(Role.ContentAdmin);
                //if (model.PortalAdmin)
                result.Add(Role.PortalAdmin);
                //if (model.Publisher)
                result.Add(Role.Publisher);
                //if (model.Reporter)
                result.Add(Role.Reporter);
                //if (model.UserAdmin)
                result.Add(Role.UserAdmin);
                //if (model.NotificationAdmin)
                result.Add(Role.NotificationAdmin);
                //if (model.TrainingActivityAdmin)
                result.Add(Role.TrainingActivityAdmin);
                //if (model.TrainingActivityReporter)
                result.Add(Role.TrainingActivityReporter);
                //if (model.ManageTags)
                result.Add(Role.ManageTags);
                //if (model.ManageVirtualMeetings)
                result.Add(Role.ManageVirtualMeetings);
                //if (model.ManageActivityLog)
                result.Add(Role.ManageActivityLog);

                result.Add(Role.ManageAutoWorkflow);
                result.Add(Role.ManageReportSchedule);
            }
            else if (model.SelectedCustomUserRole != null)
            {
                //neeraj
                //get thje selected cusomt user role id
                //fetch all roles linked to it, and run the above code as usual.
                var csr = ExecuteQuery<FetchByIdQuery, CustomUserRoles>(
                new FetchByIdQuery
                { Id = model.SelectedCustomUserRole });

                if (csr.StandardUser)
                {
                    result.Add(Role.StandardUser);
                }
                if (csr.ContentAdmin)
                {
                    result.Add(Role.ContentAdmin);
                }
                if (csr.ContentApprover)
                {
                    result.Add(Role.ContentApprover);
                }
                if (csr.ContentCreator)
                {
                    result.Add(Role.ContentCreator);
                }
                if (csr.ManageTags)
                {
                    result.Add(Role.ManageTags);
                }
                if (csr.ManageVirtualMeetings)
                {
                    result.Add(Role.ManageVirtualMeetings);
                }
                if (csr.PortalAdmin)
                {
                    result.Add(Role.PortalAdmin);
                }
                if (csr.Publisher)
                {
                    result.Add(Role.Publisher);
                }
                if (csr.Reporter)
                {
                    result.Add(Role.Reporter);
                }
                if (csr.UserAdmin)
                {
                    result.Add(Role.UserAdmin);
                }
                if (csr.ManageAutoWorkflow)
                {
                    result.Add(Role.ManageAutoWorkflow);
                }
                if (csr.ManageReportSchedule)
                {
                    result.Add(Role.ManageReportSchedule);
                }

            }
            else if (model.SelectedCustomerType.Equals("StandardUser"))
            {
                result.Add(Role.StandardUser);
            }
            return result;
        }

        public ActionResult DocumentPeakList(string id)
        {


            var model = ExecuteQuery<DocumentsAssignedToUserQuery, IEnumerable<AssignedDocumentListModel>>(new DocumentsAssignedToUserQuery()
            {
                UserId = id,
                CompanyId = Thread.CurrentPrincipal.GetCompanyId().ToString()
            }).ToList();

            var incomplete = model.Where(c => c.Status == AssignedDocumentStatus.UnderReview).ToList();
            var pending = model.Where(c => c.Status == AssignedDocumentStatus.Pending).ToList();
            var other = model.Where(c => c.Status != AssignedDocumentStatus.UnderReview && c.Status != AssignedDocumentStatus.Pending).ToList();
            var result = new List<AssignedDocumentListModel>();
            result.AddRange(pending);
            result.AddRange(incomplete);
            result.AddRange(other);
            model = result;
            return PartialView("_DocumentList", model);
        }

        [HttpGet]
        public string GetCustomUSerRolePermission(string roleId)
        {
            string permissions = null;

            var csr = ExecuteQuery<FetchAllRecordsQuery, List<CustomUserRoles>>(
                    new FetchAllRecordsQuery
                    { });

            foreach (var role in csr)
            {
                if (role.Id.ToString() == roleId)
                {
                    if (role.StandardUser)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Standard User";
                        }
                        else
                        {
                            permissions = "Standard User";
                        }
                    }
                    if (role.ContentAdmin)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Document Manager";
                        }
                        else
                        {
                            permissions = "Document Manager";
                        }

                    }
                    if (role.ContentApprover)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Content Approver";
                        }
                        else
                        {
                            permissions = "Content Approver";
                        }
                    }
                    if (role.ContentCreator)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Content Creator";
                        }
                        else
                        {
                            permissions = "Content Creator";
                        }
                    }
                    if (role.ManageTags)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Tag Manager";
                        }
                        else
                        {
                            permissions = "Tag Manager";
                        }
                    }
                    if (role.ManageVirtualMeetings)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Virtual Meetings Manager";
                        }
                        else
                        {
                            permissions = "Virtual Meetings Manager";
                        }
                    }
                    if (role.ManageAutoWorkflow)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Workflow Manager";
                        }
                        else
                        {
                            permissions = "Workflow Manager";
                        }
                    }
                    if (role.ManageReportSchedule)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Report Schedule Manager";
                        }
                        else
                        {
                            permissions = "Report Schedule Manager";
                        }
                    }
                    if (role.PortalAdmin)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Settings Manager";
                        }
                        else
                        {
                            permissions = "Settings Manager";
                        }
                    }
                    if (role.Publisher)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Document Sender";
                        }
                        else
                        {
                            permissions = "Document Sender";
                        }
                    }
                    if (role.Reporter)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", Report Manager";
                        }
                        else
                        {
                            permissions = "Report Manager";
                        }
                    }
                    if (role.UserAdmin)
                    {
                        if (permissions != null)
                        {
                            permissions = permissions + ", User Manager";
                        }
                        else
                        {
                            permissions = "User Manager";
                        }
                    }
                }
            }

            return permissions;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ImportUsers()
        {
            string response = "true";
            try
            {
                new ActiveDirectoryController().ImportAllUsersFromAD();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ImportGroups()
        {
            try
            {
                new ActiveDirectoryController().ImportAllGroupsFromAD();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}