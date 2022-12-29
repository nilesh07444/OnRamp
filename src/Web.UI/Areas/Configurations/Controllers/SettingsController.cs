using Common.Command;
using Data.EF;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Settings;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Attributes;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RampController = Web.UI.Controllers.RampController;

namespace Web.UI.Areas.Configurations.Controllers
{
    public class SettingsController : RampController
    {
        private readonly MainContext db = new MainContext();

        // GET: /Configurations/Settings/

        public ActionResult Index()
        {
            return View(db.SettingSet.ToList());
        }

        // GET: /Configurations/Settings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setting setting = db.SettingSet.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        // GET: /Configurations/Settings/Create
        public ActionResult Create()
        {
            var settingQueryParameter = new SettingQueryParameter();
            SettingViewModel result = ExecuteQuery<SettingQueryParameter, SettingViewModel>(settingQueryParameter);
            return View(result);
        }

        // POST: /Configurations/Settings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,PasswordPolicy")] Setting setting)
        {
            if (ModelState.IsValid)
            {
                db.SettingSet.Add(setting);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(setting);
        }

        // GET: /Configurations/Settings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setting setting = db.SettingSet.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        // POST: /Configurations/Settings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,PasswordPolicy")] Setting setting)
        {
            if (ModelState.IsValid)
            {
                db.Entry(setting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(setting);
        }

        // GET: /Configurations/Settings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setting setting = db.SettingSet.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        // POST: /Configurations/Settings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Setting setting = db.SettingSet.Find(id);
            db.SettingSet.Remove(setting);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdateSetting(SettingViewModel settingModel)
        {
            var settingCommandParameter = new SettingCommandParameter
            {
                Id = settingModel.SettingViewModelShort.Id,
                PasswordPolicy = settingModel.SettingViewModelShort.PasswordPolicy,
            };
            ExecuteCommand(settingCommandParameter);
            NotifySuccess("password policy has been updated successfully.");
            return RedirectToAction("Create");
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