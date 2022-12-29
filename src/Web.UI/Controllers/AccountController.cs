using Common.Events;
using Data.EF;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Ramp.Contracts.Command.CheckList;
using Ramp.Contracts.Command.Memo;
using Ramp.Contracts.Command.Policy;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.Command.TrainingManual;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Login;
using Ramp.Contracts.Events.Account;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.RecycleBinQuery;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.QueryParameter.Account;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.Login;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.UI.Code;
using Web.UI.Code.Extensions;
using Web.UI.Models;
using WebGrease.Css.Extensions;
using static Web.UI.AppSettings;
using Role = Ramp.Contracts.Security.Role;

namespace Web.UI.Controllers
{
    public class AccountController : RampController
    {
        private OnRampSignInManager _signInManager;
        private OnRampUserManager _userManager;
        private string _urlAfterLogin = null;
        public byte[] NotificationHeaderLogo { get; set; }
        public byte[] NotificationFooterLogo { get; set; }

        public OnRampSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<OnRampSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public OnRampUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<OnRampUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private IList<ResetPasswordToken> ResetTokens
        {
            get
            {
                var key = "Password+ResetTokens";
                var tokens = ControllerContext.HttpContext.Cache.Get(key);
                if (tokens == null)
                    ControllerContext.HttpContext.Cache.Add(key, new List<ResetPasswordToken>(), null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration,
                        System.Web.Caching.CacheItemPriority.Default, null);

                return (IList<ResetPasswordToken>)ControllerContext.HttpContext.Cache.Get(key);
            }
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //return Redirect(Url.Action("SelectLoginRole", "Account"));

            if (PortalContext.Current != null && PortalContext.Current.UserCompany != null)
                PortalContext.Current.UserCompany.CustomConfiguration = ExecuteQuery<GetCustomConfigurationQueryParameter, CustomConfigurationViewModel>(new GetCustomConfigurationQueryParameter());
            var loginModel = new LoginUserCommandParameter();
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["AllowPortalContextOverrideOnLogin"]))
            {
                Session["DEV"] = true;
                loginModel.Companies =
                               ExecuteQuery<AllCustomerCompanyQueryParameter, CompanyViewModel>(new AllCustomerCompanyQueryParameter()).CompanyList.Select(
                                   x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            }

            if (TempData["IsApproved"] != null)
                ViewData["Message"] = TempData["IsApproved"].ToString();
            else
                ViewData["Message"] = null;

            if (TempData["Success"] != null)
                ViewData["Success"] = TempData["Success"].ToString();
            else
                ViewData["Success"] = null;


            ModelStateDictionary message = ModelState;
            var identity = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (identity != null)
            {
                var roles = SessionManager.GetRolesForCurrentlyLoggedInUser().ToList();
                if (roles.Contains("StandardUser") && roles.Count > 1)
                {
                    return Redirect(Url.Action("SelectLoginRole", "Account"));
                }
                else
                {
                    if (returnUrl != null && roles.Any())
                        return Redirect(Url.Content(Request.QueryString["ReturnUrl"]));
                    if (Role.IsInGlobalAdminRole(roles) || Role.IsInResellerRole(roles))
                        return Redirect(Url.Action("Index", "Home"));
                    else if (Role.IsInAdminRole(roles))
                        return Redirect(Url.Action("Administrator", "Home"));
                    else if (Role.IsInStandardUserRole(roles))
                        return Redirect(Url.Action("StandardUser", "Home"));
                    this._urlAfterLogin = Request.QueryString["ReturnUrl"];
                }
            }
            return View(loginModel);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult SelectLoginRole()
        {
            return View("SelectLoginRole");
        }

        [HttpPost]
        public ActionResult SelectLoginRole(string role)
        {
            return View("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginUserCommandParameter model, string returnUrl)
        {

            if (ModelState.IsValid)
            {

                if (Convert.ToBoolean(Session["DEV"]))
                {
                    PortalContext.Override(Guid.Parse(model.SelectedCompany));
                }
                
                model.PortalContext = PortalContext.Current;
                                
                var IsActiveDirectoryAuthenticated = false;
              
               
                if (PortalContext.Current != null && PortalContext.Current.UserCompany != null)
                    PortalContext.Current.UserCompany.CustomConfiguration = ExecuteQuery<GetCustomConfigurationQueryParameter, CustomConfigurationViewModel>(new GetCustomConfigurationQueryParameter());

                IsActiveDirectoryAuthenticated = new ActiveDirectoryController().CheckUserEmail(model.Email, model.Password, PortalContext.Current.UserCompany.Id.ToString());
                Session["LDAP_xb"] = IsActiveDirectoryAuthenticated;
                var response = ExecuteCommand(model);
                if (response.Validation.Any() && !IsActiveDirectoryAuthenticated)
                    response.Validation.ForEach(e => Session[e.MemberName] = e.Message);
                else
                {
                    var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
                    if (result != SignInStatus.Success)
                        Session["LOGIN_FAILED"] = "Invalid username or password.";
                    else
                    {
                        _urlAfterLogin = Request.UrlReferrer?.Query?.Replace("?returnUrl=", "~");
                        if (!string.IsNullOrWhiteSpace(_urlAfterLogin))
                        {
                            if (!_urlAfterLogin.StartsWith("~"))
                                _urlAfterLogin = "~/" + _urlAfterLogin;
                            return Redirect(Urls.ResolveUrl(_urlAfterLogin));
                        }
                        if (!(Thread.CurrentPrincipal is ClaimsPrincipal identity))
                            Session["LOGIN_FAILED"] = "Invalid username or password.";
                        else
                        {
                            var roles = identity.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value).ToList();
                            var id = identity.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
                            Guid gId = Guid.Empty;
                            if (!Guid.TryParse(id, out gId))
                                Session["LOGIN_FAILED"] = "Invalid username or password.";
                            else
                            {
                                var userRoles = PortalContext.Current.UserDetail.CustomUserRoleId;
                                if (true)
                                {
                                    if (Request.Url != null)
                                    {
                                        var path = Request.Url.GetLeftPart(UriPartial.Authority) +
                                                   Url.Content(@"~/LogoImages/ProvisionalLogo/"
                                                               + PortalContext.Current.UserCompany.LogoImageUrl);
                                        PortalContext.Current.LogoFileName = path;
                                    }
                                    if (Ramp.Contracts.Security.Role.IsInResellerRole(roles) || Ramp.Contracts.Security.Role.IsInGlobalAdminRole(roles))
                                        return RedirectToAction("Index", "Home");
                                    else if (Ramp.Contracts.Security.Role.IsInStandardUserRole(roles) || Ramp.Contracts.Security.Role.IsInAdminRole(roles))
                                    {

                                        var logins = ExecuteQuery<UserLoginHistoryQueryParameter, LoginStatsViewModel>(new UserLoginHistoryQueryParameter
                                        {
                                            UserId = gId
                                        });
                                        if (PortalContext.Current.UserCompany.IsChangePasswordFirstLogin && logins.Count == 1)
                                            return RedirectToAction("ChangeYourPassword", "Account");
                                        if (Role.IsInStandardUserRole(roles))
                                        {
                                            return RedirectToAction("StandardUser", "Home");
                                        }
                                        return RedirectToAction("Administrator", "Home");
                                    }
                                    else
                                    {
                                        //TODO:DO NOT DELETE THIS CODE.Remove the comment for customer domain login
                                        if (PortalContext.Current.UserCompany != null &&
                                            (HttpContext.Request.UrlReferrer != null && HttpContext.Request.UrlReferrer.Host.Contains(PortalContext.Current.UserCompany.LayerSubDomain.ToLower()))
                                           )
                                            return RedirectToAction("StandardUser", "Home");

                                        SessionManager.RemoveSessionInformation();
                                        return View(model);
                                    }
                                }
                            }
                        }
                    }
                }

                return RedirectToAction("Login", "Account");
            }
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["AllowPortalContextOverrideOnLogin"]))
            {
                Session["DEV"] = true;

                model.Companies =
                ExecuteQuery<AllCustomerCompanyQueryParameter, CompanyViewModel>(new AllCustomerCompanyQueryParameter()).CompanyList.Select(
                    x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            }
            return View(model);
        }

        public ActionResult LogOff()
        {
            try
            {
                //Update data for UserLoginStats
                var userLoginStats = new AddUserLoginStatsCommand
                {
                    IsUserLoggedIn = true,
                    LoggedInUserId = Thread.CurrentPrincipal.GetId(),
                    LogOutTime = DateTime.Now,
                    UserLoginStatsId = Thread.CurrentPrincipal.GetStatsId()
                };
                if (Thread.CurrentPrincipal.IsInStandardUserRole() || Thread.CurrentPrincipal.IsInAdminRole())
                    userLoginStats.StandardUser = true;
                ExecuteCommand(userLoginStats);

                if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                {
                    //Insert User Activity

                    var addUserActivityCommand = new AddUserActivityCommand
                    {
                        ActivityDescription = "User LogOff",
                        ActivityType = UserActivityEnum.Logout,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    };
                    ExecuteCommand(addUserActivityCommand);
                }


            }
            catch (Exception) { }
            SessionManager.RemoveSessionInformation();
            Session.Abandon();

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ChangeYourPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ChangeYourPassword(ManageUserViewModel model)
        {
            //Update New Password
            var resetPasswordCommand = new ResetPasswordCommand
            {
                Password = model.NewPassword,
                Id = SessionManager.GetCurrentlyLoggedInUserId()
            };
            var response = ExecuteCommand(resetPasswordCommand);

            //Insert data for UserLoginStats
            Guid userLoginStatsId = Guid.NewGuid();
            var userLoginStatsNew = new AddUserLoginStatsCommand
            {
                UserLoginStatsId = userLoginStatsId,
                IsUserLoggedIn = false,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                LogInTime = DateTime.Now,
            };
            if (SessionManager.GetCustomerRolesOfCurrentlyLoggedInUser().Count > 0)
            {
                userLoginStatsNew.StandardUser = true;
            }
            ExecuteCommand(userLoginStatsNew);

            //Set Session Values
            SessionManager.SetSessionInformation(new SessionInformation());

            if (response.ErrorMessage == null)
            {
                NotifySuccess("Password Successfully changed");
                return Redirect(Url.Action("StandardUser", "Home"));
            }
            else
            {
                ViewBag.Message = "Error, password has not been changed";
            }
            return View();
        }

        [HttpGet]
        public ActionResult RecycleBinDocuments(Guid? id)
        {

            var model = GetRecycleDocs();

            return PartialView("_RecylceBinDocuments", model);
        }

        [HttpGet]
        public ActionResult RestoreDocuments(string id)
        {

            var restoreId = id.Split('_');
            switch (restoreId[1])
            {
                case "Memo":
                    var memo = ExecuteCommand(new RestoreMemoCommand { Id = restoreId[0] });
                    break;
                case "Checklist":
                    var checklist = ExecuteCommand(new RestoreChecklistCommand { Id = restoreId[0] });
                    break;
                case "Test":
                    var test = ExecuteCommand(new RestoreTestCommand { Id = restoreId[0] });
                    break;
                case "Policy":
                    var policy = ExecuteCommand(new RestorePolicyCommand { Id = restoreId[0] });
                    break;
                case "TrainingManual":
                    var manual = ExecuteCommand(new RestoreManualCommand { Id = restoreId[0] });
                    break;
                default:
                    break;
            }

            var model = GetRecycleDocs();

            return PartialView("_RecylceBinDocuments", model);
        }
        /// <summary>
        /// this is used to get last week deleted documents
        /// </summary>
        /// <returns></returns>
        public List<DocumentListModel> GetRecycleDocs()
        {
            var model = new List<DocumentListModel>();
            DateTime lastWeek = DateTime.UtcNow.AddDays(-7.0).Date;
            var memoList = ExecuteQuery<RecycleMemoQuery, IEnumerable<DocumentListModel>>(new RecycleMemoQuery());
            var manualList = ExecuteQuery<RecycleManualQuery, IEnumerable<DocumentListModel>>(new RecycleManualQuery());
            var testList = ExecuteQuery<RecycleTestQuery, IEnumerable<DocumentListModel>>(new RecycleTestQuery());
            var policyList = ExecuteQuery<RecyclePolicyQuery, IEnumerable<DocumentListModel>>(new RecyclePolicyQuery());
            var checklistList = ExecuteQuery<RecycleChecklistQuery, IEnumerable<DocumentListModel>>(new RecycleChecklistQuery()).Where(c => c.LastEditDate.Value.Date >= lastWeek).ToList();

            model.AddRange(memoList.Where(c => c.LastEditDate.Value.Date >= lastWeek));
            model.AddRange(manualList.Where(c => c.LastEditDate.Value.Date >= lastWeek));
            model.AddRange(testList.Where(c => c.LastEditDate.Value.Date >= lastWeek));
            model.AddRange(policyList.Where(c => c.LastEditDate.Value.Date >= lastWeek));
            model.AddRange(checklistList.Where(c => c.LastEditDate.Value.Date >= lastWeek));
            return model;
        }

        [HttpGet]
        public ActionResult EditMyProfile(Guid? id)
        {
            MainContext _mainContext = new MainContext();

            if (PortalContext.Current != null)
            {
                var fileUploads = _mainContext.CustomerConfigurationSet.Where(c => c.Company.Id == PortalContext.Current.UserCompany.Id && (c.Type == CustomerConfigurationType.NotificationFooterLogo || c.Type == CustomerConfigurationType.NotificationHeaderLogo)).ToList();
                if (fileUploads.Any())
                {
                    NotificationHeaderLogo = fileUploads.FirstOrDefault(c => c.Type == CustomerConfigurationType.NotificationHeaderLogo).Upload != null ? fileUploads.FirstOrDefault(c => c.Type == CustomerConfigurationType.NotificationHeaderLogo).Upload.Data : null;
                }
            }

            var result = new EditUserProfileViewModel();
            if (id.HasValue)
            {
                result =
                    ExecuteQuery<UserProfileQuery, EditUserProfileViewModel>(new UserProfileQuery() { UserId = id.Value });

                if (result == null)
                {
                    return HttpNotFound();
                }
            }

            return PartialView("_EditUserProfilePartial", result);
        }

        [HttpPost]

        public ActionResult EditMyProfile(EditUserProfileViewModel model, string returnUrl)
        {
            ExecuteCommand(new UpdateMyProfileCommandParameter
            {
                EditUserProfileViewModel = model
            });
            if (SessionManager.GetUserRoleOfCurrentlyLoggedInUser().Contains(UserRole.Admin))
            {
                // TODO: REDIRECT TO ADMIN DASHBOARD
                return Redirect(Url.Action("Index", "Home"));
            }
            // TODO: REDIRECT TO STANDARDUSER DASHBOARD
            return Redirect(Url.Action("StandardUser", "Home"));
        }

        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        [AllowAnonymous]
        public ActionResult LostPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LostPassword(LostPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Email = model.Email.TrimAllCastToLowerInvariant();
                var userViewModel =
                    ExecuteQuery<UserSearchQueryParameter, UserViewModel>(new UserSearchQueryParameter
                    {
                        Email = model.Email.ToLowerInvariant()
                    });

                if (userViewModel != null)
                {
                    var token = new EncryptionHelper().Encrypt($"{PortalContext.Current.UserCompany.Id}:{userViewModel.Id}");
                    ResetTokens.Add(new ResetPasswordToken
                    {
                        UserName = userViewModel.EmailAddress,
                        Token = token
                    });

                    new EventPublisher().Publish(new LostPasswordEvent
                    {
                        Company = PortalContext.Current.UserCompany,
                        ResetToken = token,
                        User = userViewModel,
                        Subject = LostPasswordEvent.DefaultSubject
                    });
                }
            }
            TempData["Success"] = "Check your email for verification link";
            NotifyInfo("Check your email for verification link");

            return Redirect(Url.Action("Login"));
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string rt)
        {
            if (ResetTokens.FirstOrDefault(c => c.Token == rt) == null)
            {
                NotifyError("Page has expired i.e used or invalid link");
                return Redirect(Url.Action("Login"));
            }
            if (rt == null)
            {
                NotifyError("Page has expired i.e used or invalid link");
                return Redirect(Url.Action("Login"));
            }
            return View(new ResetPasswordViewModel { ReturnToken = rt });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var token = ResetTokens.FirstOrDefault(c => c.Token == model.ReturnToken);
                if (token == null)
                {
                    NotifyError("Page has expired i.e used or invalid link");
                    return RedirectToAction("Login");
                }
                ResetTokens.Remove(token);
                var response = ExecuteCommand(new ResetPasswordCommand
                {
                    Password = model.Password,
                    Token = token.Token
                });
                if (response.Validation.Any())
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ConfirmEmail(string token, string email, Guid companyId)
        {
            PortalContext.Override(companyId);
            var users = ExecuteQuery<AllCustomerUserQueryParameter, List<UserViewModel>>(new AllCustomerUserQueryParameter
            {
                LoginUserCompanyId = companyId
            });
            if (users.Any(u => u.EmailAddress.Equals(email.ToLowerInvariant().Trim())))
            {
                var user = users.FirstOrDefault(u => u.EmailAddress.Equals(email.ToLowerInvariant().Trim()));
                var updateUserCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
                {
                    CompanyId = user.CompanyId,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ParentUserId = user.ParentUserId,
                    MobileNumber = user.MobileNumber,
                    EmployeeNo = user.EmployeeNo,
                    IsConfirmEmail = true,
                    IDNumber = user.IDNumber
                };
                updateUserCommand.Roles.Add(Role.CustomerAdmin);
                ExecuteCommand(updateUserCommand);
                var updateProvisionalCompanyStatus = new UpdateSelfProvisionEmailConfirm
                {
                    EmailId = email.ToLowerInvariant().Trim()
                };

                ExecuteCommand(updateProvisionalCompanyStatus);

                NotifySuccess("Email verified successfuly!");
            }
            else
            {
                NotifyError("Email verification failed");
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public string checkSelfSignupCompany()
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["AllowPortalContextOverrideOnLogin"]))
            {
                return string.Empty;
            }
            if (PortalContext.Current.UserCompany.IsForSelfSignUp)
                return PortalContext.Current.UserCompany.Id.ToString();
            return string.Empty;
        }
        [HttpGet]
        public ActionResult KeepAlive()
        {
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}

//        [AllowAnonymous]
//        public ActionResult SessionExpired()
//        {
//            Uri returnUrltoaction = Request.UrlReferrer;
//            FormsAuthentication.SignOut();
//            return RedirectToAction("Login", "Account", new { returnUrl = returnUrltoaction });
//        }