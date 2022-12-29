using Data.EF;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.Settings;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.ActivityManagement;
using Ramp.Contracts.QueryParameter.CorrespondenceManagement;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Attributes;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Mvc;
using Common.Command;
using Ramp.Contracts.Query.Group;
using Ramp.Contracts.Query.User;
using Web.UI.Code.Extensions;
using Web.UI.Helpers;
using RampController = Web.UI.Controllers.RampController;
using Role = Ramp.Contracts.Security.Role;
using Ramp.Contracts.QueryParameter.Group;

namespace Web.UI.Areas.Configurations.Controllers
{
    public class ManagerUserController : RampController
    {
        private readonly MainContext db = new MainContext();

        //
        // GET: /Configurations/ManagerUser/
        public ActionResult Index()
        {
			return View();
        }

        //
        // GET: User Activity Details
        public ActionResult UserActivityDetails()
        {
            var userList = new List<UserModelShort>();
            var userActivityStatsViewModel = new UserActivityStatsViewModel();
            if (Thread.CurrentPrincipal.IsInAdminRole())
            {
                List<UserViewModel> allusers = ExecuteQuery<AllCustomerUserQueryParameter, List<UserViewModel>>(new AllCustomerUserQueryParameter
                {
                    LoginUserCompanyId = PortalContext.Current.UserCompany.Id
                });

                userActivityStatsViewModel.UserDropDown = allusers.Where(z=>z.IsActive).Select(c => new SerializableSelectListItem
                {
                    Text = c.FullName,
                    Value = c.Id.ToString()
                });
            }
            else if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                //When user logged in as reseller
                ProvisionalCompanyUserViewModel allCustomerCompaniesForProvisionalCompany =
                    ExecuteQuery<CustomerCompanyQueryParameter, ProvisionalCompanyUserViewModel>(
                        new CustomerCompanyQueryParameter
                        {
                            Id = PortalContext.Current.UserCompany.Id
                        });

                userActivityStatsViewModel.CustomerCompanyDropDown =
                    allCustomerCompaniesForProvisionalCompany.CompanyList.OrderBy(c => c.CompanyName).Select(c => new SerializableSelectListItem
                    {
                        Text = c.CompanyName,
                        Value = c.Id.ToString()
                    });
            }
            else
            {
                //When user logged in as Master layer Admin
                var allProvisionalCompanies =
                    ExecuteQuery<AllProvisionalUserQueryParameter, CompanyViewModel>(
                        new AllProvisionalUserQueryParameter());
                userActivityStatsViewModel.ProvisionalCompanyDropDown =
                    allProvisionalCompanies.CompanyList.OrderBy(c => c.Name).Select(c => new SerializableSelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    });
            }

