using Data.EF;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Attributes;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using Ramp.Services.QueryHandler.ProvisionalManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Common;
using Web.UI.Areas.CustomerManagement.Controllers;
using Web.UI.Code.Extensions;
using RampController = Web.UI.Controllers.RampController;
using Role = Ramp.Contracts.Security.Role;

namespace Web.UI.Areas.ProvisionalManagement.Controllers
{
    public class ProvisionalMgmtController : RampController
    {
        private readonly MainContext db = new MainContext();

        // GET: /ProvisionalManagement/ProvisionalMgmt/
        public ActionResult Index(Guid? id)
        {
            Session["IS_COMPANY_EDITED"] = false;
            var provisionalQueryParameter = new ProvisionalCompanyQueryParameter();
            if (id != null && id != Guid.Empty)
            {
                Session["IS_COMPANY_EDITED"] = true;
                provisionalQueryParameter.Id = id;
            }
            CompanyViewModelLong result =
                ExecuteQuery<ProvisionalCompanyQueryParameter, CompanyViewModelLong>(provisionalQueryParameter);
            return View(result);
        }

        [HttpPost]
        public JsonResult ChangeCompanyStatus(Guid companyId, string status)
        {
            var updateProvisionalCompanyStatus = new UpdateProvisionalCompanyStatusCommand
            {
                ProvisionalCompanyId = companyId,
                ProvisionalCompanyStatus = status == "true"
            };
            ExecuteCommand(updateProvisionalCompanyStatus);
            NotifyInfo("Company status updated successfully");
            return null;
        }

        [HttpPost]
        public JsonResult ChangeCompanyUserStatus(Guid userId, string status)
        {
            var updateProvisionalCompanyStatus = new UpdateProvisionalCompanyUserStatusCommand
            {
                ProvisionalCompanyUserId = userId,
                ProvisionalCompanyUserStatus = status == "true",
                SentFromProvisionalManagement = true
            };
            ExecuteCommand(updateProvisionalCompanyStatus);
            NotifyInfo("User status updated successfully");
            return null;
        }

        [HttpPost]
        public ActionResult CreateOrUpdateProvisionalCompany(CompanyViewModelLong provisionalCompanyViewModel)
        {
            string fileNameToSave = "";
            if (provisionalCompanyViewModel.CompanyViewModel.CompanyLogo != null)
            {
                string name = Path.GetFileName(provisionalCompanyViewModel.CompanyViewModel.CompanyLogo.FileName);
                if (name != null)
                {
                    var ext = Path.GetExtension(name);

                    fileNameToSave = name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmdd") + ext;

                    ViewBag.FileName = fileNameToSave;

                    string provisionalCompanyLogoDirectoryPath =
                       Path.Combine(Server.MapPath(ConfigurationManager.AppSettings["CompaniesLogo"]),
                           "ProvisionalLogo");

                    RampDirectoryManager.CreateDirectory(provisionalCompanyLogoDirectoryPath);

                    string path =
                        Path.Combine(provisionalCompanyLogoDirectoryPath, fileNameToSave);

                    using (var s = provisionalCompanyViewModel.CompanyViewModel.CompanyLogo.InputStream)
                    {
                        CommonHelper.SaveImage(s, null, 100, path);
                    }
                }
            }

            var provisionalCompanyCommandParameter = new ProvisionalCompanyCommandParameter
            {
                Id = provisionalCompanyViewModel.CompanyViewModel.Id,
                CompanyName = provisionalCompanyViewModel.CompanyViewModel.CompanyName.Trim(),
                CompanyCreatedByUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                LayerSubDomain = provisionalCompanyViewModel.CompanyViewModel.LayerSubDomain,
                PhysicalAddress = provisionalCompanyViewModel.CompanyViewModel.PhysicalAddress,
                PostalAddress = provisionalCompanyViewModel.CompanyViewModel.PostalAddress,
                ProvisionalAccountLink = provisionalCompanyViewModel.CompanyViewModel.ProvisionalAccountLink,
                TelephoneNumber = provisionalCompanyViewModel.CompanyViewModel.TelephoneNumber,
                WebsiteAddress = provisionalCompanyViewModel.CompanyViewModel.WebsiteAddress.TrimAllCastToLowerInvariant(),
                LogoImageUrl = fileNameToSave,
                IsChangePasswordFirstLogin = provisionalCompanyViewModel.CompanyViewModel.IsChangePasswordFirstLogin,
                IsSendWelcomeSMS = provisionalCompanyViewModel.CompanyViewModel.IsSendWelcomeSMS,
                IsForSelfProvision = provisionalCompanyViewModel.CompanyViewModel.IsForSelfProvision
            };
            ExecuteCommand(provisionalCompanyCommandParameter);

            if (Session["IS_COMPANY_EDITED"] != null)
            {
                if ((bool)(Session["IS_COMPANY_EDITED"]))
                {
                    NotifyInfo("Provisional company successfully updated");
                }
                else
                {
                    NotifySuccess("Provisional company successfuly created");
                }
            }
            Session["IS_COMPANY_EDITED"] = false;
            return RedirectToAction("Index");
        }

