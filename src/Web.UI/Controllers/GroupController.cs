
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.Command.User;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.CommandHandler.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Web.UI.Controllers
{
	public class GroupController : RampController {
		// GET: Group
		public ActionResult Index() {
			return View();
		}

		[HttpGet]
		public JsonResult JsTree() {
			var groups = ExecuteQuery<EmptyQueryParameter, IEnumerable<GroupViewModel>>(
				new EmptyQueryParameter {

				}).Select(c =>
					new JSTreeViewModel {
						text = c.Title.Length > 100 ? c.Title.Substring(0, 100) : c.Title,
						id = c.GroupId.ToString(),
						parent = c.ParentGroupId.HasValue ? c.ParentGroupId.Value.ToString() : "#"
					}).Where(c=>c.text!=null).ToArray();
			return new JsonResult { Data = groups, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public JsonResult JsTreeWithGroups() {
			var root = new List<dynamic>
			{
				new
				{
					text = "Groups",
					id = Guid.Empty.ToString(),
					parent = "#",
					state = new
					{
						opened = true
					}
				}
			};

			var groups =
				 ExecuteQuery<CustomerGroup, IEnumerable<JSTreeViewModel>>(new CustomerGroup())
					.Select(c => {
						if (c.parent == "#") {
							c.parent = Guid.Empty.ToString();
						}

						return c;
					}).Where(c=>c.text!=null);
			var query = new CustomerCompanyUserQueryParameter {
				CompanyId = PortalContext.Current.UserCompany.Id,
				CompanyName=PortalContext.Current.UserCompany.CompanyName,
				LoggedInUserId=SessionManager.GetCurrentlyLoggedInUserId()

			};

			CompanyUserViewModel companyUserViewModel =
			ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(query);

			//var users = companyUserViewModel.UserList.Select(d => new JSTreeViewModel {
			//	text = d.FullName,
			//	id = d.Id.ToString(),
			//	parent = d.SelectedGroupId.ToString(),
			//	type= "user"
			//}).ToArray();

			//var documents = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery())
			//	.Select(d => new JSTreeViewModel {
			//		text = d.Title,
			//		id = d.Id,
			//		parent = d.Category.Id,
			//		type = d.DocumentType.ToString()
			//	}).ToArray();
			
			IEnumerable<dynamic> result = groups.Concat(root);
			return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public JsonResult Create(string id, string parentId, string title) {
			if (parentId == Guid.Empty.ToString()) {
				parentId = null;
			}

			var result = ExecuteCommand(new SaveOrUpdateGroupCommand {
				GroupId = Guid.Parse(id),
				ParentId = parentId,
				Title = title
			});

			if (result.ErrorMessage != null || result.Validation.Any()) {
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new JsonResult { Data = result };
			}

			return null;
		}

		[HttpPut]
		public JsonResult Rename(string id, string title, string parentId) {
			var result = ExecuteCommand(new SaveOrUpdateGroupCommand {
				GroupId = Guid.Parse(id),
				Title = title,
				ParentId = parentId
			});

			if (result.ErrorMessage != null || result.Validation.Any()) {
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new JsonResult { Data = result };
			}

			return null;
		}

		[HttpDelete]
		public JsonResult Delete(string id) {
			var result = ExecuteCommand(new DeleteGroupCommand {
				GroupId = Guid.Parse(id)
			});

			if (result.ErrorMessage != null || result.Validation.Any()) {
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new JsonResult { Data = result };
			}

			return null;
		}

		[HttpPut]
		public JsonResult UpdateNodeParent(string id, string parentId, string type) {
			try {

			var userDetail= ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery {Id=id });

				if (userDetail != null) {
					// user move
					var result1 = ExecuteCommand(new UserUpdateGroupCommand {
						Id = id,
						GroupId = parentId
					});
				} else {
					// category move
					var result = ExecuteCommand(new GroupUpdateParentCommand {
						Id = id,
						ParentId = parentId == Guid.Empty.ToString() ? null : parentId
					});
					if (result.ErrorMessage != null || result.Validation.Any()) {
						Response.StatusCode = (int)HttpStatusCode.BadRequest;
						return new JsonResult { Data = result };
					}

				}
				#region Need to work 

				//if (type == "default") {
				//	// category move
				//	var result = ExecuteCommand(new GroupUpdateParentCommand {
				//		Id = id,
				//		ParentId = parentId == Guid.Empty.ToString() ? null : parentId
				//	});
				//	if (result.ErrorMessage != null || result.Validation.Any()) {
				//		Response.StatusCode = (int)HttpStatusCode.BadRequest;
				//		return new JsonResult { Data = result };
				//	}
				//} else {
				//	var result = ExecuteCommand(new SaveOrUpdateGroupCommand {
				//		GroupId = Guid.Parse(id),
				//		ParentId = parentId,

				//	});
				//	if (result.ErrorMessage != null || result.Validation.Any()) {
				//		Response.StatusCode = (int)HttpStatusCode.BadRequest;
				//		return new JsonResult { Data = result };
				//	}

				//}
				#endregion
			}
			catch (Exception e) {

				throw e;
			}

			return null;
		}


		[HttpPost]
		public JsonResult UpdateGroup(string id) {
			try {
				var group = ExecuteQuery<FetchByIdQuery, CustomerGroup>(new FetchByIdQuery {
					Id = Guid.Parse(id)
				});
				var result = ExecuteCommand(new SaveOrUpdateGroupCommand {
					GroupId = Guid.Parse(id),
					IsforSelfSignUpGroup = true,
					ParentId = group.ParentId,
					Title = group.Title,
					Description = group.Description,
					CompanyId = group.CompanyId
				});

				if (result.ErrorMessage != null || result.Validation.Any()) {
					Response.StatusCode = (int)HttpStatusCode.BadRequest;
					return new JsonResult { Data = result };
				}
			}
			catch (Exception e) {

				throw e;
			}

			return null;
		}

		public JsonResult GetGroup() {
			//var group = ExecuteQuery<FetchByIdQuery, CustomerGroup>(new FetchByIdQuery {
			//	Id = Guid.Parse(id)
			//});
		

			return null;
		}
	}
}