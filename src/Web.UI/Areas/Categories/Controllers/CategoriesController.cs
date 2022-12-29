using Data.EF;
using Data.EF.Customer;
using Domain.Enums;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Categories;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.Security;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Attributes;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Web.UI.Code.Cache;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;
using RampController = Web.UI.Controllers.RampController;

namespace Web.UI.Areas.Categories.Controllers
{
    public class CategoriesController : RampController
    {
        private readonly MainContext db = new MainContext();

        private CustomerContext _db = new CustomerContext();

        private JsTreeModel rootTree = new JsTreeModel();

        private List<CategoryViewModel> CategoryList;
        private List<CategoryViewModel> CategoryListReport = new List<CategoryViewModel>();

        public ActionResult Index(Guid? id)
        {
            Session["IS_CATEGORY_EDITED"] = false;
            var categoryQueryParameter = new CategoriesQueryParameter();
            if (id != null && id != Guid.Empty)
            {
                Session["IS_CATEGORY_EDITED"] = true;
                categoryQueryParameter.Id = id;
            }
            CategoryViewModelLong result =
                ExecuteQuery<CategoriesQueryParameter, CategoryViewModelLong>(categoryQueryParameter);

            CategoryList = result.CategoryViewModelList;
            var parentDr = CategoryList.Where(c => c.ParentCategoryId == null || c.ParentCategoryId == Guid.Empty).FirstOrDefault();
            rootTree.data = "Category";
            rootTree.attr.id = Guid.Empty;
            rootTree.id = Guid.Empty;
            rootTree.state = new JsTreeState { opened = true, selected = true };
            PopulateTree(rootTree, null);
            var model = rootTree;
            string jsonModel = new JavaScriptSerializer().Serialize(model);

            return View("Index", "_LayoutStandardUser", jsonModel);
        }

        [HttpGet]
        public string GetTree()
        {
            CategoryViewModelLong result =
                ExecuteQuery<CategoriesQueryParameter, CategoryViewModelLong>(new CategoriesQueryParameter());

            CategoryList = result.CategoryViewModelList;
            rootTree.data = "Category";
            rootTree.attr.id = Guid.Empty;
            rootTree.id = Guid.Empty;
            rootTree.state = new JsTreeState { opened = true, selected = true };
            rootTree.children = new List<JsTreeModel>();
            PopulateTree(rootTree, null);

            return new JavaScriptSerializer().Serialize(rootTree);
        }

