using Common.Command;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TrainingActivity;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers {
	public class TrainingActivityLogController : RampController
    {
        // GET: TrainingActivityLog
        public ActionResult Index()
        {
			var model = ExecuteQuery<FetchAllTrainingActivityLogQuery, List<TrainingActivityModel>>(new FetchAllTrainingActivityLogQuery() { }).ToList();
			var enumList = Enum.GetValues(typeof(TrainingActivityType)).OfType<TrainingActivityType>().ToList();
			List<string> dict = new List<string> {
				{ VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Bursary) },
				{  VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.External) },
				{  VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Internal) },
				{  VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.MentoringAndCoaching) },
				{ VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.ToolboxTalk) }
			};
			ViewBag.TrainingActivityTypeList = dict;
			if (TempData["Message"] != null)
				ViewBag.Message = TempData["Message"];

			return View(model);
        }
		[HttpGet]
		public ActionResult AddEditTrainingActivityLog(string id) {
			
			var model = new TrainingActivityModel();
			if (!string.IsNullOrEmpty(id)) {

				model = ExecuteQuery<FetchByIdQuery, TrainingActivityModel>(
					new FetchByIdQuery {
						Id = id
					});
				Action<UploadResultViewModel> rewriteUrls = x =>
				{
					x.Url = Url.ActionLink<UploadController>(a => a.Get(x.Id.ToString(), false));
					x.DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(x.Id.ToString(), null));
					x.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(x.Id.ToString(), 300, 300));
				};
				model.Documents.ToList().ForEach(rewriteUrls);
				switch (model.TrainingActivityType) {
					case TrainingActivityType.Bursary:
						model.BursaryTrainingActivityDetail.Invoices.ToList().ForEach(rewriteUrls);
						break;
					case TrainingActivityType.External:
						model.ExternalTrainingActivityDetail.Invoices.ToList().ForEach(rewriteUrls);
						break;
					default:
						break;
				}

			}
			var labels= ExecuteQuery<LabelListQuery, IEnumerable<TrainingLabelListModel>>(new LabelListQuery ());
			model.TrainingLabelList = new SelectList(labels, "Id", "Name");

			var trainigLabels = new List<string>();
			foreach (var item in model.TrainingLabels.Split(',')) {
				var label = ExecuteQuery<FetchByNameQuery, TrainingLabelModel>(new FetchByNameQuery() { Name = item });
				trainigLabels.Add(label.Id);
			}
			model.LabelIds = string.Join(",", trainigLabels);

			var enumList = Enum.GetValues(typeof(TrainingActivityType)).OfType<TrainingActivityType>().ToList();
			IDictionary<int, string> dict = new Dictionary<int, string> {
				{ (int)TrainingActivityType.Bursary, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Bursary) },
				{ (int)TrainingActivityType.External, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.External) },
				{ (int)TrainingActivityType.Internal, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Internal) },
				{ (int)TrainingActivityType.MentoringAndCoaching, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.MentoringAndCoaching) },
				{ (int)TrainingActivityType.ToolboxTalk, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.ToolboxTalk) }
			};
			model.TrainingActivityTypeList = new SelectList(dict, "Key", "Value");

			var users= ExecuteQuery<UserSearchQuery, IEnumerable<UserModelShort>>(new UserSearchQuery());
			model.TrainersList = new SelectList(users, "Id", "UserName");
			model.TraineesList = new SelectList(users, "Id", "UserName");
			var externalTrainingProviders = ExecuteQuery<FetchAllRecordsQuery, List<ExternalTrainingProviderListModel>>(new FetchAllRecordsQuery());
			model.ExternalTrainingProviderList = new SelectList(externalTrainingProviders,"Id", "CompanyName");
			if (model.UsersTrained.Any()) {
				model.Trainees = string.Join(",", model.UsersTrained.Select(c => c.Id).ToList()); 
			}

			switch (model.TrainingActivityType) {
				case TrainingActivityType.Bursary:
					if(model.BursaryTrainingActivityDetail!=null && model.BursaryTrainingActivityDetail.ConductedBy.Any()) {
						model.Trainers = string.Join(",", model.BursaryTrainingActivityDetail.ConductedBy.Select(c => c.Id.ToString()));
					}
					break;
				case TrainingActivityType.External:
					if (model.ExternalTrainingActivityDetail != null && model.ExternalTrainingActivityDetail.ConductedBy.Any()) {
						model.Trainers = string.Join(",", model.ExternalTrainingActivityDetail.ConductedBy.Select(c => c.Id.ToString()));
					}
					break;
				case TrainingActivityType.Internal:
					if (model.InternalTrainingActivityDetail != null && model.InternalTrainingActivityDetail.ConductedBy.Any()) {
						model.Trainers = string.Join(",", model.InternalTrainingActivityDetail.ConductedBy.Select(c => c.Id.ToString()));
					}
					break;
				case TrainingActivityType.MentoringAndCoaching:
					if (model.MentoringAndCoachingTrainingActivityDetail != null && model.MentoringAndCoachingTrainingActivityDetail.ConductedBy.Any()) {
						model.Trainers = string.Join(",", model.MentoringAndCoachingTrainingActivityDetail.ConductedBy.Select(c => c.Id.ToString()));
					}
					break;
				case TrainingActivityType.ToolboxTalk:
					if (model.ToolboxTalkTrainingActivityDetail != null && model.ToolboxTalkTrainingActivityDetail.ConductedBy.Any()) {
						model.Trainers = string.Join(",", model.ToolboxTalkTrainingActivityDetail.ConductedBy.Select(c => c.Id.ToString()));
					}
					break;
				default:
					break;
			}
		
			return PartialView("_CreateOrUpdateTrainingActivity", model);
		}


		[HttpGet]
		public ActionResult ViewDocsTrainingActivityLog(string id) {

			var model = new TrainingActivityModel();
			if (!string.IsNullOrEmpty(id)) {

				model = ExecuteQuery<FetchByIdQuery, TrainingActivityModel>(
					new FetchByIdQuery {
						Id = id
					});
				Action<UploadResultViewModel> rewriteUrls = x => {
					x.Url = Url.ActionLink<UploadController>(a => a.Get(x.Id.ToString(), false));
					x.DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(x.Id.ToString(), null));
					x.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(x.Id.ToString(), 300, 300));
				};
				model.Documents.ToList().ForEach(rewriteUrls);
				switch (model.TrainingActivityType) {
					case TrainingActivityType.Bursary:
						model.BursaryTrainingActivityDetail.Invoices.ToList().ForEach(rewriteUrls);
						break;
					case TrainingActivityType.External:
						model.ExternalTrainingActivityDetail.Invoices.ToList().ForEach(rewriteUrls);
						break;
					default:
						break;
				}

			}
		
			return PartialView("_ViewDocuments", model);
		}

		[HttpGet]
		public ActionResult FilterTrainingActivityLog(string filters, string searchText) {
			var model = ExecuteQuery<FetchAllTrainingActivityLogQuery, List<TrainingActivityModel>>(new FetchAllTrainingActivityLogQuery() { }).ToList();
			if (!string.IsNullOrEmpty(filters)) {
				
				model = model.Where(c => filters.Contains(c.TrainingActivityType.ToString())).ToList();
			}
			if (!string.IsNullOrEmpty(searchText)) {
				var search = searchText.ToLower();
				model = model.Where(c => c.Title.ToLower().Contains(search) || c.Description.ToLower().Contains(search)).ToList();
			}
			
			return PartialView("_TrainingActivityLogList",model);
		}



		[HttpPost]
		public ActionResult CreateOrUpdateTrainingActivity(TrainingActivityModel model) {

			var TrainingId = model.Id;
			if (string.IsNullOrEmpty(TrainingId)) {
				TempData["Message"] = model.Title+" " +"Training has been added successfully!";
			} else {
				TempData["Message"] = model.Title+" "+ "Training has been updated successfully!";
			}

			var users = ExecuteQuery<UserSearchQuery, IEnumerable<UserModelShort>>(new UserSearchQuery());
			var command = new CreateOrUpdateTrainingActivityCommand {
				CreatedBy = new UserModelShort { Id = SessionManager.GetCurrentlyLoggedInUserId() },
				EditedBy = new UserModelShort { Id = SessionManager.GetCurrentlyLoggedInUserId() },
				Time = DateTime.UtcNow,
				Title = model.Title,
				Description = model.Description,
				To = model.To,
				From = model.From,
				LastEditDate = DateTime.UtcNow,
				Created = DateTime.UtcNow,
				Id = model.Id,
				Venue = model.Venue,
				RewardPoints = model.RewardPoints,
				TrainingActivityType = model.TrainingActivityType,
				CostImplication = model.CostImplication,
				TrainingLabels=model.TrainingLabels,
				AdditionalInfo = model.AdditionalInfo,
				UsersTrained = users.Where(c => model.Trainees.Contains(c.Id.ToString())).ToList()
			};

			if (!string.IsNullOrEmpty(model.UploadIds)) {
				var ids = model.UploadIds.Split(',').ToList();
				foreach (var id in ids) {
					var upload= ExecuteQuery<FetchByIdQuery, Upload>(new FetchByIdQuery() { Id=id});
					var document = new UploadResultViewModel {
						DeleteType = "DELETE",
						DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(upload.Id, null)),
						Name = upload.Name,
						Description = upload.Description,
						Type = upload.ContentType,
						InProcess = false,
						Progress = "100%",
						Size = upload.Data.Length,
						Url = Url.ActionLink<UploadController>(a => a.Get(upload.Id, false)),
						ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(upload.Id, 115, 115)),
						Id = upload.Id
					};
					command.Documents.Add(document);
				}
			}

			switch (command.TrainingActivityType) {
				case TrainingActivityType.Bursary:
					command.BursaryTrainingActivityDetail.ConductedBy= users.Where(c => model.Trainers.Contains(c.Id.ToString())).ToList();
					if (!string.IsNullOrEmpty(model.BursaryInvoiceIds)) {
						var ids = model.BursaryInvoiceIds.Split(',').ToList();
						foreach (var id in ids) {
							var upload = ExecuteQuery<FetchByIdQuery, Upload>(new FetchByIdQuery() { Id = id });
							var document = new UploadResultViewModel {
								DeleteType = "DELETE",
								DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(upload.Id, null)),
								Name = upload.Name,
								Description = upload.Description,
								Type = upload.ContentType,
								InProcess = false,
								Progress = "100%",
								Size = upload.Data.Length,
								Url = Url.ActionLink<UploadController>(a => a.Get(upload.Id, false)),
								ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(upload.Id, 115, 115)),
								Id = upload.Id
							};
							command.BursaryTrainingActivityDetail.Invoices.Add(document);
						}
					}
					break;
				case TrainingActivityType.External: 
						var externalTrainingProviders = ExecuteQuery<FetchAllRecordsQuery, List<ExternalTrainingProviderListModel>>(new FetchAllRecordsQuery());
						command.ExternalTrainingActivityDetail.ConductedBy = externalTrainingProviders.Where(c => model.ExternalTrainingProviders.Contains(c.Id.ToString())).ToList();
					if (!string.IsNullOrEmpty(model.ExternalInvoiceIds)) {
						var ids = model.ExternalInvoiceIds.Split(',').ToList();
						foreach (var id in ids) {
							var upload = ExecuteQuery<FetchByIdQuery, Upload>(new FetchByIdQuery() { Id = id });
							var document = new UploadResultViewModel {
								DeleteType = "DELETE",
								DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(upload.Id, null)),
								Name = upload.Name,
								Description = upload.Description,
								Type = upload.ContentType,
								InProcess = false,
								Progress = "100%",
								Size = upload.Data.Length,
								Url = Url.ActionLink<UploadController>(a => a.Get(upload.Id, false)),
								ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(upload.Id, 115, 115)),
								Id = upload.Id
							};
							command.ExternalTrainingActivityDetail.Invoices.Add(document);
						}
					}
					break;
				case TrainingActivityType.Internal:
					command.InternalTrainingActivityDetail.ConductedBy = users.Where(c => model.Trainers.Contains(c.Id.ToString())).ToList();
					break;
				case TrainingActivityType.MentoringAndCoaching:
					command.MentoringAndCoachingTrainingActivityDetail.ConductedBy = users.Where(c => model.Trainers.Contains(c.Id.ToString())).ToList();
					break;
				case TrainingActivityType.ToolboxTalk:
					command.ToolboxTalkTrainingActivityDetail.ConductedBy = users.Where(c => model.Trainers.Contains(c.Id.ToString())).ToList();
					break;
				default:
					break;
			}

			var response = ExecuteCommand(command);
			//var trainingActivity = ExecuteQuery<FetchAllTrainingActivityLogQuery, List<TrainingActivityModel>>(new FetchAllTrainingActivityLogQuery() { }).ToList();
			//return PartialView("_TrainingActivityLogList", trainingActivity);
			return new EmptyResult();

		}

		[HttpGet]
		public ActionResult DeleteTrainingActivityLog (string id) {

			var result = ExecuteCommand(new DeleteByIdCommand<StandardUserTrainingActivityLog> { Id = id });

			var trainingActivity = ExecuteQuery<FetchAllTrainingActivityLogQuery, List<TrainingActivityModel>>(new FetchAllTrainingActivityLogQuery() { }).ToList();
			return PartialView("_TrainingActivityLogList", trainingActivity);
		}
		[HttpGet]
		public ActionResult ViewTrainees(string id) {

			var model = new TrainingActivityModel();
			if (!string.IsNullOrEmpty(id)) {

				model = ExecuteQuery<FetchByIdQuery, TrainingActivityModel>(
					new FetchByIdQuery {
						Id = id
					});

			}
			return PartialView("_Viewtrainees", model.UsersTrained);
		}
		[HttpGet]
		public ActionResult ViewTrainers(string id) {

			var model = new TrainingActivityModel();
			if (!string.IsNullOrEmpty(id)) {

				model = ExecuteQuery<FetchByIdQuery, TrainingActivityModel>(
					new FetchByIdQuery {
						Id = id
					});

			}
			return PartialView("_ViewTrainers", model);
		}

		public JsonResult GetLabelList(string name) {
			var tags = new List<string> {
				name
			};
			var labels = ExecuteQuery<LabelListQuery, IEnumerable<TrainingLabelListModel>>(new LabelListQuery{Values= tags });
			return Json(labels.Select(x => x.Name), JsonRequestBehavior.AllowGet);
		}

	}
}