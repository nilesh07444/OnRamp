using Common.Query;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using VirtuaCon.Reporting;

namespace Ramp.Services.QueryHandler.Reporting {
	class TrainingActivityLogExportQueryHandler :
		ReportingQueryHandler<FilterTrainingActivityLogQuery > {
		private readonly IQueryExecutor _queryExecutor;
		
		public TrainingActivityLogExportQueryHandler(
			IQueryExecutor queryExecutor) {
			_queryExecutor = queryExecutor;
		}

		public override void BuildReport(ReportDocument document, out string title, out string recepitent, FilterTrainingActivityLogQuery data) {
			title = "TrainingLog Report";
			recepitent = data.Recepients;
			var section = CreateSection(title);
			var vm =
				_queryExecutor
					.Execute<FilterTrainingActivityLogQuery, List<TrainingActivityModel>>(data);
			CreateTrainingLogTable(vm, section);
			document.AddElement(section);
		}

		private void CreateTrainingLogTable(List<TrainingActivityModel> vm, ReportSection section) {
			var grid = CreateGrid();
			var columns = new[]
			{
				new Tuple<string, int>("Title", 30),
				new Tuple<string, int>("Description", 40),
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Cost Implication", 20),
				new Tuple<string, int>("Reward Points", 20),
				new Tuple<string, int>("From", 20),
				new Tuple<string, int>("To", 20)
			};

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Training Log" });
			CreateTableHeader(grid, columns);
			
			foreach (var trainingLogModel in vm) {
				CreateTableDataRow(grid,
					trainingLogModel.Title,
					trainingLogModel.Description,
					trainingLogModel.TrainingActivityType.ToString(),
					trainingLogModel.CostImplication,
					trainingLogModel.RewardPoints,
					trainingLogModel.From.Value.ToLocalTime(),
					trainingLogModel.To.Value.ToLocalTime());
			}

			section.AddElement(grid);
		}

	}
}