
using Common.Query;
using Common.Report;
using Domain.Customer.Models;
using Domain.Customer.Models.ScheduleReport;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.Query.Report;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Controllers;

namespace Web.UI.Areas.Reporting.Controllers {
	public class TrainingActivityLogReportController :  ExportController<FilterTrainingActivityLogQuery> {
		// GET: Reporting/TrainingActivityLogReport
		public ActionResult Index() {
			var model = new TrainingActivityModel();
			
			var labels = ExecuteQuery<LabelListQuery, IEnumerable<TrainingLabelListModel>>(new LabelListQuery());
			

			model.TrainingLabelList = new SelectList(labels, "Id", "Name");

			var enumList = Enum.GetValues(typeof(TrainingActivityType)).OfType<TrainingActivityType>().ToList();
			IDictionary<int, string> dict = new Dictionary<int, string> {
				{ 10,"None Selected"},
				{ (int)TrainingActivityType.Bursary, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Bursary) },
				{ (int)TrainingActivityType.External, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.External) },
				{ (int)TrainingActivityType.Internal, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.Internal) },
				{ (int)TrainingActivityType.MentoringAndCoaching, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.MentoringAndCoaching) },
				{ (int)TrainingActivityType.ToolboxTalk, VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(TrainingActivityType.ToolboxTalk) }
			};
			model.TrainingActivityTypeList = new SelectList(dict, "Key", "Value");
			var users = ExecuteQuery<UserSearchQuery, IEnumerable<UserModelShort>>(new UserSearchQuery());
			model.TrainersList = new SelectList(users, "Id", "UserName");
			model.TraineesList = new SelectList(users, "Id", "UserName");
			var externalTrainingProviders = ExecuteQuery<FetchAllRecordsQuery, List<ExternalTrainingProviderListModel>>(new FetchAllRecordsQuery());
			model.ExternalTrainingProviderList = new SelectList(externalTrainingProviders, "Id", "CompanyName");
			if (model.UsersTrained.Any()) {
				model.Trainees = string.Join(",", model.UsersTrained.Select(c => c.Id).ToList());
			}


			return View(model);
		}
		/// <summary>
		/// this is used to filter the report
		/// </summary>
		/// <param name="fromDate"></param>
		/// <param name="toDate"></param>
		/// <param name="trainers"></param>
		/// <param name="trainees"></param>
		/// <param name="trainingLables"></param>
		/// <param name="trainingType"></param>
		/// <param name="costRangeFrom"></param>
		/// <param name="costRangeTo"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GetTrainingLogFilter(DateTime fromDate, DateTime toDate, string trainers, string trainees, string trainingLables, string externalTrainingProviders, int trainingType=0, decimal costRangeFrom=0, decimal costRangeTo=0) {

			var model = ExecuteQuery<FilterTrainingActivityLogQuery, List<TrainingActivityModel>>(new FilterTrainingActivityLogQuery() {
				FromDate=fromDate,
				ToDate=toDate,
				Trainees=trainees,
				Trainers=trainers,
				TrainingLables=trainingLables,
				ExternalTrainingProviders=externalTrainingProviders,
				TrainingType= trainingType,
				CostRangeFrom= costRangeFrom,
				CostRangeTo= costRangeTo

			}).ToList();

			return PartialView("_TrainingActivityLogReportList", model);
		}

		[System.Web.Mvc.HttpGet]
		[System.Web.Mvc.AllowAnonymous]
		public void DownloadExcelLog(string Occurance)
		{
			var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "11").ToList();
			foreach (var d in getAllReport)
			{
				var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
				{
					Id = d.Id
				});
				 
				FilterTrainingActivityLogQuery query = new FilterTrainingActivityLogQuery
				{
					FromDate = ReportParam.FromDate,
					ToDate = ReportParam.ToDate,
					Trainees = ReportParam.ReturnSingleParams(data.Params, "Users"),
                    Trainers = ReportParam.ReturnSingleParams(data.Params, "Users"),					
					Recepients = data.RecipientsList,

				};		 
				DownloadExcelLog(query);
			}
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult DownloadExcelLog(FilterTrainingActivityLogQuery query) {
			query.PortalContext = PortalContext.Current;
			return Publish(ExecuteQuery<FilterTrainingActivityLogQuery, IExportModel>(query), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
		}


		public override ActionResult Zip(FilterTrainingActivityLogQuery query) {
			throw new NotImplementedException();
		}
	}
}