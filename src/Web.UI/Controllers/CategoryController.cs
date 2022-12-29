using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Categories;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Ramp.Contracts.Command.DocumentCategory;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Domain.Customer;
using Microsoft.Win32.TaskScheduler;
using System.Threading.Tasks;


namespace Web.UI.Controllers
{
    public class CategoryController : RampController
    {
        public ActionResult ManageCategoryModalPartial()
        {
            return PartialView();
        }

        [HttpGet]
        public JsonResult JsTree()
        {
            var categories = ExecuteQuery<AllCategoriesQueryParameter, List<CategoryViewModel>>(
                new AllCategoriesQueryParameter
                {
                    CompanyId = PortalContext.Current.UserCompany.Id
                }).Select(c =>
                new JSTreeViewModel
                {
                    text = c.CategorieTitle.Length > 100 ? c.CategorieTitle.Substring(0, 100) : c.CategorieTitle,
                    id = c.Id.ToString(),
                    parent = c.ParentCategoryId.HasValue ? c.ParentCategoryId.Value.ToString() : "#"
                }).ToArray();
            return new JsonResult { Data = categories, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public async Task<JsonResult> JsTreeWithDocuments()
        {
            var root = new List<dynamic>
            {
                new
                {
                    text = "Category",
                    id = Guid.Empty.ToString(),
                    parent = "#",
                    state = new
                    {
                        opened = true
                    }
                }
            };

            var categories =
                ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery())
                    .Select(c =>
                    {
                        if (c.parent == "#")
                        {
                            c.parent = Guid.Empty.ToString();
                        }
                        return c;
                    });

            var documents = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery() { EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument })
                .Select(d => new JSTreeViewModel
                {
                    text = d.Title,
                    id = d.Id,
                    parent = (d.Category != null ? d.Category.Id : null),
                    type = d.DocumentType.ToString()
                }).ToArray();


            IEnumerable<dynamic> result = categories.Concat(documents).Concat(root);
            return await System.Threading.Tasks.Task.FromResult(new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        public JsonResult Create(string id, string parentId, string title)
        {
            if (parentId == Guid.Empty.ToString())
            {
                parentId = null;
            }

            var result = ExecuteCommand(new CreateDocumentCategoryCommand
            {
                Id = id,
                ParentId = parentId,
                Title = title
            });

            if (result.ErrorMessage != null || result.Validation.Any())
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new JsonResult { Data = result };
            }

            return null;
        }

        [HttpPut]
        public JsonResult Rename(string id, string title)
        {
            var result = ExecuteCommand(new RenameDocumentCategoryCommand
            {
                Id = id,
                Title = title
            });

            if (result.ErrorMessage != null || result.Validation.Any())
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new JsonResult { Data = result };
            }

            return null;
        }

        [HttpDelete]
        public JsonResult Delete(string id)
        {
            var result = ExecuteCommand(new DeleteDocumentCategoryCommand
            {
                Id = id
            });

            if (result.ErrorMessage != null || result.Validation.Any())
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new JsonResult { Data = result };
            }

            return null;
        }

        [HttpPut]
        public JsonResult UpdateNodeParent(string id, string parentId, string type)
        {
            if (type == "default")
            {
                // category move
                var result = ExecuteCommand(new DocumentCategoryUpdateParentCommand
                {
                    Id = id,
                    ParentId = parentId == Guid.Empty.ToString() ? null : parentId
                });
                if (result.ErrorMessage != null || result.Validation.Any())
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return new JsonResult { Data = result };
                }
            }
            else
            {
                DocumentType docType;
                if (Enum.TryParse(type, out docType))
                {
                    // document move
                    var result = ExecuteCommand(new DocumentUpdateCategoryCommand
                    {
                        Id = id,
                        CategoryId = parentId,
                        DocumentType = docType
                    });
                    if (result.ErrorMessage != null || result.Validation.Any())
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return new JsonResult { Data = result };
                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }

            return null;
        }
    }
}