            return View(userActivityStatsViewModel);

        }

        //
        // GET: User Activity Details
        public ActionResult UserCorrespondenceDetails()
        {

			

			var userActivityStatsViewModel = new UserActivityStatsViewModel();
            if (Thread.CurrentPrincipal.IsInAdminRole())
            {
                List<UserViewModel> allusers = ExecuteQuery<AllCustomerUserQueryParameter, List<UserViewModel>>(new AllCustomerUserQueryParameter
                {
                    LoginUserCompanyId = PortalContext.Current.UserCompany.Id
                });

                userActivityStatsViewModel.UserDropDown = allusers.Where(z=>z.IsActive).OrderBy(u => u.FirstName).Select(c => new SerializableSelectListItem
                {
                    Text = c.FullName,
                    Value = c.Id.ToString()
                });



                var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
                {
                    CompanyId = PortalContext.Current.UserCompany.Id,
                    CompanyName = PortalContext.Current.UserCompany.CompanyName,
                    UserId = Guid.Empty,
                    LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
                };
                CompanyUserViewModel companyUserViewModel =
                    ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);
                var datas = companyUserViewModel.UserList.Where(z => z.Department != null && z.IsActive).Select(z => z.Department).Distinct().ToList();
                ViewBag.Departments = datas;

                //userActivityStatsViewModel.Groups = ExecuteQuery<GroupsWithUsersQuery, IEnumerable<GroupViewModelShort>>(new GroupsWithUsersQuery());


                //ViewBag.Groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(
                //new AllGroupsByCustomerAdminQueryParameter {
                //	CompanyId = PortalContext.Current.UserCompany.Id
                //}).ToList();




                ViewBag.Groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(
				   new AllGroupsByCustomerAdminQueryParameter {
					   CompanyId = PortalContext.Current.UserCompany.Id
				   }).Select(g => new SerializableSelectListItem {
					   Value = g.GroupId.ToString(),
					   Text = g.Title
				   }).OrderBy(x => x.Text);


				userActivityStatsViewModel.Users = ExecuteQuery<AllUsersQuery, IEnumerable<UserModelShort>>(new AllUsersQuery()).Where(z=>z.IsActive);
                
            }
            else if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                //When user logged in as reseller
                ProvisionalCompanyUserViewModel allCustomerCompaniesForProvisionalCompany =
                    ExecuteQuery<CustomerCompanyQueryParameter, ProvisionalCompanyUserViewModel>(
                        new CustomerCompanyQueryParameter
                        {
                            Id = PortalContext.Current.UserCompany.Id
                        });

                userActivityStatsViewModel.CustomerCompanyDropDown =
                    allCustomerCompaniesForProvisionalCompany.CompanyList.OrderBy(c => c.CompanyName).Select(c => new SerializableSelectListItem
                    {
                        Text = c.CompanyName,
                        Value = c.Id.ToString()
                    });
            }
            else
            {
                //When user logged in as Master layer Admin
                var allProvisionalCompanies =
                    ExecuteQuery<AllProvisionalUserQueryParameter, CompanyViewModel>(
                        new AllProvisionalUserQueryParameter());
                userActivityStatsViewModel.ProvisionalCompanyDropDown =
                    allProvisionalCompanies.CompanyList.OrderBy(c => c.Name).Select(c => new SerializableSelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    });
            }

            return View(userActivityStatsViewModel);
        }

        //Used to get the Users Correspondence
        [HttpPost]
        public PartialViewResult GetUserCorrespondence(string provisionalcompanyId = "", string companyId = "", string groupId = "", string userId = "", string fromDate = "", string toDate = "", string selectedOption = "")
        {

			string[] groups = new string[] { };

			if (groupId != null) {
				groups = groupId.Split(',');
			}

            if (!string.IsNullOrEmpty(companyId))
                ExecuteCommand(new UpdateConnectionStringCommand
                {
                    CompanyId = companyId
                });
            var date = new DateTime();

            var queryParameter = new AllUserCorrespondenceInTimeSpanQueryParameter
            {
                ProvisionalCompanyId = provisionalcompanyId != "" ? Guid.Parse(provisionalcompanyId) : Guid.Empty,
                CompanyId = companyId != "" ? Guid.Parse(companyId) : Guid.Empty,
               // GroupId = groupId != "" ? Guid.Parse(groupId) : Guid.Empty,
                UserId = userId != "" ? Guid.Parse(userId) : Guid.Empty,
                FromDate = DateTime.TryParse(fromDate, out date) ? date : new DateTime(),
                ToDate = DateTime.TryParse(toDate, out date) ? date : new DateTime()
            };
            if (selectedOption.Equals(AllUserCorrespondenceInTimeSpanQueryParameter.ProvisionalUsers))
                queryParameter.SelectedOption = AllUserCorrespondenceInTimeSpanQueryParameter.ProvisionalUsers;
            else if (selectedOption.Equals(AllUserCorrespondenceInTimeSpanQueryParameter.CompanyUsers))
                queryParameter.SelectedOption = AllUserCorrespondenceInTimeSpanQueryParameter.CompanyUsers;

            if (Thread.CurrentPrincipal.IsInAdminRole())
                queryParameter.CompanyId = PortalContext.Current.UserCompany.Id;

            List<UserCorrespondenceLogViewModel> list = ExecuteQuery
                <AllUserCorrespondenceInTimeSpanQueryParameter, List<UserCorrespondenceLogViewModel>>(
                  queryParameter);

			if (groups[0] != null && groups[0] != "") {

					var xr = list.Where(u => groups.Contains(u.UserViewModel.GroupName)).ToList();
	
					list = xr;
				
			}

            ExecuteCommand(new UpdateConnectionStringCommand());

            return PartialView(list);
        }

        public ActionResult CorrespondenceDetails(string id = "")
        {
            Guid correspondenceId;

            if (!Guid.TryParse(id, out correspondenceId))
                correspondenceId = new Guid();

            var query = new UserCorrespondenceWithIdQueryParameter { Id = correspondenceId };
            var result = ExecuteQuery<UserCorrespondenceWithIdQueryParameter, UserCorrespondenceLogViewModel>(query);

            return PartialView(result);
        }

        [HttpPost]
        public PartialViewResult GetUserActivities(string userId, string fromDate, string toDate, string selectedOption, string provisionalCompanyId, string companyId)
        {
            var queryParameter = new AllUserActivitiesInTimeSpanQueryParameter
            {
                FromDate = !string.IsNullOrWhiteSpace(fromDate) ? Convert.ToDateTime(fromDate) : default(DateTime),
                ToDate = !string.IsNullOrWhiteSpace(toDate) ? Convert.ToDateTime(toDate) : default(DateTime),
                UserId = !string.IsNullOrWhiteSpace(userId) ? Guid.Parse(userId) : Guid.Empty,
                SelectedOption = selectedOption,
                ProvisionalCompanyId = !string.IsNullOrWhiteSpace(provisionalCompanyId) ? Guid.Parse(provisionalCompanyId) : Guid.Empty,
                CompanyId = !string.IsNullOrWhiteSpace(companyId) ? Guid.Parse(companyId) : Guid.Empty
            };

            return
                PartialView(ExecuteQuery<AllUserActivitiesInTimeSpanQueryParameter, List<UserActivityLogViewModel>>
                (queryParameter));
        }

        [HttpPost]
        public PartialViewResult GetUserAndCompanyList(string type)
        {
            if (type == "optionAllCompany")
            {
                ViewBag.PageType = "AllCompany";
                var emptyQueryParameter = new EmptyQueryParameter();
                ManageUserAndCompanyViewModel result =
                    ExecuteQuery<EmptyQueryParameter, ManageUserAndCompanyViewModel>(
                        emptyQueryParameter);
                return PartialView("_GetUserAndCompanyListPartial", result);
            }
            else
            {
                ViewBag.PageType = "AllUsers";
                var emptyQueryParameter = new EmptyQueryParameter();
                ManageUserAndCompanyViewModel result =
                    ExecuteQuery<EmptyQueryParameter, ManageUserAndCompanyViewModel>(
                        emptyQueryParameter);
                return PartialView("_GetUserAndCompanyListPartial", result);
            }
        }

        public ActionResult CustomerCompaniesLinkedToProvisionalUser()
        {
            CompanyViewModel provisonalCompanyList =
                ExecuteQuery<AllProvisionalUserQueryParameter, CompanyViewModel>(new AllProvisionalUserQueryParameter());
            provisonalCompanyList.Companies =
                provisonalCompanyList.CompanyList.OrderBy(c => c.Name).Select(c => new SerializableSelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
            return View(provisonalCompanyList);
        }

        [HttpPost]
        public PartialViewResult CustomerCompanyLinkedToProvisionalUserPartial(Guid companyId)
        {
            var customerUserQueryParameter = new CustomerCompanyQueryParameter
            {
                Id = companyId
            };
            var model = ExecuteQuery<CustomerCompanyQueryParameter, ProvisionalCompanyUserViewModel>(
                        customerUserQueryParameter);
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult UpdateStatus(string status, string id, string type)
        {
            var userId = new Guid(id);
            if (status == "Active")
            {
                if (type == "Company")
                {
                    List<Company> user = db.Company.Where(em => em.Id == userId).ToList();
                    if (user.Count > 0)
                    {
                        user[0].IsActive = false;
                        db.SaveChanges();
                    }
                }
                else
                {
                    List<User> user = db.Users.Where(em => em.Id == userId).ToList();
                    if (user.Count > 0)
                    {
                        user[0].IsActive = false;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                if (type == "Company")
                {
                    List<Company> user = db.Company.Where(em => em.Id == userId).ToList();
                    if (user.Count > 0)
                    {
                        user[0].IsActive = true;
                        db.SaveChanges();
                    }
                }
                else
                {
                    List<User> user = db.Users.Where(em => em.Id == userId).ToList();
                    if (user.Count > 0)
                    {
                        user[0].IsActive = true;
                        db.SaveChanges();
                    }
                }
            }

            NotifyInfo("Status successfully updated");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdatePassword(ChangePasswordViewModel model)
        {
            var changePasswordCommandParameter = new ChangePasswordCommandParameter
            {
                Id = model.UserId,
                Password = model.Password
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

            NotifyInfo("Password successfully updated");

            Uri redirectUrl = Request.UrlReferrer;

            return Redirect(redirectUrl.SetQueryStringParameter("userId", Guid.Empty.ToString()).ToString());
        }

        public ActionResult KpiReport(UserKpiReportQueryParameter query)
        {
            if (query.CustomerCompanyId != null)
                PortalContext.Override(query.CustomerCompanyId.Value);

            query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole() ? new Guid?() : PortalContext.Current.UserCompany.Id;

            var vm = ExecuteQuery<UserKpiReportQueryParameter, UserKpiReportViewModel>(query);
            return View(vm);
        }
    }
}