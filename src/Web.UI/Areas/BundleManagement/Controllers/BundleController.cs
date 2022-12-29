using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.EF;
using Domain.Models;
using Ramp.Contracts.Command.Bundle;
using Ramp.Contracts.CommandParameter.PackageManagement;
using Ramp.Contracts.Query.Bundle;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.PackageManagement;
using Ramp.Contracts.ViewModel;
using Web.UI.Areas.CustomerManagement.Controllers;
using RampController = Web.UI.Controllers.RampController;

namespace Web.UI.Areas.BundleManagement.Controllers
{
    public class BundleController : RampController
    {
        private readonly MainContext db = new MainContext();

        // GET: /BundleManagement/Bundle/
        public ActionResult Index(string id)
        {
            //return View(db.PackageSet.ToList());
            //var packageQueryParameter = new PackageQueryParameter();
            //if (id != null && id != Guid.Empty)
            //{
            //    packageQueryParameter.id = id;
            //}

            var bundleQuery = new BundleQuery();
            if (id != null && id != Guid.Empty.ToString())
            {
                bundleQuery.Id = id;
            }

            //PackageViewModel result = ExecuteQuery<PackageQueryParameter, PackageViewModel>(packageQueryParameter);
            var result = ExecuteQuery<BundleQuery, BundleViewModel>(bundleQuery);
            //var packageCommandParameter = new PackageCommandParameter();
            //var response = ExecuteCommand(packageCommandParameter);
            return View(result);
        }

        // GET: /PackageManagement/Package/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Package package = db.PackageSet.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }
            return View(package);
        }

        // GET: /PackageManagement/Package/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /PackageManagement/Package/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "MyId,Title,Description,MaxNumberOfGuides,MaxNumberOfChaptersPerGuide")] Package package)
        {
            if (ModelState.IsValid)
            {
                db.PackageSet.Add(package);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(package);
        }

        // GET: /PackageManagement/Package/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Package package = db.PackageSet.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }
            return View(package);
        }

        // POST: /PackageManagement/Package/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "MyId,Title,Description,MaxNumberOfGuides,MaxNumberOfChaptersPerGuide")] Package package)
        {
            if (ModelState.IsValid)
            {
                db.Entry(package).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(package);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            var model = ExecuteQuery<CustomerCompanyByBundleIdQuery, ProvisionalCompanyUserViewModel>(
                new CustomerCompanyByBundleIdQuery
                {
                    BundleId = id
                });

            var custMgmmtController = new CustomerMgmtController();
            foreach (var customerCompany in model.CompanyList)
            {
                custMgmmtController.DropCustomerDb(customerCompany.Id);
            }
            var response = ExecuteCommand(new DeleteBundleCommand { BundleId = id });
            if (response.Validation.Any())
            {
                NotifyError(response.Validation.First().Message);
                return Json(new { Status = 'F' }, JsonRequestBehavior.AllowGet);
            }

            NotifySuccess("Successfully deleted");
            return Json(new { Status = 'S' }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        /* [ValidateAntiForgeryToken]*/
        public ActionResult CreateOrUpdateBundle(BundleViewModel packageModel)
        {
            var response = ExecuteCommand(new CreateOrUpdateBundleCommand
            {
                Id = packageModel.BundleViewModelShort.Id,
                Title = packageModel.BundleViewModelShort.Title,
                Description = packageModel.BundleViewModelShort.Description,
                MaxNumberOfDocuments = packageModel.BundleViewModelShort.MaxNumberOfDocuments,
                IsForSelfProvision = packageModel.BundleViewModelShort.IsForSelfProvision
            });
            NotifySuccess("Bundle successfully saved");
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