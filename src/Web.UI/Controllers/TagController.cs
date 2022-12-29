using Common.Command;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TrainingLabel;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Web.UI.Controllers {
	//[PortalContextActionFilter]
	public class TagController:RampController   //: KnockoutCRUDController<LabelListQuery, TrainingLabelListModel, Label,TrainingLabelModel,CreateOrUpdateTrainingLableCommand>
    {
		public ActionResult Index() {

			var model = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
			return View(model);
		}
		/// <summary>
		/// this is used to filter the tags
		/// </summary>
		/// <param name="searchText"></param>
		/// <returns></returns>
		public ActionResult FilterTags(string searchText) {
			
			var model = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
			if (!string.IsNullOrEmpty(searchText)) {
				var search = searchText.ToLower();
				model = model.Where(c => c.Name.ToLower().Contains(search)).ToList();
			}
			return PartialView("_TrainingLabelList", model);
		}

		/// <summary>
		/// this is one used show to the modal to add/edit the TrainingLabel
		/// </summary>
		/// <param name="id">this is for TrainingLabelId</param>
		/// <returns></returns>
		public ActionResult AddEditTrainingLabel(string id) {
			var model = new TrainingLabelModel();
			if (!string.IsNullOrEmpty(id)) {

				 model = ExecuteQuery<FetchByIdQuery, TrainingLabelModel>(new FetchByIdQuery() { Id=id});
			}
			return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		public ActionResult CheckDuplicateTag (string tagName) {
			var check = false;
			var model = ExecuteQuery<FetchByNameQuery, TrainingLabelModel>(new FetchByNameQuery() { Name=tagName});
			if ( !string.IsNullOrEmpty(model.Name))
				check = true;
			return new JsonResult { Data = check, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		/// <summary>
		/// this one is used to Add/update the Label
		/// </summary>
		/// <param name="id">LabelId</param>
		/// <param name="name">Label Name</param>
		/// <param name="description">Label Description</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddEditTrainingLabel(string id,string name ,string description) {
			var label = new CreateOrUpdateTrainingLableCommand {
				Id=id,Name=name,Description=description
			};
			var response = ExecuteCommand(label);
			var model = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
			return PartialView("_TrainingLabelList", model);
		}
		/// <summary>
		/// this one is used to delete the Label based on selection
		/// </summary>
		/// <param name="id">selected Label Id that's record need to delete from table</param>
		/// <returns></returns>
		[HttpPost] 
		public ActionResult DeleteTrainingLabel(string id) {
			var result = ExecuteCommand(new DeleteByIdCommand<Label> { Id = id });
			
			var model = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
			return PartialView("_TrainingLabelList", model);
		}


	}
}