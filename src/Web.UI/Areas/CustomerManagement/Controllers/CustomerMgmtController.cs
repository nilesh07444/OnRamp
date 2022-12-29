using CaptchaMvc.HtmlHelpers;
using Common;
using Data.EF;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using Ramp.Contracts.CommandParameter.Settings;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.IconSet;
using Ramp.Contracts.QueryParameter.PackageManagement;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.CommandParameter.GuideManagement;
using Ramp.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Web.Mvc;
using Common.Command;
using Domain.Customer.Models;
using Ramp.Contracts.Command.Company;
using Ramp.Contracts.Command.StandardUser;
using Ramp.Contracts.Query.Bundle;
using Ramp.Contracts.Query.StandardUser;
using Ramp.Contracts.Query.User;
using Web.UI.Code.Cache;
using Web.UI.Code.Extensions;
using RampController = Web.UI.Controllers.RampController;
using Role = Ramp.Contracts.Security.Role;
using Ramp.Contracts.CommandParameter.Group;
using Common.Query;
using Ramp.Services.Helpers;
using Ramp.Contracts.CommandParameter.CustomUserRole;
using System.Web.Script.Serialization;
using Ramp.Contracts.CommandParameter;
using Domain.Customer.Models.CustomRole;
using Domain.Customer.Models.ScheduleReport;
using Ramp.Contracts.CommandParameter.ScheduleReport;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.QueryParameter.Reporting;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.Query.Label;
using Newtonsoft.Json;
using Ramp.Contracts.CommandParameter.CustomFields;
using Domain.Customer.Models.Custom_Fields;
using Ramp.Contracts.Query.CustomFields;

namespace Web.UI.Areas.CustomerManagement.Controllers
{
    public class CustomerMgmtController : RampController
    {
        private readonly MainContext db = new MainContext();

        public ActionResult Index(Guid? id)
        {

            //string thirdlast = secondlast.Split('/').Last()
            Session["IS_COMPANY_EDITED"] = false;
            var customerCompanyQueryParameter = new CustomerCompanyQueryParameter();

            if (id != null && id != Guid.Empty)
            {
                Session["IS_COMPANY_EDITED"] = true;
                customerCompanyQueryParameter.Id = id;
            }
            if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                customerCompanyQueryParameter.IsForAdmin = true;
            }
            else if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                if (PortalContext.Current.UserCompany.CompanyType == Domain.Enums.CompanyType.CustomerCompany)
                {
                    customerCompanyQueryParameter.ProvisionalCompanyId = PortalContext.Current.UserCompany.ProvisionalAccountLink;
                }
                else
                {
                    customerCompanyQueryParameter.ProvisionalCompanyId =
                        PortalContext.Current.UserCompany.Id;
                }
            }
            else
            {
                customerCompanyQueryParameter.ProvisionalCompanyId =
                   PortalContext.Current.UserCompany.ProvisionalAccountLink;
            }

            CompanyViewModelLong customerCompanyViewModel =
                ExecuteQuery<CustomerCompanyQueryParameter, CompanyViewModelLong>(customerCompanyQueryParameter);

            //Bind 'Provisional Account Link'
            var dummyModel = new CompanyModelShort
            {
                Name = "Select Provisional Company",
                Id = Guid.Empty
            };
            CompanyViewModel provisonalCompanyList =
                ExecuteQuery<ProvisionalAccountQueryParameter, CompanyViewModel>(new ProvisionalAccountQueryParameter());
            if (customerCompanyViewModel.CompanyViewModel == null)
            {
                customerCompanyViewModel.CompanyViewModel = new CompanyViewModel();
            }
            customerCompanyViewModel.CompanyViewModel.CompanyList.Insert(0, dummyModel);
            customerCompanyViewModel.CompanyViewModel.Companies =
                provisonalCompanyList.CompanyList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            PackageViewModel packageList =
                ExecuteQuery<PackageQueryParameter, PackageViewModel>(new PackageQueryParameter());
            var bundleList = ExecuteQuery<BundleQuery, BundleViewModel>(new BundleQuery());