        public ActionResult GetUserFrequency()
        {
            CompanyViewModel companyModel = null;
            if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                companyModel =
                    ExecuteQuery<AllCustomerCompanyQueryParameter, CompanyViewModel>(
                        new AllCustomerCompanyQueryParameter());
            }
            if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                companyModel =
                    ExecuteQuery<AllCustomerCompanyByProvisionalCompanyParameter, CompanyViewModel>(new AllCustomerCompanyByProvisionalCompanyParameter
                    {
                        CompanyId = Thread.CurrentPrincipal.GetCompanyId(),
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        UserRole = SessionManager.GetUserRoleOfCurrentlyLoggedInUser()
                    });
            }
            if (Thread.CurrentPrincipal.IsInAdminRole())
            {
                companyModel = new CompanyViewModel
                {
                    CompanyList = new List<CompanyModelShort>
                    {
                        new CompanyModelShort
                        {
                            Id = PortalContext.Current.UserCompany.Id,
                            Name = PortalContext.Current.UserCompany.CompanyName
                        }
                    }
                };
            }
            if (companyModel != null)
            {
                companyModel.Companies =
                    companyModel.CompanyList.Select(c => new SerializableSelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    });
            }
            return View(companyModel);
        }

        public ActionResult GetUsersLoginStatistics()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LoadBarChartToGetUsersStats(string fromDate, string toDate)
        {
            var list = new List<UserLoginFrequencyViewModel>();
            CompanyViewModel companyViewModel = null;
            if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                companyViewModel =
                    ExecuteQuery<AllCustomerCompanyQueryParameter, CompanyViewModel>(new AllCustomerCompanyQueryParameter());
            }
            if (Thread.CurrentPrincipal.IsInResellerRole())
            {
                companyViewModel =
                    ExecuteQuery<AllCustomerCompanyByProvisionalCompanyParameter, CompanyViewModel>(new AllCustomerCompanyByProvisionalCompanyParameter
                    {
                        CompanyId = Thread.CurrentPrincipal.GetCompanyId(),
                        CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                        UserRole = SessionManager.GetUserRoleOfCurrentlyLoggedInUser()
                    });
            }
            if (companyViewModel != null)
            {
                long totalLoginOnOnRamp = 0;
                foreach (var companyModelShort in companyViewModel.CompanyList)
                {
                    PortalContext.Override(companyModelShort.Id);
                    var userFrequencyViewModels = ExecuteQuery
                        <UsersLoginStatsQueryParameter, UserLoginFrequencyViewModel>(
                            new UsersLoginStatsQueryParameter
                            {
                                CompanyId = PortalContext.Current.UserCompany.Id,
                                CurrentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                                FromDate = Convert.ToDateTime(fromDate),
                                ToDate = Convert.ToDateTime(toDate)
                            });
                    userFrequencyViewModels.CompanyName = companyModelShort.Name;
                    totalLoginOnOnRamp += userFrequencyViewModels.AllLoginsForSystem;
                    list.Add(userFrequencyViewModels);
                }

                foreach (var userLoginFrequencyViewModel in list)
                {
                    userLoginFrequencyViewModel.TotalLoginPercentageOfSystem =
                        Math.Round(
                            (double)userLoginFrequencyViewModel.AllLoginsForSystem / (double)totalLoginOnOnRamp, 2) *
                        100;
                }
                return Json(new { UserFrequencyList = list }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadBarChartToGetUserFrequency(Guid companyId, string fromDate, string toDate)
        {
            try
            {
                PortalContext.Override(companyId);
                var list = new List<UserLoginFrequencyReportViewModel>();
                var queryParameter = new AllCustomerUserQueryParameter
                {
                    LoginUserCompanyId = companyId
                };
                var allCustomerStandardUsersByCustomerAdmin =
                     ExecuteQuery<AllCustomerUserQueryParameter, List<UserViewModel>>(queryParameter);
                foreach (var userViewModel in allCustomerStandardUsersByCustomerAdmin)
                {
                    var userLoginFrequencyReport = ExecuteQuery
                         <UserLoginFrequencyQueryParameter, UserLoginFrequencyReportViewModel>(
                             new UserLoginFrequencyQueryParameter
                             {
                                 SelectedUserId = userViewModel.Id,
                                 FromDate = Convert.ToDateTime(fromDate),
                                 ToDate = Convert.ToDateTime(toDate)
                             });
                    list.Add(userLoginFrequencyReport);
                }

                return Json(new { UserFrequencyList = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ProvisionalCompanyUser(Guid companyId, Guid userId, String companyName)
        {
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
            var provisionalCompanyUserQueryParameter = new ProvisionalCompanyUserQueryParameter
            {
                CompanyId = companyId,
                UserId = userId
            };

            CompanyUserViewModel result =
                ExecuteQuery<ProvisionalCompanyUserQueryParameter, CompanyUserViewModel>(
                    provisionalCompanyUserQueryParameter);

            if (result.UserViewModel != null)
            {
                Session["IS_USER_EDITED"] = true;
            }
            if (Session["NEW_COMPANY_USER"] != null && !(bool)Session["NEW_COMPANY_USER"])
            {
                Session["NEW_COMPANY_USER"] = true;
                result.UserViewModel = new UserViewModel();
            }
            result.CompanyName = companyName;
            result.CompanyId = companyId;

            if (result.UserViewModel != null && result.UserViewModel.EmailAddress != null)
                Session["Editing_Email"] = result.UserViewModel.EmailAddress;

            return View(result);
        }

        [HttpPost]
        public ActionResult CreateOrUpdateProvisionalCompanyUser(CompanyUserViewModel provisionalCompanyUserViewModel)
        {
            var provisionalCompanyUserCommand = new AddOrUpdateProvisionalCompanyUserCommand
            {
                ContactNumber = provisionalCompanyUserViewModel.UserViewModel.ContactNumber,
                UserId = provisionalCompanyUserViewModel.UserViewModel.Id,
                CompanyId = provisionalCompanyUserViewModel.CompanyId,
                EmailAddress = provisionalCompanyUserViewModel.UserViewModel.EmailAddress.TrimAllCastToLowerInvariant(),
                FirstName = provisionalCompanyUserViewModel.UserViewModel.FullName.GetFirstName(),
                LastName = provisionalCompanyUserViewModel.UserViewModel.FullName.GetLastName(),
                MobileNumber = provisionalCompanyUserViewModel.UserViewModel.MobileNumber,
                ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                Password = provisionalCompanyUserViewModel.UserViewModel.Password,
                IDNumber = provisionalCompanyUserViewModel.UserViewModel.IDNumber
            };
            if (provisionalCompanyUserViewModel.UserViewModel.Id != Guid.Empty)
            {
                Session["NEW_COMPANY_USER"] = false;
            }
            var response = ExecuteCommand(provisionalCompanyUserCommand);
            var previousUrl = new Url(Request.UrlReferrer.ToString());
            ModelState.Clear();

            if (Session["IS_USER_EDITED"] != null)
            {
                if ((bool)(Session["IS_USER_EDITED"]))
                {
                    NotifyInfo("User successfully updated");
                }
                else
                {
                    NotifySuccess("User successfuly created");
                }
            }

            Session["IS_USER_EDITED"] = false;
            Response.Redirect(previousUrl.Value);
            return null;
        }

        public ActionResult ReassignCompanyUser()
        {
            ProvisionalCompanyListViewModel model =
                ExecuteQuery<EmptyQueryParameter, ProvisionalCompanyListViewModel>(new EmptyQueryParameter());
            var dummyModel = new CompanyViewModel
            {
                CompanyName = "Select Company",
                Id = Guid.Empty
            };
            model.FromProvisionalCompanyList.Insert(0, dummyModel);
            model.FromCompanies =
                model.FromProvisionalCompanyList.Select(c => new SerializableSelectListItem
                {
                    Text = c.CompanyName,
                    Value = c.Id.ToString()
                });
            model.ToProvisionalCompanyList.Insert(0, dummyModel);
            model.ToCompanies = model.ToProvisionalCompanyList.Select(a => new SerializableSelectListItem
            {
                Text = a.CompanyName,
                Value = a.Id.ToString()
            });
            return View(model);
        }

        [HttpPost]
        public ActionResult GetCustomerCompaniesFromProvisionalCompanyId(Guid companyId)
        {
            CompanyViewModelLong model =
                ExecuteQuery<AllCustomerCompanyQueryParameter, CompanyViewModelLong>(new AllCustomerCompanyQueryParameter
                {
                    ProvisionalCompanyId = companyId
                });

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult ResellersByProvisionalCompanyId(Guid companyId)
        {
            CompanyUserViewModel model =
                ExecuteQuery<ProvisionalCompanyUserQueryParameter, CompanyUserViewModel>(new ProvisionalCompanyUserQueryParameter
                {
                    CompanyId = companyId
                });

            return PartialView(model.UserList.OrderBy(u => u.FirstName).ToList());
        }

        [HttpPost]
        public JsonResult ResellersForProvisionalCompany(string companyId)
        {
            var model = ExecuteQuery<ProvisionalCompanyUserQueryParameter, CompanyUserViewModel>(
                new ProvisionalCompanyUserQueryParameter
                {
                    CompanyId = companyId.ConvertToGuid() ?? Guid.Empty
                });

            var users = model.UserList.OrderBy(u => u.FirstName).Select(u => new UserModelShort
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}"
            });

            return new JsonResult { Data = users, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult CustomerCompaniesFromProvisionalCompanyId(Guid companyId)
        {
            CompanyViewModelLong model =
                ExecuteQuery<AllCustomerCompanyQueryParameter, CompanyViewModelLong>(new AllCustomerCompanyQueryParameter
                {
                    ProvisionalCompanyId = companyId
                });

            return PartialView(model);
        }


        [HttpGet]
        public ActionResult DoesEmailAlreadyPresent(CompanyUserViewModel model)
        {
            if (Session["Editing_Email"] == null ||
            (Session["Editing_Email"] != null && Session["Editing_Email"].ToString() != model.UserViewModel.EmailAddress))
            {
                var emailExistQueryParameter = new EmailExistQueryParameter
                {
                    Email = model.UserViewModel.EmailAddress.TrimAllCastToLowerInvariant(),
                    PortContext = PortalContext.Current
                };
                RemoteValidationResponseViewModel result =
                    ExecuteQuery<EmailExistQueryParameter, RemoteValidationResponseViewModel>(emailExistQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DoesEmailAlreadyPresentSelfSignUp(CustomerSelfSignUpViewModel model)
        {
            var id = Session["CompanyId"]?.ToString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                if (Guid.TryParse(id, out var gId))
                {
                    var emailExistQueryParameter = new EmailExistQueryParameter
                    {
                        Email = model.EmailAddress.TrimAllCastToLowerInvariant(),
                        PortContext = PortalContext.Override(gId)
                    };
                    RemoteValidationResponseViewModel result =
                        ExecuteQuery<EmailExistQueryParameter, RemoteValidationResponseViewModel>(emailExistQueryParameter);
                    if (!result.Response)
                        return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DoesEmailAlreadyPresentSelfProvision(CustomerSelfProvisionViewModel model)
        {
            RemoteValidationResponseViewModel result =
               ExecuteQuery<ResellerEmailExistQueryParameter, RemoteValidationResponseViewModel>(new ResellerEmailExistQueryParameter { Email = model.EmailAddress.TrimAllCastToLowerInvariant() });
            if (result.Response)
                return Json(false, JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DoesCompanyNameAlreadyPresent(CompanyViewModelLong model)
        {
            if (Session["IS_COMPANY_EDITED"] != null && !(bool)Session["IS_COMPANY_EDITED"])
            {
                var companyNameExistQueryParameter = new CompanyNameExistQueryParameter
                {
                    CompanyName = model.CompanyViewModel.CompanyName.Trim()
                };
                RemoteValidationResponseViewModel result =
                    ExecuteQuery<CompanyNameExistQueryParameter, RemoteValidationResponseViewModel>(
                        companyNameExistQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DoesClientSystemNameAlreadyPresent(CompanyViewModelLong model)
        {
            if (Session["IS_COMPANY_EDITED"] != null && !(bool)Session["IS_COMPANY_EDITED"])
            {
                var clientSystemNameExistsQueryParameter = new ClientSystemNameExistsQueryParameter
                {
                    ClientSystemName = model.CompanyViewModel.ClientSystemName.Trim()
                };
                RemoteValidationResponseViewModel result =
                    ExecuteQuery<ClientSystemNameExistsQueryParameter, RemoteValidationResponseViewModel>(
                        clientSystemNameExistsQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DoesLayerSubDomainAlreadyPresent(CompanyViewModelLong model)
        {
            if (Session["IS_COMPANY_EDITED"] != null && !(bool)Session["IS_COMPANY_EDITED"])
            {
                var layerSubDomainExistsQueryParameter = new LayerSubDomainExistsQueryParameter
                {
                    LayerSubDomainName = model.CompanyViewModel.LayerSubDomain.TrimAllCastToLowerInvariant()
                };
                RemoteValidationResponseViewModel result =
                    ExecuteQuery<LayerSubDomainExistsQueryParameter, RemoteValidationResponseViewModel>(
                        layerSubDomainExistsQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DoesLayerSubDomainAlreadyExistSelfProvition(CustomerSelfProvisionViewModel model)
        {
            var layerSubDomainExistsQueryParameter = new LayerSubDomainExistsQueryParameter
            {
                LayerSubDomainName = model.LayerSubDomain.TrimAllCastToLowerInvariant()
            };
            RemoteValidationResponseViewModel result =
                ExecuteQuery<LayerSubDomainExistsQueryParameter, RemoteValidationResponseViewModel>(
                    layerSubDomainExistsQueryParameter);
            if (result.Response)
                return Json(false, JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ChangeCustomerCompaniesToAnotherProvisionalAccount(string model)
        {
            var serializer = new JavaScriptSerializer();
            var viewModel = serializer.Deserialize<
                CompanyViewModelLong>(model);
            var command = new ReassignUserToAnotherProvisionalCompanyCommand();
            List<Guid> list = viewModel.CompanyList.Select(company => company.Id).ToList();
            command.CustomerCompanyGuidList = list;
            command.ToProvisionalCompanyId = viewModel.ToSelectedProvisionalCompany;
            var response = ExecuteCommand(command);
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteProvisionalCompany(Guid id)
        {
            var customerUserQueryParameter = new CustomerCompanyQueryParameter
            {
                Id = id,
                IsForAdmin = true
            };

            ProvisionalCompanyUserViewModel model = ExecuteQuery
                <CustomerCompanyQueryParameter, ProvisionalCompanyUserViewModel>(
                    customerUserQueryParameter);
            var custMgmmtController = new CustomerMgmtController();
            var success = false;
            foreach (CompanyViewModel customerCompany in model.CompanyList)
            {
                success = custMgmmtController.DropCustomerDb(customerCompany.Id);
                if (!success)
                    break;
            }
            if (success || model.CompanyList.Count == 0)
            {
                var deleteProvisionalCompanyCommand = new DeleteProvisionalCompanyCommandParameter
                {
                    ProvisionalComapanyId = id
                };
                ExecuteCommand(deleteProvisionalCompanyCommand);
                success = true;
            }
            if (!success)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteProvisionalCompanyUser(Guid userId)
        {
            var deleteProvisionalCompanyCommand = new DeleteProvisionalCompanyUserCommandParameter
            {
                ProvisionalComapanyUserId = userId,
            };
            ExecuteCommand(deleteProvisionalCompanyCommand);

            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        // GET: /ProvisionalManagement/ProvisionalMgmt/Details/5
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

        // GET: /ProvisionalManagement/ProvisionalMgmt/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ProvisionalManagement/ProvisionalMgmt/Create
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

        // GET: /ProvisionalManagement/ProvisionalMgmt/Edit/5
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

        // POST: /ProvisionalManagement/ProvisionalMgmt/Edit/5
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

        // GET: /ProvisionalManagement/ProvisionalMgmt/Delete/5

        // POST: /ProvisionalManagement/ProvisionalMgmt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            User user = db.UserSet.Find(id);
            db.UserSet.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult UpdatePassword(FormCollection frm)
        {
            string hdUserId = frm["hdUserId"];
            string txtPassword = frm["txtPassword"];
            var userId = new Guid(hdUserId);
            if (hdUserId != null)
            {
                List<User> user = db.Users.Where(em => em.Id == userId).ToList();
                if (user != null)
                {
                    user[0].Password = new EncryptionHelper().Encrypt(txtPassword);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult CustomerExpiryDateReport()
        {
            var userActivityStatsViewModel = new UserActivityStatsViewModel();
            //When user logged in as Master layer Admin

            //var allProvisionalCompanies =
            //    ExecuteQuery<AllProvisionalUserQueryParameter, CompanyViewModel>(
            //        new AllProvisionalUserQueryParameter());

            //userActivityStatsViewModel.ProvisionalCompanyDropDown =
            //    allProvisionalCompanies.CompanyList.Select(c => new SerializableSelectListItem
            //    {
            //        Text = c.Name,
            //        Value = c.Id.ToString()
            //    });

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
            CompanyViewModelLong customerCompanyViewModel =
              ExecuteQuery<CustomerCompanyQueryParameter, CompanyViewModelLong>(customerCompanyQueryParameter);

            userActivityStatsViewModel.CustomerCompanyDropDown =
               customerCompanyViewModel.CompanyList.Select(c => new SerializableSelectListItem
               {
                   Text = c.CompanyName,
                   Value = c.Id.ToString()
               });

            return View(userActivityStatsViewModel);
        }

        [HttpPost]
        public PartialViewResult GetCustomerExpiryDateReport(string companyId, bool IsMonthly, bool IsYearly, int ExpireInXDays, bool IsAutoExpire)
        {
            try
            {
                CompanyViewModelLong Model = new CompanyViewModelLong();

                if (companyId != null)
                {
                    var queryParameter = new CompanyExpiryDateReoprtQueryParameter();

                    if (Thread.CurrentPrincipal.IsInResellerRole())
                    {
                        queryParameter.IsReseller = SessionManager.GetCurrentlyLoggedInUserId();
                    }
                    if (companyId == "")
                    {
                        queryParameter.CompanyId = Guid.Empty;
                    }
                    else
                    {
                        queryParameter.CompanyId = Guid.Parse(companyId);
                    }

                    queryParameter.IsMonthly = IsMonthly;
                    queryParameter.IsYearly = IsYearly;
                    queryParameter.ExpireInXDays = ExpireInXDays;
                    queryParameter.AutoExpire = IsAutoExpire;

                    Model = ExecuteQuery
                      <CompanyExpiryDateReoprtQueryParameter, CompanyViewModelLong>(
                        queryParameter);
                }
                return PartialView(Model);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // code for orp-81 self provision

        //[HttpPost]
        //public JsonResult ChangeSelfProvisionStatus(string status)
        //{
        //    var updateChangeSelfProvisionStatusStatus = new UpdateChangeSelfProvisionStatusStatusCommand
        //    {
        //        IsForSelfProvision = status == "true"
        //    };
        //    ExecuteCommand(updateChangeSelfProvisionStatusStatus);
        //    NotifyInfo("User status updated successfully");
        //    return null;
        //}
    }
}