        [HttpPost]
        public ActionResult CreateOrUpdateCategories(CategoryViewModelLong categoryModel)
        {
            var catagoriesCommandParameter = new CategoriesCommandParameter
            {
                Id = categoryModel.CategorieViewModel.Id,
                CategoryTitle = categoryModel.CategorieViewModel.CategorieTitle.Trim(),
                Description = categoryModel.CategorieViewModel.Description,
                CreatedUnderCompanyId = PortalContext.Current.UserCompany.Id
            };
            ExecuteCommand(catagoriesCommandParameter);
            if (Session["IS_CATEGORY_EDITED"] != null)
            {
                if ((bool)(Session["IS_CATEGORY_EDITED"]))
                {
                    NotifyInfo("Category successfully updated");
                }
                else
                {
                    NotifySuccess("Category successfully created");
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult DoesCategoryNameAlreadyPresent(CategoryViewModelLong model)
        {
            if (Session["IS_CATEGORY_EDITED"] != null && !(bool)Session["IS_CATEGORY_EDITED"])
            {
                var categoryNameExistQueryParameter = new CategoryNameExistsQueryParameter
                {
                    CategoryName = model.CategorieViewModel.CategorieTitle.Trim()
                };

                RemoteValidationResponseViewModel result =
                    ExecuteQuery<CategoryNameExistsQueryParameter, RemoteValidationResponseViewModel>(
                        categoryNameExistQueryParameter);
                if (result.Response)
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit(Guid? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //Categories categories = db.CategorieSet.Find(id);
            //if (categories == null)
            //{
            //    return HttpNotFound();
            //}
            return View();
        }

        public ActionResult ViewCategoryStatistics(CategoryStatisticsReportQueryParameter query)
        {
            var overriden = false;
            if (Thread.CurrentPrincipal.IsInGlobalAdminRole()  || Thread.CurrentPrincipal.IsInResellerRole())
            {
                if (query.SelectedCompanyId != Guid.Empty)
                {
                    PortalContext.Override(query.SelectedCompanyId);
                    Session["OverridenCompanyIdCategory"] = query.SelectedCompanyId;
                    overriden = true;
                }
            }
            if (Thread.CurrentPrincipal.IsInAdminRole() || overriden)
            {
                CategoryViewModelLong result =
                    ExecuteQuery<CategoriesQueryParameter, CategoryViewModelLong>(new CategoriesQueryParameter
                    {
                        Id = query.SelectedCategoryId
                    });

                if (result.CategorieViewModel != null)
                {
                    ViewBag.CategoryId = "Category : " + result.CategorieViewModel.CategorieTitle;
                }
                CategoryList = result.CategoryViewModelList;
                PopulateTreeReport(rootTree, query.CategoryId);
                query.CatList = CategoryListReport;
                CategoryListReport = null;
            }

            ViewBag.Query = query;

            var vm = ExecuteQuery<CategoryStatisticsReportQueryParameter, CategoryStatisticsReportViewModel>(query);
            vm.Categories = CategoryList;
            if (Thread.CurrentPrincipal.IsInGlobalAdminRole() || Thread.CurrentPrincipal.IsInResellerRole())
            {
                if (query.SelectedCompanyId != Guid.Empty)
                {
                    if (query.SelectedCategoryId != Guid.Empty)
                    {
                        vm.SelectedCategoryId = query.SelectedCategoryId;
                    }
                }
            }
            CategoryList = null;
            return View(vm);
        }

        public void PopulateTree(JsTreeModel parentNode, Guid? parentId)
        {
            List<CategoryViewModel> children = CategoryList.Where(c => c.ParentCategoryId == parentId).OrderBy(o => o.CategorieTitle).ToList();

            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    JsTreeModel node = new JsTreeModel();
                    node.attr.id = child.Id;
                    node.data = child.CategorieTitle;
                    node.id = child.Id;
                    parentNode.children.Add(node);
                    PopulateTree(node, child.Id);
                }
            }
        }

        public void PopulateTreeReport(JsTreeModel parentNode, Guid? parentId)
        {
            List<CategoryViewModel> children = CategoryList.Where(c => c.ParentCategoryId == parentId).ToList();

            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    JsTreeModel node = new JsTreeModel();
                    node.attr.id = child.Id;
                    node.data = child.CategorieTitle;
                    parentNode.children.Add(node);
                    CategoryListReport.Add(child);
                    PopulateTreeReport(node, child.Id);
                }
            }
        }

        // new code-------------------------------
        [HttpPost]
        public ActionResult Create(string ABC, Guid id, Guid? newid)
        {
            var CategoryNameAlreadyExists = ExecuteQuery<CategoryNameExistsQueryParameter, RemoteValidationResponseViewModel>(new CategoryNameExistsQueryParameter
            {
                CategoryName = ABC
            }).Response;

            if (CategoryNameAlreadyExists)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Category name is already in use");

            var catagoriesCommandParameter = new CategoriesCommandParameter
            {
                Id = newid ?? Guid.NewGuid(),
                CategoryTitle = ABC.Trim(),
                Description = "Hello All",
                ParentCategoryId = id,
                CreatedUnderCompanyId = PortalContext.Current.UserCompany.Id
            };
            ExecuteCommand(catagoriesCommandParameter);

            return new HttpStatusCodeResult(HttpStatusCode.Created, "Category Successfully Created");
        }

        [HttpPost]
        public ActionResult Edit(string ABC, Guid id, Guid ParentId)
        {
            var CategoryNameAlreadyExists = ExecuteQuery<CategoryNameExistsQueryParameter, RemoteValidationResponseViewModel>(new CategoryNameExistsQueryParameter
            {
                CategoryName = ABC
            }).Response;

            if (CategoryNameAlreadyExists)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Category name is already in use");

            var catagoriesCommandParameter = new CategoriesCommandParameter
            {
                Id = id,
                CategoryTitle = ABC.Trim(),
                Description = "Hello All",
                CreatedUnderCompanyId = PortalContext.Current.UserCompany.Id
            };
            ExecuteCommand(catagoriesCommandParameter);
            return null;
        }

        [HttpPost]
        public JsonResult Delete(Guid id)
        {
            var deletecategoryCommand = new DeleteCategoryCommandParamter
            {
                Id = id
            };
            var response = ExecuteCommand(deletecategoryCommand);
            if (response.Validation.Any())
            {
                return Json(new { Status = false, Message = response.Validation.First().Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeRootOfNode(Guid ParentId, Guid Id)
        {
            var catagoriesCommandParameter = new CategoriesChangeRootOfNodeCommandParameter
            {
                Id = Id,
                Description = "Hello All",
                ParentCategoryId = ParentId,
                CreatedUnderCompanyId = PortalContext.Current.UserCompany.Id
            };
            ExecuteCommand(catagoriesCommandParameter);
            return null;
        }

        public ActionResult GetCategoryReport()
        {
            if (!string.IsNullOrWhiteSpace(Session["OverridenCompanyIdCategory"]?.ToString()) && Guid.TryParse(Session["OverridenCompanyIdCategory"].ToString(), out var gId))
                PortalContext.Override(gId);
            var categoryQueryParameter = new CategoriesQueryParameter();
            categoryQueryParameter.CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId();
            CategoryViewModelLong result =
             ExecuteQuery<CategoriesQueryParameter, CategoryViewModelLong>(categoryQueryParameter);

            // return View(result);

            CategoryList = result.CategoryViewModelList;
            var parentDr = CategoryList.Where(c => c.ParentCategoryId == null || c.ParentCategoryId == Guid.Empty).FirstOrDefault();
            rootTree.data = "Category";
            rootTree.attr.id = Guid.Empty;
            PopulateTree(rootTree, null);
            var model = rootTree;

            // string jsonModel = new JavaScriptSerializer().Serialize(model);
            return Json(model, JsonRequestBehavior.AllowGet);
           // return PartialView("_CategoryMenuReport", model);
        }

        public ActionResult GetCategoryReportAdmin(Guid Id)
        {
            if (Id != Guid.Empty)
            {
                var company = db.Company.Where(c => c.Id == Id).FirstOrDefault();
                if (company != null)
                {
                    var categoryQueryParameter = new CategoriesQueryParameter();
                    categoryQueryParameter.CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId();
                    CategoryViewModelLong result =
                     ExecuteQuery<CategoriesQueryParameter, CategoryViewModelLong>(categoryQueryParameter);

                    // return View(result);

                    CategoryList = result.CategoryViewModelList.OrderBy(o => o.CategorieTitle).ToList();
                    var parentDr = CategoryList.Where(c => c.ParentCategoryId == null || c.ParentCategoryId == Guid.Empty).FirstOrDefault();
                    rootTree.data = "Category";
                    rootTree.attr.id = Guid.Empty;
                    PopulateTree(rootTree, null);
                    var model = rootTree;

                    // string jsonModel = new JavaScriptSerializer().Serialize(model);

                    if (model.children.Count > 0)
                    {
                        return PartialView("_CategoryMenuReport", model);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        // end new code ---------------------------

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