            customerCompanyViewModel.CompanyViewModel.Bundles =
                bundleList.BundleViewModelList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Title,
                    Value = c.Id
                });
            customerCompanyViewModel.CompanyViewModel.IconSets = ExecuteQuery<IconSetListQuery, IEnumerable<IconSetModel>>(new IconSetListQuery()).Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id
            });
            if (string.IsNullOrEmpty(customerCompanyViewModel.CompanyViewModel.SelectedIconSet))
                customerCompanyViewModel.CompanyViewModel.SelectedIconSet = customerCompanyViewModel.CompanyViewModel.IconSets.FirstOrDefault(x => x.Text == "DEFAULT")?.Value;
            // customerCompanyViewModel.CompanyViewModel.ExpiryDate = DateTime.Now;
            return View(customerCompanyViewModel);
        }

        [HttpPost]
        public JsonResult ChangeCompanyStatus(Guid companyId, string status)
        {
            var updateProvisionalCompanyStatus = new UpdateProvisionalCompanyStatusCommand
            {
                ProvisionalCompanyId = companyId,
                ProvisionalCompanyStatus = status == "true",
                CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };
            ExecuteCommand(updateProvisionalCompanyStatus);
            return null;
        }


        [HttpPost]
        public ActionResult DeleteLogo(string id)
        {
            var deleteDashboardLogo = false;
            var deleteLoginLogo = false;
            var deleteFooterLogo = false;

            if (id.Equals("DashboardLogo"))
                deleteDashboardLogo = true;
            if (id.Equals("FooterLogo"))
                deleteFooterLogo = true;
            if (id.Equals("LoginLogo"))
                deleteLoginLogo = true;

            ExecuteCommand(new SaveCustomConfigurationCommand
            {
                CompanyId = PortalContext.Current.UserCompany.Id.ToString(),
                DeleteFooterLogo = deleteFooterLogo,
                DeleteDashboardLogo = deleteDashboardLogo,
                DeleteLoginLogo = deleteLoginLogo

            });
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult SaveSelfSignUpForCompany(string companyId, bool isForSelfSignUp, bool isSelfSignUpApprove, bool isEmployeeCodeReq, bool IsEnabledEmployeeCode)
        {
            var response = ExecuteCommand(new UpdateCompanySelfSignUpSettingsCommand
            {
                Id = Guid.Parse(companyId),
                IsForSelfSignUp = isForSelfSignUp,
                IsSelfSignUpApprove = isSelfSignUpApprove,
                IsEmployeeCodeReq = isEmployeeCodeReq,
                IsEnabledEmployeeCode = IsEnabledEmployeeCode
            });

            var previousUrl = new Url(Request.UrlReferrer.ToString());
            ModelState.Clear();
            return Redirect(previousUrl.Value);
        }

        [HttpPost]
        public ActionResult CreateOrUpdateCustomerCompany(CompanyViewModelLong customerCompanyViewModel)
        {
            if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                if (Session["IS_COMPANY_EDITED"] == null || !Convert.ToBoolean(Session["IS_COMPANY_EDITED"]))
                {
                    //PackageViewModel packages =
                    //ExecuteQuery<PackageQueryParameter, PackageViewModel>(new PackageQueryParameter());
                    BundleViewModel bundles = ExecuteQuery<BundleQuery, BundleViewModel>(new BundleQuery());
                    customerCompanyViewModel.CompanyViewModel.SelectedBundle = bundles.BundleViewModelList.SingleOrDefault(p => p.Title.TrimAllCastToLowerInvariant().Equals("onramp5")).Id;
                    ModelState.Remove("CompanyViewModel.SelectedBundle");
                    customerCompanyViewModel.CompanyViewModel.ExpiryDate = DateTime.UtcNow.AddDays(30);
                }
                var iconSets = ExecuteQuery<IconSetListQuery, IEnumerable<IconSetModel>>(new IconSetListQuery());
                customerCompanyViewModel.CompanyViewModel.SelectedIconSet = iconSets.FirstOrDefault(x => x.Master == true)?.Id;
                ModelState.Remove("CompanyViewModel.SelectedIconSet");
            }
            ModelState.Remove("CompanyList");
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();
            if (ModelState.IsValid)
            {
                string companyConnStringOld = ConfigurationManager.ConnectionStrings["CustomerContext"].ConnectionString;
                string companyConnString = companyConnStringOld.Replace("DBNAME",
                    "" + customerCompanyViewModel.CompanyViewModel.CompanyName.Replace(" ", string.Empty));

                try
                {
                    if (Thread.CurrentPrincipal.IsInResellerRole())
                    {
                        if (Session["IS_COMPANY_EDITED"] == null || !Convert.ToBoolean(Session["IS_COMPANY_EDITED"]))
                        {
                            customerCompanyViewModel.CompanyViewModel.SelectedProvisionalAccountLink =
                            PortalContext.Current.UserCompany.Id;
                        }
                    }
                }
                catch (Exception)
                {
                    RedirectToAction("Index");
                }

                var saveOrUpdateCustomerCompanyCommand = new SaveOrUpdateCustomerCompanyCommand
                {
                    Id = customerCompanyViewModel.CompanyViewModel.Id,
                    CreatedBy = Thread.CurrentPrincipal.GetId(),
                    CompanyName = customerCompanyViewModel.CompanyViewModel.CompanyName.RemoveSpecialCharacters(),
                    LayerSubDomain = customerCompanyViewModel.CompanyViewModel.LayerSubDomain,
                    PhysicalAddress = customerCompanyViewModel.CompanyViewModel.PhysicalAddress,
                    PostalAddress = customerCompanyViewModel.CompanyViewModel.PostalAddress,
                    TelephoneNumber = customerCompanyViewModel.CompanyViewModel.TelephoneNumber,
                    WebsiteAddress = customerCompanyViewModel.CompanyViewModel.WebsiteAddress,
                    SelectedProvisionalAccountLink =
                        customerCompanyViewModel.CompanyViewModel.SelectedProvisionalAccountLink,
                    LogoImageUrl = customerCompanyViewModel.CompanyViewModel.LogoImageUrl,
                    ClientSystemName = customerCompanyViewModel.CompanyViewModel.ClientSystemName,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    //SelectedPackage = customerCompanyViewModel.CompanyViewModel.SelectedPackage,
                    SelectedBundle = customerCompanyViewModel.CompanyViewModel.SelectedBundle,
                    IsChangePasswordFirstLogin = customerCompanyViewModel.CompanyViewModel.IsChangePasswordFirstLogin,
                    IsSendWelcomeSMS = customerCompanyViewModel.CompanyViewModel.IsSendWelcomeSMS,
                    IsLock = customerCompanyViewModel.CompanyViewModel.IsLock,
                    ExpiryDate = customerCompanyViewModel.CompanyViewModel.ExpiryDate.HasValue ? customerCompanyViewModel.CompanyViewModel.ExpiryDate : new DateTime?(),
                    AutoExpire = customerCompanyViewModel.CompanyViewModel.AutoExpire,
                    YearlySubscription = customerCompanyViewModel.CompanyViewModel.YearlySubscription,
                    DefaultUserExpireDays = Convert.ToInt32(customerCompanyViewModel.CompanyViewModel.DefaultUserExpireDays),
                    ShowCompanyNameOnDashboard = customerCompanyViewModel.CompanyViewModel.ShowCompanyNameOnDashboard,
                    IconSet = customerCompanyViewModel.CompanyViewModel.SelectedIconSet,
                    EnableTrainingActivityLoggingModule = customerCompanyViewModel.CompanyViewModel.EnableTrainingActivityLoggingModule,
                    EnableRaceCode = customerCompanyViewModel.CompanyViewModel.EnableRaceCode,
                    EnableChecklistDocument = customerCompanyViewModel.CompanyViewModel.EnableChecklistDocument,
                    EnableCategoryTree = customerCompanyViewModel.CompanyViewModel.EnableCategoryTree,
                    EnableGlobalAccessDocuments = customerCompanyViewModel.CompanyViewModel.EnableGlobalAccessDocuments,
                    EnableVirtualClassRoom = customerCompanyViewModel.CompanyViewModel.EnableVirtualClassRoom,
                    JitsiServerName = customerCompanyViewModel.CompanyViewModel.JitsiServerName,

                    ActiveDirectoryEnabled = customerCompanyViewModel.CompanyViewModel.ActiveDirectoryEnabled,
                    Domain = customerCompanyViewModel.CompanyViewModel.Domain,
                    Port = customerCompanyViewModel.CompanyViewModel.Port,
                    UserName = customerCompanyViewModel.CompanyViewModel.UserName,
                    Password = customerCompanyViewModel.CompanyViewModel.Password

                };
                ExecuteCommand(saveOrUpdateCustomerCompanyCommand);

                if (customerCompanyViewModel.CompanyViewModel.CompanyLogo != null)
                    customerCompanyViewModel.CompanyViewModel.LogoImageUrl = "clear";

                ExecuteCommand(new SaveCustomConfigurationCommand
                {
                    CompanyId = customerCompanyViewModel.CompanyViewModel.Id == Guid.Empty ? saveOrUpdateCustomerCompanyCommand.Id.ToString() : customerCompanyViewModel.CompanyViewModel.Id.ToString(),
                    DashboardLogo = customerCompanyViewModel.CompanyViewModel.CompanyLogo,
                    LoginLogo = customerCompanyViewModel.CompanyViewModel.LoginLogo,
                    FooterLogo = customerCompanyViewModel.CompanyViewModel.FooterLogo,
                    NotificationHeaderLogo = customerCompanyViewModel.CompanyViewModel.NotificationHeaderLogo,
                    NotificationFooterLogo = customerCompanyViewModel.CompanyViewModel.NotificationFooterLogo,
                    DeleteDashboardLogo = customerCompanyViewModel.CompanyViewModel.DeleteDashboardLogo,
                    DeleteLoginLogo = customerCompanyViewModel.CompanyViewModel.DeleteLoginLogo,
                    DeleteFooterLogo = customerCompanyViewModel.CompanyViewModel.DeleteFooterLogo,
                    DeleteNotificationFooterLogo = customerCompanyViewModel.CompanyViewModel.DeleteNotificationFooterLogo,
                    DeleteNotificationHeaderLogo = customerCompanyViewModel.CompanyViewModel.DeleteNotificationHeaderLogo
                });

                if (customerCompanyViewModel.CompanyViewModel.Id == Guid.Empty)
                {
                    if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                    {
                        //Insert User Activity for Create Customer company
                        var addUserActivityCommand = new AddUserActivityCommand
                        {
                            ActivityDescription =
                                customerCompanyViewModel.CompanyViewModel.CompanyName.Trim(),
                            ActivityType = UserActivityEnum.CreateCustomerCompany,
                            CurrentUserId = Thread.CurrentPrincipal.GetId(),
                            ActivityDate = DateTime.Now
                        };
                        ExecuteCommand(addUserActivityCommand);
                    }
                }
                else
                {
                    if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                    {
                        //Insert User Activity for Update Customer company
                        var addActivityCommand = new AddUserActivityCommand
                        {
                            ActivityDescription =
                                customerCompanyViewModel.CompanyViewModel.CompanyName.Trim(),
                            ActivityType = UserActivityEnum.UpdateCompanyProfile,
                            CurrentUserId = Thread.CurrentPrincipal.GetId(),
                            ActivityDate = DateTime.Now
                        };
                        ExecuteCommand(addActivityCommand);
                    }
                }

                NotifySuccess("Customer saved");
                return RedirectToAction("Index");
            }
            var customerCompanyQueryParameter = new CustomerCompanyQueryParameter();

            if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                customerCompanyQueryParameter.IsForAdmin = true;
            }
            else if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                customerCompanyQueryParameter.ProvisionalCompanyId =
                    PortalContext.Current.UserCompany.Id;
            }
            else
            {
                customerCompanyQueryParameter.ProvisionalCompanyId =
                   PortalContext.Current.UserCompany.ProvisionalAccountLink;
            }

            CompanyViewModelLong tempCompanyViewModel =
                ExecuteQuery<CustomerCompanyQueryParameter, CompanyViewModelLong>(customerCompanyQueryParameter);
            customerCompanyViewModel.CompanyList = tempCompanyViewModel.CompanyList;

            //Bind 'Provisional Account Link'
            var dummyModel = new CompanyModelShort
            {
                Name = "Select Provisional Company",
                Id = Guid.Empty
            };
            CompanyViewModel provisonalCompanyList =
                ExecuteQuery<ProvisionalAccountQueryParameter, CompanyViewModel>(new ProvisionalAccountQueryParameter());
            if (customerCompanyViewModel.CompanyViewModel == null)
            {
                customerCompanyViewModel.CompanyViewModel = new CompanyViewModel();
            }
            customerCompanyViewModel.CompanyViewModel.CompanyList.Insert(0, dummyModel);
            customerCompanyViewModel.CompanyViewModel.Companies =
                provisonalCompanyList.CompanyList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            //PackageViewModel packageList =
            //    ExecuteQuery<PackageQueryParameter, PackageViewModel>(new PackageQueryParameter());
            BundleViewModel bundleList = ExecuteQuery<BundleQuery, BundleViewModel>(new BundleQuery());

            customerCompanyViewModel.CompanyViewModel.Bundles =
                bundleList.BundleViewModelList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Title,
                    Value = c.Id.ToString()
                });

            return View("Index", customerCompanyViewModel);
        }

        public ActionResult ViewCustomerSurvey()
        {
            CompanyViewModel companyModel =
                ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(new AllCustomerUserQueryParameter
                {
                    LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList()
                });
            companyModel.Companies =
                companyModel.CompanyList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
            return View(companyModel);
        }

        //Used to get the all or selected company Survey Statatics
        [HttpPost]
        public PartialViewResult CompanySurveyData(string customerCompanyId, string fromDate, string toDate)
        {
            try
            {
                List<CompanyViewModel> customerCompanyList;
                if (customerCompanyId != "")
                {
                    PortalContext.Override(Guid.Parse(customerCompanyId));
                    var queryParameter = new AverageRatingByCustomerCompanyQueryParameter
                    {
                        FromDate = Convert.ToDateTime(fromDate),
                        ToDate = Convert.ToDateTime(toDate)
                    };
                    queryParameter.CompanyIds.Add(Guid.Parse(customerCompanyId));
                    customerCompanyList = ExecuteQuery
                        <AverageRatingByCustomerCompanyQueryParameter, List<CompanyViewModel>>(queryParameter);
                }
                else
                {
                    CompanyViewModel companyModel =
                        ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(new AllCustomerUserQueryParameter
                        {
                            LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList()
                        });
                    var queryParameter = new AverageRatingByCustomerCompanyQueryParameter
                    {
                        FromDate = Convert.ToDateTime(fromDate),
                        ToDate = Convert.ToDateTime(toDate)
                    };
                    foreach (CompanyModelShort companyModelShort in companyModel.CompanyList)
                    {
                        queryParameter.CompanyIds.Add(companyModelShort.Id);
                    }
                    customerCompanyList = ExecuteQuery
                        <AverageRatingByCustomerCompanyQueryParameter, List<CompanyViewModel>>(queryParameter);
                }
                return PartialView(customerCompanyList);
            }
            catch (Exception)
            {
            }
            return null;
        }

        //Used to get the all or selected company Survey Statatics
        [HttpPost]
        public PartialViewResult CompanyUsersRatingData(string customerCompanyId, string fromDate, string toDate)
        {
            var queryParameter = new AverageRatingByCustomerCompanyQueryParameter
            {
                FromDate = Convert.ToDateTime(fromDate),
                ToDate = Convert.ToDateTime(toDate)
            };
            queryParameter.CompanyIds.Add(Guid.Parse(customerCompanyId));
            List<UserViewModel> customerCompanyUserList = ExecuteQuery
                <AverageRatingByCustomerCompanyQueryParameter, List<UserViewModel>>(queryParameter);

            return PartialView(customerCompanyUserList);
        }

        public ActionResult CompanyUserHistoryratingsDataToCSV(string customerCompanyId, string fromDate, string toDate)
        {
            try
            {
                var queryParameter = new AverageRatingByCustomerCompanyQueryParameter
                {
                    FromDate = Convert.ToDateTime(fromDate),
                    ToDate = Convert.ToDateTime(toDate)
                };
                queryParameter.CompanyIds.Add(Guid.Parse(customerCompanyId));
                List<UserViewModel> customerCompanyUserList = ExecuteQuery
                    <AverageRatingByCustomerCompanyQueryParameter, List<UserViewModel>>(queryParameter);
                var allUserRatings = new List<CustomerSurveyDetailViewModel>();

                foreach (var user in customerCompanyUserList)
                {
                    allUserRatings.AddRange(ExecuteQuery<UsersRatingHistoryByUserIdQueryParameter, List<CustomerSurveyDetailViewModel>>(new UsersRatingHistoryByUserIdQueryParameter
                    {
                        UserId = user.Id,
                        FromDate = Convert.ToDateTime(fromDate),
                        ToDate = Convert.ToDateTime(toDate)
                    }));
                }

                StringWriter sw = new StringWriter();

                sw.WriteLine(@"Date,UserName,Rating,Comment,Browser");

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment;filename=CompanySurveyStatsReport.csv");
                Response.ContentType = "text/csv";

                foreach (var line in allUserRatings)
                {
                    sw.WriteLine(string.Format(@"{0},{1},{2},{3},{4}",
                                               line.RatedOn,
                                               line.User.FullName.Trim(),
                                               line.Rating,
                                               line.Comment,
                                               line.Browser));
                }

                Response.Write(sw.ToString());

                Response.End();
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        //Get the all history of User Rating Statatics
        [HttpPost]
        public PartialViewResult CompanyUsersHistoryRatingData(string userId, string fromDate, string toDate)
        {
            var usersRatingHistoryByUserIdQueryParameter = new UsersRatingHistoryByUserIdQueryParameter
            {
                UserId = Guid.Parse(userId),
                FromDate = Convert.ToDateTime(fromDate),
                ToDate = Convert.ToDateTime(toDate)
            };
            List<CustomerSurveyDetailViewModel> customerUserRatingHistory =
                ExecuteQuery<UsersRatingHistoryByUserIdQueryParameter, List<CustomerSurveyDetailViewModel>>(
                    usersRatingHistoryByUserIdQueryParameter);

            return PartialView(customerUserRatingHistory);
        }

        [HttpPost]
        public JsonResult ChangeCompanyUserStatus(Guid userId, string status)
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
                    IsExpiryStatus = false
                });
            }
            var response = ExecuteCommand(updateProvisionalCompanyStatus);
            return null;
        }

        [HttpPost]
        public JsonResult DeleteCustomerCompanyUser(Guid userId)
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

            NotifyError("Successfully deleted");
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public ActionResult SaveCSVCustomerCompanyUser(CompanyUserViewModel customerCompanyUserViewModel)
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

        private List<string> GetRolesFromViewModel(UserViewModel model)
        {
            var result = new List<string>();

            if (model.SelectedCustomerType.Equals("StandardUser"))
                result.Add(Role.StandardUser);
            else if (model.CustomerAdmin)
                result.Add(Role.CustomerAdmin);
            else
            {
                if (model.CategoryAdmin)
                    result.Add(Role.CategoryAdmin);
                if (model.ContentAdmin)
                    result.Add(Role.ContentAdmin);
                if (model.PortalAdmin)
                    result.Add(Role.PortalAdmin);
                if (model.Publisher)
                    result.Add(Role.Publisher);
                if (model.Reporter)
                    result.Add(Role.Reporter);
                if (model.UserAdmin)
                    result.Add(Role.UserAdmin);
                if (model.NotificationAdmin)
                    result.Add(Role.NotificationAdmin);
                if (model.TrainingActivityAdmin)
                    result.Add(Role.TrainingActivityAdmin);
                if (model.TrainingActivityReporter)
                    result.Add(Role.TrainingActivityReporter);
            }
            return result;
        }
        [HttpPost]
        public ActionResult CreateOrUpdateCustomerCompanyUser(CompanyUserViewModel customerCompanyUserViewModel)
        {
            PortalContext.Override(customerCompanyUserViewModel.CompanyId);
            Session["NEW_COMPANY_USER"] = true;
            customerCompanyUserViewModel.UserViewModel.FullName = customerCompanyUserViewModel.UserViewModel.FullName.Replace("\"", string.Empty);
            var customerCompanyUserCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
            {
                UserId = customerCompanyUserViewModel.UserViewModel.Id,
                CompanyId = customerCompanyUserViewModel.CompanyId,
                EmailAddress = customerCompanyUserViewModel.EmailAddress.TrimAllCastToLowerInvariant(),
                FirstName = customerCompanyUserViewModel.UserViewModel.FullName.GetFirstName(),
                LastName = customerCompanyUserViewModel.UserViewModel.FullName.GetLastName(),
                MobileNumber = customerCompanyUserViewModel.UserViewModel.MobileNumber,
                ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                Password = customerCompanyUserViewModel.UserViewModel.Password,
                UserRoleId = customerCompanyUserViewModel.UserViewModel.SelectedCustomerUserRole,
                EmployeeNo = customerCompanyUserViewModel.UserViewModel.EmployeeNo,
                IsConfirmEmail = true,
                IDNumber = customerCompanyUserViewModel.UserViewModel.IDNumber,
                Gender = customerCompanyUserViewModel.UserViewModel.Gender,
                RaceCodeId = customerCompanyUserViewModel.UserViewModel.RaceCodeId
            };

            customerCompanyUserCommand.Roles = GetRolesFromViewModel(customerCompanyUserViewModel.UserViewModel);
            if (customerCompanyUserViewModel.UserViewModel.SelectedGroupId != null)
                //customerCompanyUserCommand.SelectedGroupId =
                //	(Guid)customerCompanyUserViewModel.UserViewModel.SelectedGroupId;

                if (customerCompanyUserViewModel.UserViewModel.Id != Guid.Empty)
                {
                    Session["NEW_COMPANY_USER"] = false;
                    if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                    {
                        //Insert User Activity for Updated Customer company user
                        var addActivityCommand = new AddUserActivityCommand
                        {
                            ActivityDescription =
                                customerCompanyUserViewModel.UserViewModel.FullName.GetFirstName().Trim(),
                            ActivityType = UserActivityEnum.UpdateProfile,
                            CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                            ActivityDate = DateTime.Now
                        };
                        ExecuteCommand(addActivityCommand);
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
                                customerCompanyUserViewModel.UserViewModel.FullName.Trim(),
                            ActivityType = UserActivityEnum.CreateCustomerUser,
                            CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                            ActivityDate = DateTime.Now
                        };
                        ExecuteCommand(addActivityCommand);
                    }
                }
            var response = ExecuteCommand(customerCompanyUserCommand);

            var previousUrl = new Url(Request.UrlReferrer.ToString());

            var url = previousUrl.Value.Split('=');

            var userId = url[2].Split('&');
            var finalUserId = Guid.Empty + "&" + userId[1];
            var returnURL = url[0] + "=" + url[1] + "=" + finalUserId + "=" + url[3];
            ModelState.Clear();

            NotifySuccess("User saved");
            Response.Redirect(returnURL);

            return null;
        }

        [HttpPost]
        public ActionResult CloseSurveyModal()
        {
            Session["ShowSurveyModal"] = false;
            return new HttpStatusCodeResult(200);
        }

        public ActionResult CustomerUserSurvey()
        {
            //Session["ShowSurveyModal"] = true;
            //var previousUrl = new Url(Request.UrlReferrer.ToString());
            //Response.Redirect(previousUrl.Value);
            //return null;

            return PartialView("CustomerUserSurvey", new CustomerSurveyDetailViewModel());

        }

        [HttpPost]
        public ActionResult CustomerSurvey(string comment, int rating, string browser, string category)
        {

            string _smtpFrom = ConfigurationManager.AppSettings["SMTPFrom"];
            string emailTo = ConfigurationManager.AppSettings["emailAdd"];
            if (rating != 0)
            {
                var addSurveyDetailsByCustomerUserCommand = new AddSurveyDetailsByCustomerUserCommand
                {
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    Comment = comment,
                    Rating = rating,
                    Browser = browser,
                    Category = category
                };
                ExecuteCommand(addSurveyDetailsByCustomerUserCommand);
            }

            var user = new UserViewModel();

            var userQueryParameter = new UserQueryParameter
            {
                UserId = SessionManager.GetCurrentlyLoggedInUserId()
            };

            user =
                ExecuteQuery<UserQueryParameter, UserViewModel>(userQueryParameter);

            var _emailAdd = user.EmailAddress;

            var msg = new SendEmail();
            var result = "<span>Portal: " + @PortalContext.Current.UserCompany.PhysicalAddress + ".onramp.training</span> " + "<br><br><span>User: " + @Thread.CurrentPrincipal.GetGivenName() + "</span> " + "<br><br><span> Report Type: " + category + "</span> " + "<br><br><span> Message: " + comment + "</span> ";
            msg.SendMessage(new List<string> { emailTo }, null, null, null, null, null, _smtpFrom, _emailAdd,
                "User's Feedback", result, null, _emailAdd);

            //Session["ShowSurveyModal"] = false;
            //return RedirectToAction("StandardUser", "Home", new { area = "" });
            //var previousUrl = new Url(Request.UrlReferrer.ToString());
            //Response.Redirect(previousUrl.Value);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateOrUpdateUserForSpecificCustomerCompany(
             CompanyUserViewModel customerCompanyUserViewModel)
        {
            Session["NEW_COMPANY_USER"] = true;
            customerCompanyUserViewModel.UserViewModel.FullName = customerCompanyUserViewModel.UserViewModel.FullName.Replace("\"", string.Empty);
            var customerCompanyUserCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
            {
                UserId = customerCompanyUserViewModel.UserViewModel.Id,
                CompanyId = PortalContext.Current.UserCompany.Id,
                //SelectedGroupId = customerCompanyUserViewModel.UserViewModel.SelectedGroupId,
                EmailAddress = customerCompanyUserViewModel.UserViewModel.EmailAddress?.TrimAllCastToLowerInvariant(),
                FirstName = customerCompanyUserViewModel.UserViewModel.FullName.GetFirstName(),
                LastName = customerCompanyUserViewModel.UserViewModel.FullName.GetLastName(),
                MobileNumber = customerCompanyUserViewModel.UserViewModel.MobileNumber,
                ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                Password = customerCompanyUserViewModel.UserViewModel.Password,
                EmployeeNo = customerCompanyUserViewModel.UserViewModel.EmployeeNo,
                IsConfirmEmail = true,
                IDNumber = customerCompanyUserViewModel.UserViewModel.IDNumber,
                Gender = customerCompanyUserViewModel.UserViewModel.Gender,
                RaceCodeId = customerCompanyUserViewModel.UserViewModel.RaceCodeId
            };
            customerCompanyUserCommand.Roles = GetRolesFromViewModel(customerCompanyUserViewModel.UserViewModel);

            if (customerCompanyUserViewModel.UserViewModel.Id != Guid.Empty)
            {
                Session["NEW_COMPANY_USER"] = false;
                if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
                {
                    //Insert User Activity for update Customer company user
                    var addActivityCommand = new AddUserActivityCommand
                    {
                        ActivityDescription =
                            customerCompanyUserViewModel.UserViewModel.FullName,
                        ActivityType = UserActivityEnum.UpdateUserProfile,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    };
                    ExecuteCommand(addActivityCommand);
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
                            customerCompanyUserViewModel.UserViewModel.FullName,
                        ActivityType = UserActivityEnum.CreateCustomerUser,
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        ActivityDate = DateTime.Now
                    };
                    ExecuteCommand(addActivityCommand);
                }
            }

            ExecuteCommand(customerCompanyUserCommand);
            ModelState.Clear();
            Session["EditForm"] = false;
            NotifySuccess("User saved");
            var url = "ManageOwnCustomerCompanyUser?companyId=" + customerCompanyUserViewModel.CompanyId + "&UserId=" + Guid.Empty.ToString() +
                "&companyName=" + customerCompanyUserViewModel.CompanyName;
            Response.Redirect(url);
            return null;
        }

        public ActionResult CustomerCompanyUser(Guid companyId, Guid userId, String companyName)
        {
            var customerType = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Value = Ramp.Contracts.Security.Role.StandardUser,
                    Text = "Standard User"
                },
                new SelectListItem()
                {
                    Value = "Administrator",
                    Text = "Administrator"
                }
            };
            PortalContext.Override(companyId);
            if (userId == Guid.Empty)
            {
                Session["EditForm"] = false;
            }
            else
            {
                Session["EditForm"] = true;
            }
            Session["IS_USER_EDITED"] = false;
            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = companyId,
                UserId = userId
            };

            //Bind Group DropDown
            List<GroupViewModel> groupViewModelList =
                ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter
                {
                    CompanyId = companyId
                });
            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);

            if (companyUserViewModel.UserViewModel != null)
            {
                companyUserViewModel.UserViewModel.CustomerTypesSelectList = customerType;
                Session["Editing_Email"] = companyUserViewModel.UserViewModel.EmailAddress;
                companyUserViewModel.UserViewModel.DropDownForGroup =
                    groupViewModelList.Select(c => new SelectListItem
                    {
                        Text = c.Title,
                        Value = c.GroupId.ToString()
                    });
                companyUserViewModel.UserViewModel.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();
                companyUserViewModel.UserViewModel.RaceCodes = ExecuteQuery<GetAllRaceCodesQuery, List<RaceCodeViewModel>>(new GetAllRaceCodesQuery());
                ExtractUserRolesToViewModel(companyUserViewModel.UserViewModel);
            }
            else
            {
                companyUserViewModel.UserViewModel = new UserViewModel
                {
                    DropDownForGroup = groupViewModelList.Select(c => new SelectListItem
                    {
                        Text = c.Title,
                        Value = c.GroupId.ToString()
                    }),
                    GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList()
                };
                companyUserViewModel.UserViewModel.CustomerTypesSelectList = customerType;
                companyUserViewModel.UserViewModel.RaceCodes = ExecuteQuery<GetAllRaceCodesQuery, List<RaceCodeViewModel>>(new GetAllRaceCodesQuery());
            }

            if (Session["NEW_COMPANY_USER"] != null && !(bool)Session["NEW_COMPANY_USER"])
            {
                Session["NEW_COMPANY_USER"] = true;
            }
            else if (companyUserViewModel.UserViewModel != null)
            {
                Session["IS_USER_EDITED"] = true;
            }
            companyUserViewModel.CompanyName = companyName;
            companyUserViewModel.CompanyId = companyId;

            return View(companyUserViewModel);
        }

        [HttpGet]
        public JsonResult CheckForSelectedUserRole(Guid selectedUserRoleId)
        {
            List<RoleViewModel> roleViewModelList =
                ExecuteQuery<EmptyQueryParameter, List<RoleViewModel>>(new EmptyQueryParameter());

            for (int x = 0; x < roleViewModelList.Count; x++)
            {
                if (roleViewModelList[x].RoleId == selectedUserRoleId)
                {
                    if (roleViewModelList[x].RoleName.Contains("Admin"))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManageOwnCustomerCompanyUser(Guid companyId, Guid userId, String companyName, Guid? groupId)
        {
            var customerTypes = new List<SelectListItem>
            {
                new SelectListItem {Value = "StandardUser", Text = "Standard User"},
                new SelectListItem {Value = "Administrator", Text = "Administrator"}
            };

            if (userId == Guid.Empty)
            {
                Session["EditForm"] = false;
            }
            else
            {
                Session["EditForm"] = true;
            }
            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = companyId,
                CompanyName = companyName,
                UserId = userId,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };

            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);
            //Get data to bind group drop-down
            List<GroupViewModel> groupViewModelList =
                ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter
                {
                    CompanyId = companyId,
                });
            if (Session["NEW_COMPANY_USER"] != null && !(bool)Session["NEW_COMPANY_USER"])
            {
                Session["NEW_COMPANY_USER"] = true;
                companyUserViewModel.UserViewModel = new UserViewModel
                {
                    SelectedGroupId = groupId,
                    DropDownForGroup = groupViewModelList.Select(c => new SelectListItem
                    {
                        Text = c.Title,
                        Value = c.GroupId.ToString()
                    }),
                    RaceCodes = ExecuteQuery<GetAllRaceCodesQuery, List<RaceCodeViewModel>>(new GetAllRaceCodesQuery()),
                    GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList()
                };
                companyUserViewModel.UserViewModel.CompanyName = companyUserViewModel.CompanyName;
            }
            else if (companyUserViewModel.UserViewModel != null)
            {
                Session["IS_USER_EDITED"] = true;

                companyUserViewModel.UserViewModel.SelectedGroupId = groupId;
                companyUserViewModel.UserViewModel.DropDownForGroup =
                    groupViewModelList.Select(c => new SelectListItem
                    {
                        Text = c.Title,
                        Value = c.GroupId.ToString()
                    });

                companyUserViewModel.UserViewModel.RaceCodes = ExecuteQuery<GetAllRaceCodesQuery, List<RaceCodeViewModel>>(new GetAllRaceCodesQuery());
                companyUserViewModel.UserViewModel.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();
                companyUserViewModel.UserViewModel.CustomerTypesSelectList = customerTypes;
                ExtractUserRolesToViewModel(companyUserViewModel.UserViewModel);
            }
            else
            {
                companyUserViewModel.UserViewModel = new UserViewModel
                {
                    DropDownForGroup = groupViewModelList.Select(c => new SelectListItem
                    {
                        Text = c.Title,
                        Value = c.GroupId.ToString()
                    }),
                    CustomerTypesSelectList = customerTypes,
                    GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList()
                };
                companyUserViewModel.UserViewModel.SelectedGroupId = groupId;
                companyUserViewModel.UserViewModel.RaceCodes = ExecuteQuery<GetAllRaceCodesQuery, List<RaceCodeViewModel>>(new GetAllRaceCodesQuery());
            }
            companyUserViewModel.UserViewModel.CustomerTypesSelectList = customerTypes;
            companyUserViewModel.CompanyName = companyName;
            companyUserViewModel.CompanyId = companyId;
            return View(companyUserViewModel);
        }
        private void ExtractUserRolesToViewModel(UserViewModel model)
        {
            if (!model.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)))
            {
                model.CustomerTypesSelectList[1].Selected = true;
            }
            if (model.Roles.Any(r => r.RoleName.Equals(Role.CustomerAdmin)))
            {
                model.CustomerAdmin = true;
                model.UserAdmin = true;
                model.CategoryAdmin = true;
                model.ContentAdmin = true;
                model.PortalAdmin = true;
                model.Publisher = true;
                model.Reporter = true;
                model.NotificationAdmin = true;
                model.TrainingActivityAdmin = true;
                model.TrainingActivityReporter = true;
            }
            else
            {
                foreach (var customerRole in model.Roles)
                {
                    if (customerRole.RoleName.Equals(Role.UserAdmin))
                        model.UserAdmin = true;
                    if (customerRole.RoleName.Equals(Role.CategoryAdmin))
                        model.CategoryAdmin = true;
                    if (customerRole.RoleName.Equals(Role.ContentAdmin))
                        model.ContentAdmin = true;
                    if (customerRole.RoleName.Equals(Role.PortalAdmin))
                        model.PortalAdmin = true;
                    if (customerRole.RoleName.Equals(Role.Publisher))
                        model.Publisher = true;
                    if (customerRole.RoleName.Equals(Role.Reporter))
                        model.Reporter = true;
                    if (customerRole.RoleName.Equals(Role.NotificationAdmin))
                        model.NotificationAdmin = true;
                    if (customerRole.RoleName.Equals(Role.TrainingActivityAdmin))
                        model.TrainingActivityAdmin = true;
                    if (customerRole.RoleName.Equals(Role.TrainingActivityReporter))
                        model.TrainingActivityReporter = true;
                }
            }
        }
        [HttpPost]
        public ActionResult CustomerUsersByCustomerCompanyId(string companyId)
        {
            ExecuteCommand(new UpdateConnectionStringCommand
            {
                CompanyId = companyId
            });
            var allusers = ExecuteQuery<AllCustomerUserQueryParameter, List<UserViewModel>>(
                new AllCustomerUserQueryParameter
                {
                    LoginUserCompanyId = PortalContext.Current.UserCompany.Id
                });

            ExecuteCommand(new UpdateConnectionStringCommand());

            return PartialView(allusers);
        }

        [HttpPost]
        public JsonResult UsersForCompany(string companyId)
        {
            var allUsers = ExecuteQuery<AllUsersQuery, IEnumerable<UserModelShort>>(new AllUsersQuery
            {
                CompanyId = companyId
            });

            return new JsonResult { Data = allUsers, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // GET: /CustomerManagement/CustomerMgmt/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.UserSet.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: /CustomerManagement/CustomerMgmt/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /CustomerManagement/CustomerMgmt/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(
                Include =
                    "Id,ParentUserID,ComapnyID,CompanyName,FirstName,LastName,PhysicalAddress,PostalAddress,EmailAddress,TelephoneNumber,WebsiteAddress,Password,MobileNumber,LayerSubDomain,ProvisionalAccountLink"
                )] User user)
        {
            if (ModelState.IsValid)
            {
                user.Id = Guid.NewGuid();
                db.UserSet.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: /CustomerManagement/CustomerMgmt/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.UserSet.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /CustomerManagement/CustomerMgmt/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(
                Include =
                    "Id,ParentUserID,ComapnyID,CompanyName,FirstName,LastName,PhysicalAddress,PostalAddress,EmailAddress,TelephoneNumber,WebsiteAddress,Password,MobileNumber,LayerSubDomain,ProvisionalAccountLink"
                )] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: /CustomerManagement/CustomerMgmt/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.UserSet.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult DeleteCustomerCompany(Guid id)
        {
            var portalContext = PortalContext.Current;
            DropCustomerDb(id);

            var deleteCustomerCompanyCommand = new DeleteCustomerCompanyCommand
            {
                CustomerCompanyId = id
            };
            var response = ExecuteCommand(deleteCustomerCompanyCommand);
            if (response.Validation.Any())
            {
                return Json(new { Status = 'F' }, JsonRequestBehavior.AllowGet);
            }
            PortalContext.Current = portalContext;
            if (!Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                //Insert User Activity
                var addUserActivityCommand = new AddUserActivityCommand
                {
                    ActivityDescription = "Deleted Customer Company",
                    ActivityType = UserActivityEnum.DeleteCustomerCompany,
                    CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    ActivityDate = DateTime.Now
                };
                ExecuteCommand(addUserActivityCommand);
            }
            NotifyError("Successfully deleted");
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        private string FixConnectionString(string input)
        {
            var parameters = input.ToLower().ToDictionaryFromConnectionString();

            if (parameters.ContainsKey("initial catalog"))
                parameters["initial catalog"] = "master";

            if (parameters.ContainsKey("database"))
                parameters["database"] = "master";

            if (parameters.ContainsKey("attachdbfilename"))
                parameters.Remove("attachdbfilename");

            if (parameters.ContainsKey("app"))
                parameters.Remove("app");

            var c = parameters.ToConnectionString();
            return c;
        }

        private string GetDbName(string input)
        {
            var parameters = input.ToLower().ToDictionaryFromConnectionString();

            if (parameters.ContainsKey("initial catalog"))
                return parameters["initial catalog"];

            if (parameters.ContainsKey("database"))
                return parameters["database"];

            return null;
        }

        [NonAction]
        public bool DropCustomerDb(Guid id)
        {
            bool success = false;
            try
            {
                var company = db.CompanySet.Find(id);
                if (company != null)
                {
                    PortalContext.Current = null;
                    SessionCache.Current.Clear();
                    RequestCache.Current.Clear();
                    var conString = ConfigurationManager.ConnectionStrings["CustomerContext"].ConnectionString;
                    var alldatabasesWithNameVariations = new string[]
                    {
                        conString.Replace("DBNAME", company.CompanyName) ,
                        conString.Replace("DBNAME", company.CompanyName.Replace(" ", string.Empty))
                    };
                    foreach (var dataBaseConString in alldatabasesWithNameVariations)
                    {
                        if (Database.Exists(dataBaseConString))
                            success = Database.Delete(dataBaseConString);
                        else
                        {
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return success;
        }

        // POST: /CustomerManagement/CustomerMgmt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            User user = db.UserSet.Find(id);
            db.UserSet.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //Customer Self Provisioning
        [AllowAnonymous]
        public ActionResult CustomerSelfProvision()
        {
            var customerSelfProvisionViewModel = new CustomerSelfProvisionViewModel();
            customerSelfProvisionViewModel.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();
            return View(customerSelfProvisionViewModel);

            // return null;
        }

        [AllowAnonymous]
        public ActionResult ConfirmEmail()
        {
            ViewBag.Email = Session["Email"];
            return View();
        }

        /// <summary>
        /// update the company lock status
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChangeCompanyLockStatus(Guid companyId, string status)
        {
            var updateCustomerCompanyLockStatusCommand = new UpdateCustomerCompanyLockStatusCommandParameter
            {
                CompanyId = companyId,
                LockStatus = status == "true"
            };
            var response = ExecuteCommand(updateCustomerCompanyLockStatusCommand);
            return null;
        }

        // Shedular for lock the company status
        public ActionResult CustomerCompanyLockSheduler()
        {
            var company = db.Company.Where(c => c.CompanyType == Domain.Enums.CompanyType.CustomerCompany && c.IsLock == false).ToList();
            if (company.Count > 0)
            {
                // var selfprovision = db.Company.Where(c => c.IsForSelfProvision == true).FirstOrDefault();
                // if (selfprovision != null)
                //  {

                #region company

                foreach (var comp in company)
                {
                    if (comp.AutoExpire == true)
                    {
                        #region CompanyLock

                        if (comp.IsSelfCustomer == true)
                        {
                            // company is self provision company so lock after 1 Month +1 day
                            //DateTime CreatedDate = comp.CreatedOn;
                            //DateTime CurrentDate = DateTime.UtcNow;
                            //int totaldays = Convert.ToInt32((CurrentDate - CreatedDate).TotalDays);
                            //if (totaldays >= 31)
                            //{
                            //    // lock the company
                            //    var updateCustomerCompanyLockStatusCommand = new UpdateCompanyLockStatusShedulerCommandParameter
                            //    {
                            //        CompanyId = comp.Id,
                            //        LockStatus = true,
                            //    };
                            //    var response = ExecuteCommand(updateCustomerCompanyLockStatusCommand);
                            //}

                            if (comp.ExpiryDate != null)
                            {
                                DateTime ExpiryDate = Convert.ToDateTime(comp.ExpiryDate);
                                DateTime CurrentDate = DateTime.Now;
                                if (CurrentDate.Date == ExpiryDate.Date)
                                {
                                    //  lock the company
                                    var updateCustomerCompanyLockStatusCommand = new UpdateCompanyLockStatusShedulerCommandParameter
                                    {
                                        CompanyId = comp.Id,
                                        LockStatus = true,
                                    };
                                    var response = ExecuteCommand(updateCustomerCompanyLockStatusCommand);
                                }
                            }
                        }
                        else
                        {
                            // Not a self provision customer company so lock after 1 Year + 1day
                            //DateTime CreatedDate = comp.CreatedOn;
                            //DateTime CurrentDate = DateTime.UtcNow;
                            //int totaldays = Convert.ToInt32((CurrentDate - CreatedDate).TotalDays);
                            //if (totaldays >= 366)
                            //{
                            //    // lock the company
                            //    var updateCustomerCompanyLockStatusCommand = new UpdateCompanyLockStatusShedulerCommandParameter
                            //    {
                            //        CompanyId = comp.Id,
                            //        LockStatus = true,
                            //    };
                            //    var response = ExecuteCommand(updateCustomerCompanyLockStatusCommand);
                            //}
                            if (comp.ExpiryDate != null)
                            {
                                DateTime ExpiryDate = Convert.ToDateTime(comp.ExpiryDate);
                                DateTime CurrentDate = DateTime.Now;
                                if (CurrentDate.Date == ExpiryDate.Date)
                                {
                                    //  lock the company
                                    var updateCustomerCompanyLockStatusCommand = new UpdateCompanyLockStatusShedulerCommandParameter
                                    {
                                        CompanyId = comp.Id,
                                        LockStatus = true,
                                    };
                                    var response = ExecuteCommand(updateCustomerCompanyLockStatusCommand);
                                }
                            }
                        }

                        #endregion CompanyLock
                    }
                }

                #endregion company

                //}
            }
            return null;
        }

        //Shedular for lock the Customer User  CustomerUserLockSheduler
        [AllowAnonymous]
        public ActionResult CustomerUserLockScheduler()
        {
            // find customer companies
            var customerCompany = db.Company.Where(c => c.CompanyType == Domain.Enums.CompanyType.CustomerCompany && c.IsActive).ToList();

            foreach (var company in customerCompany)
            {
                // find customer user  by company Id

                if (!company.DefaultUserExpireDays.HasValue)
                {
                    continue;
                }

                var newExpiredUsers = ExecuteQuery<NewExpiredUsersQuery, IEnumerable<StandardUser>>(new NewExpiredUsersQuery
                {
                    CompanyId = company.Id,
                    DefaultUserExpireDays = company.DefaultUserExpireDays
                }).ToList();

                var admins = ExecuteQuery<ActiveCustomerAdminsQuery, IEnumerable<StandardUser>>(
                    new ActiveCustomerAdminsQuery
                    {
                        CompanyId = company.Id
                    });

                ExecuteCommand(new UpdateUsersExpiryStatusCommand
                {
                    CompanyId = company.Id,
                    DefaultUserExpireDays = company.DefaultUserExpireDays
                });

                // send Email to customer Admin With List of User

                if (newExpiredUsers.Any())
                {
                    foreach (var admin in admins)
                    {
                        // send Email to Customer Admin.

                        string hostnameImage = HttpContext.Request.Url.AbsoluteUri;
                        string urlToRemove = hostnameImage.Substring(hostnameImage.IndexOf("CustomerManagement/CustomerMgmt/CustomerUserLockScheduler"));
                        string secondlasturl = hostnameImage.Replace(urlToRemove, String.Empty);

                        new NotificationService(CommandDispatcher).SendEmailCustomerExpire(new NotificationService.EmailSendUserExpire
                        {
                            userAdminName = admin.FirstName,
                            HeaderUrl = secondlasturl,
                            EmailAddress = admin.EmailAddress,
                            UserList = newExpiredUsers,
                        });
                    }
                }
            }

            return null;
        }

        [HttpPost]
        public JsonResult ChangeCompanyUserExpiryStatus(Guid userId, string status)
        {
            var ChangeCompanyUserExpiryStatus = new ChangeCompanyUserExpiryStatusCommandParameter
            {
                UserId = userId,
                IsExpiryStatus = status == "true"
            };
            var response = ExecuteCommand(ChangeCompanyUserExpiryStatus);
            return null;
        }

        // customer Admin Setting

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

        public ActionResult CustomerAdminSetting()
        {
            FillAllControls();
            PortalContext.Override(PortalContext.Current.UserCompany.Id);
            //model.Company = PortalContext.Current.UserCompany;
            return View(PortalContext.Current.UserCompany);
        }


        private void FillAllControls()
        {
            //below line by neeraj
            //var model = new AdminSettingsViewModel();
            ViewBag.CustomRoles = ExecuteQuery<FetchAllRecordsQuery, List<CustomUserRoles>>(new FetchAllRecordsQuery
            { });
            //below line by neeraj
            ViewBag.WorkflowList = ExecuteQuery<FetchAllRecordsQuery, List<AutoAssignWorkflowViewModel>>(new FetchAllRecordsQuery
            { });
            #region neeraj
            var testDropDown = ExecuteQuery<FocusAreaReportQuery, FocusAreaReportDataSources>(new FocusAreaReportQuery());
            ViewBag.TestDropDown = testDropDown.TestDropDown;


            var query = new PointsStatementQuery { WithData = false };
            query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole()
                ? (Guid?)null
                : PortalContext.Current.UserCompany.Id;
            query.EnableGlobalAccessDocuments = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
            var temp = ExecuteQuery<PointsStatementQuery, PointsStatementViewModel>(query);
            ViewBag.PointReportData = Newtonsoft.Json.JsonConvert.SerializeObject(temp);

            var meetings = ExecuteQuery<FetchAllQuery, IEnumerable<VirtualClassModel>>(new FetchAllQuery()).ToList();
            ViewBag.VirtualMeetings = meetings;

            var labels = ExecuteQuery<LabelListQuery, IEnumerable<TrainingLabelListModel>>(new LabelListQuery());
            ViewBag.TrainingLabelList = new SelectList(labels, "Id", "Name");


            var enumList = Enum.GetValues(typeof(TrainingActivityType)).OfType<TrainingActivityType>().ToList();
            IDictionary<int, string> dict = new Dictionary<int, string> {
                { 10,"None Selected"},
                { (int)TrainingActivityType.Bursary, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Bursary) },
                { (int)TrainingActivityType.External, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.External) },
                { (int)TrainingActivityType.Internal, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Internal) },
                { (int)TrainingActivityType.MentoringAndCoaching, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.MentoringAndCoaching) },
                { (int)TrainingActivityType.ToolboxTalk, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.ToolboxTalk) }
            };
            ViewBag.TrainingActivityTypeList = new SelectList(dict, "Key", "Value");


            ViewBag.ActivityCheckLists = ExecuteQuery<AllCheckListSubmissionReportQuery, IEnumerable<CheckListModel>>(new AllCheckListSubmissionReportQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Title)).Select(x => new MyCheckList
            {
                Value = x.Title,
                Extra = x.Description,
                Id = x.Id
            }).ToList();

            ViewBag.CustomDocumentPublished = ExecuteQuery<AllCustomDocumentListSubmissionReportQuery, IEnumerable<CustomDocumentCheckList>>(new AllCustomDocumentListSubmissionReportQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Title)).Select(x => new MyCustumDocumentCheckList
            {
                Value = x.Title,
                Extra = x.Description,
                Id = x.Id
            }).ToList();

            #endregion

            var query1 = new PointsStatementQuery { WithData = false };
            query1.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole()
                ? (Guid?)null
                : PortalContext.Current.UserCompany.Id;
            query1.EnableGlobalAccessDocuments = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
            var vm = ExecuteQuery<PointsStatementQuery, PointsStatementViewModel>(query1);
            ViewBag.Users = vm.Users;
            var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();
            ViewBag.Categories = IndentedCategories(categories, "#", 0);
            ViewBag.Groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(
                new AllGroupsByCustomerAdminQueryParameter
                {
                    CompanyId = PortalContext.Current.UserCompany.Id
                }).Select(g => new SerializableSelectListItem
                {
                    Value = g.GroupId.ToString(),
                    Text = g.Title
                }).OrderBy(x => x.Text);
            //ViewBag.WorkflowList = ExecuteQuery<FetchAllWorkflowQuery, List<AutoAssignWorkflow>>(new FetchAllWorkflowQuery
            //{ });
            ViewBag.ScheduleReportList = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery
            { });

            // bind group 
            List<GroupViewModel> groupViewModelList =
           ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter
           {
               CompanyId = PortalContext.Current.UserCompany.Id
           });
            ViewBag.Groups = groupViewModelList.Select(c => new SelectListItem
            {
                Text = c.Title,
                Value = c.GroupId.ToString()
            });

            ViewBag.DefaultGroup = groupViewModelList.FirstOrDefault(c => c.Title == "Default") == null ? "" : groupViewModelList.FirstOrDefault(c => c.Title == "Default").GroupId.ToString();

            ViewBag.SelfSignedGroup = groupViewModelList.FirstOrDefault(c => c.IsforSelfSignUpGroup) != null ? groupViewModelList.FirstOrDefault(c => c.IsforSelfSignUpGroup).GroupId : Guid.NewGuid();
            if (TempData["Message"] != null && TempData["Message"].ToString() == "Saved")
            {
                ViewBag.SaveMessage = "Admin setting update successfully";
            }
            else
            {
                ViewBag.SaveMessage = null;
            }

        }
        public ActionResult ReIndexUploads()
        {
            ExecuteCommand(new ReindexChapterUploadsCommandParameter());

            return RedirectToAction("CustomerAdminSetting", new { IndexSuccess = true });
        }

        [HttpPost]
        public ActionResult CustomerAdminSetting(CompanyViewModel model)
        {

            if (!model.IsForSelfSignUp)
            {
                model.IsSelfSignUpApprove = false;
                model.IsEmployeeCodeReq = false;
                model.IsEnabledEmployeeCode = false;

            }
            var updateCustomerCompanyAdminSettingCommand = new UpdateCustomerCompanyAdminSettingCommand
            {
                Id = model.Id,
                DefaultUserExpireDays = Convert.ToInt32(model.DefaultUserExpireDays),
                ShowCompanyNameOnDashboard = model.ShowCompanyNameOnDashboard,
                LegalDisclaimer = model.LegalDisclaimer,
                LegalDisclaimerActivationType = model.LegalDisclaimerActivationType,
                HideDashboardLogo = !model.ShowCompanyLogoOnDashboard,
                DashboardVideoFileId = model.DashboardVideoFileId,
                DashboardVideoTitle = model.DashboardVideoTitle,
                DashboardVideoDescription = model.DashboardVideoDescription,
                DashboardQuoteAuthor = model.DashboardQuoteAuthor,
                DashboardQuoteText = model.DashboardQuoteText,
                IsEmployeeCodeReq = model.IsEmployeeCodeReq,
                IsEnabledEmployeeCode = model.IsEnabledEmployeeCode,
                IsForSelfSignUp = model.IsForSelfSignUp,
                CompanySiteTitle = model.CompanySiteTitle,
                IsSelfSignUpApprove = model.IsSelfSignUpApprove
            };

            ExecuteCommand(updateCustomerCompanyAdminSettingCommand);
            if (model.CustomColours != null)
            {
                ExecuteCommand(new AddOrUpdateCustomColourCommand
                {
                    ButtonColour = model.CustomColours.ButtonColour,
                    FeedbackColour = model.CustomColours.FeedbackColour,
                    FooterColour = model.CustomColours.FooterColour,
                    HeaderColour = model.CustomColours.HeaderColour,
                    LoginColour = model.CustomColours.LoginColour,
                    NavigationColour = model.CustomColours.NavigationColour,
                    SearchColour = model.CustomColours.SearchColour,
                    CompanyId = model.Id
                });
            }
            #region Upload logo,footer,dashboard
            ExecuteCommand(new SaveCustomConfigurationCommand
            {
                CompanyId = model.Id.ToString(),
                DashboardLogo = model.CompanyLogo,
                LoginLogo = model.LoginLogo,
                FooterLogo = model.FooterLogo,
                NotificationHeaderLogo = null,
                NotificationFooterLogo = null,
                DeleteDashboardLogo = null,
                DeleteLoginLogo = null,
                DeleteFooterLogo = null,
                DeleteNotificationFooterLogo = null,
                DeleteNotificationHeaderLogo = null,

            });
            #endregion

            var group = ExecuteQuery<FetchByIdQuery, CustomerGroup>(new FetchByIdQuery
            {
                Id = Guid.Parse(model.SelectedGroupId)
            });

            var result = ExecuteCommand(new SaveOrUpdateGroupCommand
            {
                GroupId = Guid.Parse(model.SelectedGroupId),
                IsforSelfSignUpGroup = true,
                ParentId = group.ParentId,
                Title = group.Title,
                Description = group.Description,
                CompanyId = group.CompanyId
            });

            TempData["Message"] = "Saved";

            PortalContext.Override(model.Id);
            if (model.CustomerCompanyCss != null)
            {
                if (model.CustomerCompanyCss.ContentType == "text/css")
                {
                    if (model.CustomerCompanyCss != null && model.CustomerCompanyCss.ContentLength > 0)
                    {
                        var path = string.Join("/", Server.MapPath("~/Content"), string.Join("-", PortalContext.Current.UserCompany.Id, "custom.css"));
                        model.CustomerCompanyCss.SaveAs(path);
                        var uploadCustomCSSCommandParameter = new UploadCustomCSSCommandParameter
                        {
                            CSSFile = Utility.Convertor.BytesFromFilePath(path),
                            CompanyId = PortalContext.Current.UserCompany.Id
                        };
                        ExecuteCommand(uploadCustomCSSCommandParameter);
                        ModelState.AddModelError("", "Custom css uploaded sucessfully");
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please login to system");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "File Content should be type=text/css");
                }

            }
            return RedirectToAction("CustomerAdminSetting");
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




        [AllowAnonymous]
        public ActionResult CustomerUserSelfSignUp(string companyId)
        {
            var id = Guid.Parse(companyId);
            PortalContext.Override(id);
            CustomerSelfSignUpViewModel model = new CustomerSelfSignUpViewModel();
            if (PortalContext.Current != null)
            {
                Session["CompanyId"] = id;
                var imageName = PortalContext.Current.LogoFileName;
                model.LogoPath = System.IO.File.Exists(Server.MapPath("~/LogoImages/CustomerLogo/" + imageName)) ? Url.Content("~/LogoImages/CustomerLogo/" + imageName) : Url.Content("~/Content/images/logo.png");
                model.CompanyId = id;
                model.IsEnabledEmployeeCode = PortalContext.Current.UserCompany.IsEnabledEmployeeCode;
                model.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();
                model.IsEmpCodeRequired = PortalContext.Current.UserCompany.IsEmployeeCodeReq;
                return View(model);
            }
            return null;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult CustomerUserSelfSignUp(CustomerSelfSignUpViewModel model)
        {
            if (model != null)
            {
                model.EmailAddress = model.EmailAddress.TrimAllCastToLowerInvariant();


                if (!string.IsNullOrWhiteSpace(Session["CompanyId"]?.ToString()) && Guid.TryParse(Session["CompanyId"].ToString(), out var gId))
                {
                    try
                    {
                        var saveCustomerUserSelfSignUpCommandParameter = new SaveCustomerUserSelfSignUpCommandParameter
                        {
                            CompanyViewModel = PortalContext.Override(gId).UserCompany,
                            CustomerSelfSignUpViewModel = model
                        };
                        var response = ExecuteCommand(saveCustomerUserSelfSignUpCommandParameter);

                        if (response.Validation.Any())
                        {
                            ViewBag.ErrMessage = response.Validation.First(e => e.Message != null).Message;
                            var imageName = PortalContext.Current.LogoFileName;
                            model.LogoPath = System.IO.File.Exists(Server.MapPath("~/LogoImages/CustomerLogo/" + imageName)) ? Url.Content("~/LogoImages/CustomerLogo/" + imageName) : Url.Content("~/Content/images/logo.png");
                            model.CompanyId = PortalContext.Current.UserCompany.Id;
                            model.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();
                            return View(model);
                        }
                        TempData["IsApproved"] = saveCustomerUserSelfSignUpCommandParameter.CompanyViewModel.IsSelfSignUpApprove;
                        Session["Email"] = model.EmailAddress;

                        NotifySuccess(PortalContext.Current.UserCompany.IsSelfSignUpApprove
                            ? "Your account has been created - please check your email."
                            : "Sign up successfully, Approval pending");
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    return RedirectToAction("LogOff", "Account", new { area = "" });
                    //return RedirectToAction("Login", "Account", new { area = "" });
                }
            }
            return View();
        }

        public ActionResult CustomerUserSelfSignUpNotApproved()
        {
            var customerSelfSignUpQueryParameter = new CustomerSelfSignUpQueryParameter
            {
                CompanyId = PortalContext.Current.UserCompany.Id,
            };

            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerSelfSignUpQueryParameter, CompanyUserViewModel>(customerSelfSignUpQueryParameter);

            return View(companyUserViewModel);
        }

        [HttpPost]
        public JsonResult ChangeCustomerSelfSignUpApproveStatus(Guid userId, string status)
        {
            var changeCustomerSelfSignUpApproveStatusCommandParameter = new ChangeCustomerSelfSignUpApproveStatusCommandParameter
            {
                UserId = userId,
                ApproveStatus = status == "true",
                CompanyViewModel = PortalContext.Current.UserCompany
            };
            var response = ExecuteCommand(changeCustomerSelfSignUpApproveStatusCommandParameter);

            return null;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult CustomerSelfProvision(CustomerSelfProvisionViewModel model)
        {
            // Code for validating the CAPTCHA
            if (this.IsCaptchaValid("Captcha is not valid"))
            {
                #region SaveCustomerCompanySelfProvision

                string fileNameToSave = "";
                try
                {
                    string name = Path.GetFileName(model.CompanyLogo?.FileName);
                    if (name != null)
                    {
                        var ext = Path.GetExtension(name);
                        fileNameToSave = name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmdd") + ext;
                        ViewBag.FileName = fileNameToSave;
                        var customerCompanyLogoDirectoryPath =
                            Path.Combine(Server.MapPath(ConfigurationManager.AppSettings["CompaniesLogo"]),
                                "CustomerLogo");

                        RampDirectoryManager.CreateDirectory(customerCompanyLogoDirectoryPath);
                        var path = Path.Combine(customerCompanyLogoDirectoryPath, fileNameToSave);

                        using (var s = model.CompanyLogo.InputStream)
                        {
                            CommonHelper.SaveImage(s, null, 100, path);
                        }
                    }
                }
                catch (IOException)
                {
                }
                var saveOrUpdateCustomerCompanyCommand = new SaveSelfProvisionalCustomerCompanyCommand
                {
                    Id = model.Id,
                    CompanyName = model.CompanyName.RemoveSpecialCharacters(),
                    LayerSubDomain = model.LayerSubDomain,
                    PhysicalAddress = model.PhysicalAddress,
                    PostalAddress = model.PostalAddress,
                    TelephoneNumber = model.TelephoneNumber,
                    WebsiteAddress = model.WebsiteAddress,
                    LogoImageUrl = fileNameToSave,
                    ClientSystemName = model.ClientSystemName,
                    FullName = model.FullName,
                    MobileNumber = model.MobileNumber,
                    EmailAddress = model.EmailAddress.TrimAllCastToLowerInvariant(),
                    Password = model.Password,
                    AbsoluteUrl = HttpContext.Request.Url.AbsoluteUri,
                    IDNumber = model.IDNumber,
                    Gender = model.Gender
                };

                var response = ExecuteCommand(saveOrUpdateCustomerCompanyCommand);
                if (response.Validation.Any())
                {
                    ViewBag.ErrMessage = response.Validation.First().Message;
                    model.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();

                    return View(model);
                }

                Session["Email"] = model.EmailAddress;
                NotifySuccess("Customer saved");
                return RedirectToAction("ConfirmEmail", "CustomerMgmt", new { area = "CustomerManagement" });

                #endregion SaveCustomerCompanySelfProvision
            }

            ViewBag.ErrMessage = "Error: captcha is not valid.";
            model.GenderDropDown = Code.Helpers.GenderHelper.ToDropDownList();
            return View();
        }

        public DateTime ExpiryDate(string yearly)
        {
            DateTime expiryDate = DateTime.Now;

            if (yearly == "1")
            {
                DateTime currentDate = DateTime.Now;
                expiryDate = currentDate.AddYears(1);
            }
            else
            {
                DateTime currentDate = DateTime.Now;
                expiryDate = currentDate.AddMonths(1);
            }

            return expiryDate;
        }

        public ActionResult ResetCustomColours(Guid CompanyId)
        {
            ExecuteCommand(new DeleteCustomColourCommand { CompanyId = CompanyId });
            ExecuteCommand(new SaveCustomConfigurationCommand
            {
                CompanyId = PortalContext.Current.UserCompany.Id.ToString(),
                DeleteFooterLogo = true,
                DeleteDashboardLogo = true,
                DeleteLoginLogo = true
            });
            var updateCustomerCompanyAdminSettingCommand = new UpdateCustomerCompanyAdminSettingCommand
            {
                Id = PortalContext.Current.UserCompany.Id,
                IsCompanySiteTitleReset = true
            };
            ExecuteCommand(updateCustomerCompanyAdminSettingCommand);
            PortalContext.Override(CompanyId);

            return RedirectToAction("CustomerAdminSetting");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult DoesEmailAlreadyPresent(string EmailAddress)
        {
            var userViewModel =
                    ExecuteQuery<UserSearchQueryParameter, UserViewModel>(new UserSearchQueryParameter
                    {
                        Email = EmailAddress
                    });
            var checkEmail = userViewModel != null ? true : false;
            return Json(checkEmail, JsonRequestBehavior.AllowGet);
        }


        //below code added by neeraj
        #region
        [HttpPost]
        public ActionResult AddEditCutomUserRole(string data)
        {
            var model = (new JavaScriptSerializer()).Deserialize<AddCustomUserRoleViewModel>(data);

            if (model.RoleName != null &&
                (
                model.StandardUser == true ||
                model.ContentCreator == true ||
                model.ContentApprover == true ||
                model.ContentAdmin == true ||
                model.PortalAdmin == true ||
                model.Publisher == true ||
                model.Reporter == true ||
                model.UserAdmin == true ||
                model.ManageTags == true ||
                model.ManageVirtualMeetings == true ||
                    model.ManageAutoWorkflow == true ||
                model.ManageReportSchedule == true
                )
                )
            {
                var command = new SaveOrUpdateCustomUserRoleCommand()
                {

                    Id = (model.Id != null) ? Guid.Parse(model.Id) : Guid.Empty,
                    UserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    Title = model.RoleName,

                    StandardUser = model.StandardUser,
                    ContentCreator = model.ContentCreator,
                    ContentApprover = model.ContentApprover,
                    ContentAdmin = model.ContentAdmin,
                    PortalAdmin = model.PortalAdmin,
                    Publisher = model.Publisher,
                    Reporter = model.Reporter,
                    UserAdmin = model.UserAdmin,
                    ManageTags = model.ManageTags,
                    ManageVirtualMeetings = model.ManageVirtualMeetings,
                    ManageAutoWorkflow = model.ManageAutoWorkflow,
                    ManageReportSchedule = model.ManageReportSchedule,
                    DateCreated = DateTime.Now,
                    IsActive = true,

                };

                var res = ExecuteCommand(command);

                if (res.ErrorMessage == null)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult EditCustomUserRole(string id)
        {

            var data = ExecuteQuery<FetchByIdQuery, CustomUserRoles>(new FetchByIdQuery
            {
                Id = Guid.Parse(id)
            });

            ViewBag.EdirCustomRole = data;

            return PartialView("_EditCustomUserRole");
        }

        [HttpPost]
        public ActionResult DeleteCustomUserRole(Guid id)
        {
            var command = new DeleteCustomUserRoleCommand()
            {
                Id = id
            };

            var res = ExecuteCommand(command);

            if (res.ErrorMessage == null)
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false, message = res.ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        [HttpPost]
        public JsonResult ChangeReportStatus(Guid reportID, bool status)
        {
            var scheduleReportStatus = new scheduleReportStatusVM
            {
                ReportId = reportID,
                Status = status
            };



            ExecuteCommand(scheduleReportStatus);


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #region below code added by Gaurav


        public ActionResult AddEditScheduleReport(ScheduleReportVM model)
        {
            //var model = (new JavaScriptSerializer()).Deserialize<ScheduleReportVM>(data);
            // List<string> ReportAssigned = new List<string>();
            if (model.ScheduleName != null)
            {
                var command = new SaveOrUpdateScheduleReportCommand()
                {

                    Id = model.Id,
                    ScheduleName = model.ScheduleName,
                    RecipientsList = model.RecipientsList,
                    ReportAssignedId = model.ReportAssignedId,
                    Occurences = model.Occurences,
                    ScheduleTime = model.ScheduleTime,
                    DateCreated = DateTime.Now,
                    IsDeleted = model.IsDeleted,
                    Params = model.Params

                };

                var res = ExecuteCommand(command);

                if (res.ErrorMessage == null)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteScheduleReport(Guid id)
        {
            var command = new DeleteScheduleReportCommand()
            {
                Id = id
            };

            var res = ExecuteCommand(command);

            if (res.ErrorMessage == null)
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false, message = res.ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditScheduleReport(Guid id)
        {
            //var reportData=FillScheduleReportModel();
            FillAllControls();
            var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
            {
                Id = id
            });

            ViewBag.EditScheduleReport = data;

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


            //	return PartialView("_EditScheduleReport");
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
                            permissions = permissions + ", Auto Workflow Manager";
                        }
                        else
                        {
                            permissions = "Auto Workflow Manager";
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

        #endregion



        //below code added by neeraj for autoassignworkflow

        #region autoassign workflow

        [HttpPost]
        public ActionResult AddAutoAssignWorkflow(AddAutoAssignWorkflowViewModel model)
        {

            var command = new SaveOrUpdateAutoAssignWorkflowCommand()
            {
                Id = model.Id,
                WorkflowName = model.WorkflowName,
                GroupId = model.GroupIds,
                DocumentListID = model.DocumentList,
                SendNotiEnabled = model.SendNotiEnabled,
                AssignedBy = Thread.CurrentPrincipal.GetId().ToString(),
                ComapnyId = PortalContext.Current.UserCompany.Id.ToString()

            };

            var res = ExecuteCommand(command);

            if (res.ErrorMessage == null)
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult DeleteAutoAssignWorkFlow(string id)
        {

            var command = new DeleteAutoAssignWorkflowCommand()
            {
                Id = id
            };

            var res = ExecuteCommand(command);

            if (res.ErrorMessage == null)
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false, message = res.ErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditAutoAssignWorkFlow(string id)
        {
            if (id != null && id != "")
            {
                var data = ExecuteQuery<FetchByIdQuery, AutoAssignWorkflowViewModel>(new FetchByIdQuery
                {
                    Id = Guid.Parse(id)
                });

                ViewBag.AutoAssignWorkFlowData = JsonConvert.SerializeObject(data);

                List<GroupViewModel> groupViewModelList =
               ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter
               {
                   CompanyId = PortalContext.Current.UserCompany.Id
               });
                ViewBag.Groups = groupViewModelList.Select(c => new SelectListItem
                {
                    Text = c.Title,
                    Value = c.GroupId.ToString()
                }).ToList();
            }
            return PartialView("_EditAutoAssignWorkflow");
        }
        #endregion

        public ActionResult AddEditField(CustomFieldsViewModel model)
        {
            //var model = (new JavaScriptSerializer()).Deserialize<ScheduleReportVM>(data);
            // List<string> ReportAssigned = new List<string>();
            if (model.FieldName != null)
            {
                var command = new SaveOrUpdateCustomFieldCommand()
                {

                    Id = model.Id,
                    FieldName = model.FieldName,
                    Type = model.Type,
                    DateCreated = DateTime.Now,
                    IsDeleted = model.IsDeleted,


                };

                var res = ExecuteCommand(command);

                if (res.ErrorMessage == null)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //public  ActionResult GetJSTreeFields()
        //{

        //    var groups = ExecuteQuery<CustomFiledsQuery, IEnumerable<JSTreeViewModel>>(new CustomFiledsQuery()).ToList();
        //    groups.ForEach(x =>
        //    {
        //        if (x.parent == "#")
        //            x.parent = $"{Guid.Empty}";
        //    });
        //    (groups as IList<JSTreeViewModel>).Insert(0, new JSTreeViewModel { parent = "#", id = $"{Guid.Empty}", text = "Groups" });
        //    ViewBag.Groups = groups;
        //    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        //}
    }
}