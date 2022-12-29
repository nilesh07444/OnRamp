using Data.EF;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Attributes;
using Ramp.Security.Authorization;
using Ramp.Services.CommandHandler;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Web.UI.Code.Extensions;
using RampController = Web.UI.Controllers.RampController;

namespace Web.UI.Areas.Configurations.Controllers
{
    public class ManageGroupsController : RampController
    {
        private readonly MainContext db = new MainContext();

        public ActionResult Index(Guid? id, Guid? companyid)
        {
            Session["IS_GROUP_EDITED"] = false;
            var groupQueryParameter = new GroupQueryParameter();
            var companyViewModel = new CompanyViewModel();

            if (id != null && id != Guid.Empty)
            {
                Session["IS_GROUP_EDITED"] = true;
                groupQueryParameter.GroupId = id;
            }
            var groupViewModelLong = new GroupViewModelLong();

            if (id != null && id != Guid.Empty)
            {
                Session["IS_GROUP_EDITED"] = true;
                groupQueryParameter.GroupId = id;
            }

            if (!companyid.HasValue)
            {
                companyid = PortalContext.Current.UserCompany.Id;
            }

            groupQueryParameter.CompanyId = companyid.Value;
            PortalContext.Override(companyid.Value);
            groupViewModelLong = ExecuteQuery<GroupQueryParameter, GroupViewModelLong>(groupQueryParameter);

            if (companyid.HasValue)
            {
                if (groupViewModelLong.GroupViewModel == null)
                {
                    groupViewModelLong.GroupViewModel = new GroupViewModel { SelectedCustomerCompanyId = companyid.Value };
                }
                else
                {
                    groupViewModelLong.GroupViewModel.SelectedCustomerCompanyId = companyid.Value;
                }
            }

            return View(groupViewModelLong);
        }

        [HttpGet]
        public JsonResult CheckForGropNameForACompanySelectedInDropdown(Guid companyId, string groupTitle)
        {
            var model = new GroupViewModelLong
            {
                GroupViewModel = new GroupViewModel
                {
                    Title = groupTitle,
                    SelectedCustomerCompanyId = companyId
                }
            };

            if (Session["IS_GROUP_EDITED"] != null && !(bool)Session["IS_GROUP_EDITED"])
            {
                var roles = SessionManager.GetRolesForCurrentlyLoggedInUser().ToList();
                var groupNameExistQueryParameter = new GroupNameExistQueryParameter
                {
                    GroupName = model.GroupViewModel.Title.Trim(),
                };

                if (Ramp.Contracts.Security.Role.IsInResellerRole(roles) || Ramp.Contracts.Security.Role.IsInGlobalAdminRole(roles))
                {
                    groupNameExistQueryParameter.CompanyId = model.GroupViewModel.SelectedCustomerCompanyId;
                }
                RemoteValidationResponseViewModel result =
                    ExecuteQuery<GroupNameExistQueryParameter, RemoteValidationResponseViewModel>(
                        groupNameExistQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DoesGroupNameAlreadyPresent(GroupViewModelLong model)
        {
            if (Session["IS_GROUP_EDITED"] != null && !(bool)Session["IS_GROUP_EDITED"])
            {
                var groupNameExistQueryParameter = new GroupNameExistQueryParameter
                {
                    GroupName = model.GroupViewModel.Title,
                    CompanyId = PortalContext.Current.UserCompany.Id
                };

                RemoteValidationResponseViewModel result =
                    ExecuteQuery<GroupNameExistQueryParameter, RemoteValidationResponseViewModel>(
                        groupNameExistQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateOrUpdateGroup(GroupViewModelLong groupViewModelLong)
        {
            if (ModelState.IsValid)
            {
                var roles = SessionManager.GetRolesForCurrentlyLoggedInUser().ToList();
                var globalAdminOrReseller = Ramp.Contracts.Security.Role.IsInResellerRole(roles) || Ramp.Contracts.Security.Role.IsInGlobalAdminRole(roles);

                var saveOrUpdateGroupCommand = new SaveOrUpdateGroupCommand
                {
                    Title = groupViewModelLong.GroupViewModel.Title.Trim(),
                    Description = groupViewModelLong.GroupViewModel.Description,
                    IsforSelfSignUpGroup = groupViewModelLong.GroupViewModel.IsforSelfSignUpGroup,
                    GroupId = groupViewModelLong.GroupViewModel.GroupId,
                    CompanyId = globalAdminOrReseller
                        ? groupViewModelLong.GroupViewModel.SelectedCustomerCompanyId
                        : PortalContext.Current.UserCompany.Id
                };
                if (Session["IS_GROUP_EDITED"] != null)
                {
                    saveOrUpdateGroupCommand.AttemptCreate = !(bool)(Session["IS_GROUP_EDITED"]);
                }

                var response = ExecuteCommand(saveOrUpdateGroupCommand);
                if (response.Validation.Any())
                {
                    NotifyError(response.Validation.First().Message);
                }
                if (Session["IS_GROUP_EDITED"] != null)
                {
                    if ((bool)(Session["IS_GROUP_EDITED"]))
                    {
                        NotifyInfo("Group successfully updated");
                    }
                    else
                    {
                        NotifySuccess("Group successfully created");
                    }
                }
            }
            return RedirectToAction("Index",
                new { companyId = groupViewModelLong.GroupViewModel.SelectedCustomerCompanyId });
        }

        [HttpPost]
        public JsonResult DeleteGroup(Guid id)
        {
            var deleteGroupCommand = new DeleteGroupCommand
            {
                GroupId = id
            };
            var response = ExecuteCommand(deleteGroupCommand);

            if (response.Validation.Any())
            {
                NotifyError(response.Validation.First().Message);
                return Json(new { Status = 'F' }, JsonRequestBehavior.AllowGet);
            }

            NotifySuccess("Successfully deleted");
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Configurations/ManageGroups/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.GroupSet.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // GET: /Configurations/ManageGroups/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Configurations/ManageGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description")] Group group)
        {
            if (ModelState.IsValid)
            {
                group.Id = Guid.NewGuid();
                db.GroupSet.Add(group);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(group);
        }

        // GET: /Configurations/ManageGroups/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.GroupSet.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: /Configurations/ManageGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(group);
        }

        // GET: /Configurations/ManageGroups/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.GroupSet.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: /Configurations/ManageGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Group group = db.GroupSet.Find(id);
            db.GroupSet.Remove(group);
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
    }
}