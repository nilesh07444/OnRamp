using Data.EF;
using Domain.Enums;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Web.UI.Controllers
{
    [AllowAnonymous]
    public class DataMigrationController : RampController
    {
        private const string _SuccessView = "Success";

        #region Tests

        #region TestVersionNumber

        public ActionResult TestVersionNumber()
        {
            MainContext mainDb = new MainContext();
            var customerCompanies =
                mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
            foreach (var company in customerCompanies)
            {
                PortalContext.Override(company.CompanyId);
                ExecuteCommand(new AddVersionNumberOnTestsCommandParameter());
            }
            return View(_SuccessView);
        }

        #endregion TestVersionNumber

        #region DeleteOrphanTests

        public ActionResult DeleteOrphanTests()
        {
            MainContext mainDb = new MainContext();
            var customerCompanies =
                mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
            foreach (var company in customerCompanies)
            {
                PortalContext.Override(company.CompanyId);
                ExecuteCommand(new DeleteOrphanTests());
            }
            return View(_SuccessView);
        }

        #endregion DeleteOrphanTests

        #region MigrateTestResultsExpansion

        public ActionResult MigrateTestResultsExpansion()
        {
            MainContext mainDb = new MainContext();
            var customerCompanies =
                mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
            foreach (var company in customerCompanies)
            {
                PortalContext.Override(company.CompanyId);
                ExecuteCommand(new MigrateTestResultsExpansionCommandParameter());
            }
            return View(_SuccessView);
        }

        #endregion MigrateTestResultsExpansion

        #region DeleteOrphanTestResults

        public ActionResult DeleteOrphanTestResults()
        {
            MainContext mainDb = new MainContext();
            var customerCompanies =
                mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
            foreach (var company in customerCompanies)
            {
                PortalContext.Override(company.CompanyId);
                ExecuteCommand(new DeleteOrphanTestResultsCommandParameter());
            }
            return View(_SuccessView);
        }

        #endregion DeleteOrphanTestResults

        public ActionResult CreateTestCerts(Guid companyId)
        {
            PortalContext.Override(companyId);
            //call the command
            return View(_SuccessView);
        }

        #endregion Tests
    }
}