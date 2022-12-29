using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common.Query;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.ViewModel;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler.Reporting
{
    public class UserActivityAndPerformanceReportExportQueryHandler :
        ReportingQueryHandler<UserActivityAndPerformanceReportExportQuery>
    {
        private readonly IQueryExecutor _queryExecutor;
		private  bool EnableCheckListDocument;
        public UserActivityAndPerformanceReportExportQueryHandler(
            IQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        public override void BuildReport(ReportDocument document, out string title,out string recepitent, UserActivityAndPerformanceReportExportQuery data)
        {

			data.ToggleFilter = "email, mobile";

			title = "User Activity & Performance Report";
			recepitent = string.Empty;
            var section = CreateSection(title);
			EnableCheckListDocument = data.PortalContext.UserCompany.EnableChecklistDocument;

			var vm =
                _queryExecutor
                    .Execute<UserActivityAndPerformanceReportQuery, UserActivityAndPerformanceViewModel>(data);

            CreatePersonalDetails(vm, section);

            CreateDocumentInteractionTables(vm, section);
			if (data.PortalContext.UserCompany.EnableGlobalAccessDocuments)
				CreateGlobalDocumentInteractionTables(vm, section);

            if (vm.EnableTrainingActivities)
                CreateTrainActivitiesTable(vm, section);

            CreatePointsTable(vm, section);

            CreateFeedbackTable(vm, section);

            document.AddElement(section);
        }

        private void CreateFeedbackTable(UserActivityAndPerformanceViewModel vm, ReportSection section)
        {
			if (vm.Feedback.Count() > 0) {
				var grid = CreateGrid();
				var columns = new[]
				{
				new Tuple<string, int>("Document Type", 30),
				new Tuple<string, int>("Title", 40)
			};

				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Feedback" });
				CreateTableHeader(grid, columns);
				if (!EnableCheckListDocument) {
					vm.Feedback = vm.Feedback.Where(c => c.DocumentType != "Checklist");
				}

				foreach (var feedbackModel in vm.Feedback) {
					CreateTableDataRow(grid,
						feedbackModel.DocumentType,
						feedbackModel.DocumentTitle,
						feedbackModel.Date.ToLocalTime().ToString("G"),
						feedbackModel.Type,
						feedbackModel.Comment);
				}

				section.AddElement(grid);
			}
        }

        private void CreatePointsTable(UserActivityAndPerformanceViewModel vm, ReportSection section)
        {
			if (vm.PointsStatement.Count > 0) {
				var grid = CreateGrid();
				var columns = new[]
				{
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Date", 30),
				new Tuple<string, int>("Access", 30),
				new Tuple<string, int>("Points Awarded", 30)
			};

				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Points and Awards" });
				CreateTableHeader(grid, columns);
				if (!EnableCheckListDocument) {
					vm.PointsStatement = vm.PointsStatement.Where(c => c.Type != "Checklist").ToList();
				}

				foreach (var pointModel in vm.PointsStatement) {
					CreateTableDataRow(grid,
						pointModel.Type,
						pointModel.Title,
						pointModel.Date.ToLocalTime().ToString("G"),
						pointModel.IsGlobalAccess,
						pointModel.Points.ToString());
				}

				section.AddElement(grid);
			}
        }

        private void CreateTrainActivitiesTable(UserActivityAndPerformanceViewModel vm, ReportSection section)
        {
			if (vm.TrainingActivities.Count() > 0) {
				var grid = CreateGrid();
				var columns = new[]
				{
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Title", 30),
				new Tuple<string, int>("Period", 40),
				new Tuple<string, int>("Cost", 20)
			};

				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Other Training Activities" });
				CreateTableHeader(grid, columns);

				foreach (var model in vm.TrainingActivities) {
					CreateTableDataRow(grid,
						model.Type,
						model.Title,
						$"{(model.FromDate.HasValue ? model.FromDate.Value.ToLocalTime().ToString("G") : string.Empty)} - {(model.ToDate.HasValue ? model.ToDate.Value.ToLocalTime().ToString("G") : string.Empty)}",
						model.Cost);
				}

				section.AddElement(grid);
			}
        }

        private void CreateDocumentInteractionTables(UserActivityAndPerformanceViewModel vm, ReportSection section)
        {
            var grid = CreateGrid();

            CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Document Interaction" });

            section.AddElement(grid);
			if (vm.MemoInteractions.Count > 0) {
				var mGrid = CreateGrid();
				var mColumns = new[]
				{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Assigned", 30),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Time Taken", 20)
			};

				CreateTableDataRowWithStyles(mGrid, HeaderStyle, new[] { "Memo" });
				CreateTableHeader(mGrid, mColumns);

				foreach (var model in vm.MemoInteractions) {
					CreateTableDataRow(mGrid,
						model.DocumentTitle,
						model.Viewed ? "Yes" : "No",
						model.DateAssigned.ToLocalTime().ToString("G"),
						model.DateViewed.HasValue ? model.DateViewed.Value.ToLocalTime().ToString("G") : "Not Available",
						model.TimeTaken == null ? "Not Available" : model.TimeTaken);
				}

				section.AddElement(mGrid);
			}

			if (vm.TrainingManualInteractions.Count > 0) {
				var tmGrid = CreateGrid();
				var tmColumns = new[]
				{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Assigned", 30),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Time Taken", 20)
			};

				CreateTableDataRowWithStyles(tmGrid, HeaderStyle, new[] { "Training Manual" });
				CreateTableHeader(tmGrid, tmColumns);

				foreach (var model in vm.TrainingManualInteractions) {
					CreateTableDataRow(tmGrid,
						model.DocumentTitle,
						model.Viewed ? "Yes" : "No",
						model.DateAssigned.ToLocalTime().ToString("G"),
						model.DateViewed.HasValue ? model.DateViewed.Value.ToLocalTime().ToString("G") : "Not Available",
						model.TimeTaken == null ? "Not Available" : model.TimeTaken);
				}

				section.AddElement(tmGrid);
			}

			if (vm.PolicyInteractions.Count > 0) {
				var pGrid = CreateGrid();
				var pColumns = new[]
				{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Assigned", 30),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Time Taken", 20),
				new Tuple<string, int>("Sign Off Status", 20),
				new Tuple<string, int>("Sign Off Date", 30)
			};

				CreateTableDataRowWithStyles(pGrid, HeaderStyle, new[] { "Policy" });
				CreateTableHeader(pGrid, pColumns);

				foreach (var model in vm.PolicyInteractions) {
					CreateTableDataRow(pGrid,
						model.DocumentTitle,
						model.Viewed ? "Yes" : "No",
						model.DateAssigned.ToLocalTime().ToString("G"),
						model.DateViewed.HasValue ? model.DateViewed.Value.ToLocalTime().ToString("G") : "Not Available",
						model.TimeTaken == null ? "Not Available" : model.TimeTaken,
						model.Response == "" ? "Not Available" : model.Response,
						model.ResponseDate.HasValue ? model.ResponseDate.Value.ToLocalTime().ToString("G") : "Not Available");
				}

				section.AddElement(pGrid);
			}
			#region code commented after discussion on Task15 Display more than 3 results 
			//var tGrid = CreateGrid();
			//var tColumns = new[]
			//{
			//    new Tuple<string, int>("Title", 40),
			//    new Tuple<string, int>("Viewed", 20),
			//    new Tuple<string, int>("Date Assigned", 30),
			//    new Tuple<string, int>("Result 1", 20),
			//    new Tuple<string, int>("Date Viewed", 30),
			//    new Tuple<string, int>("Time Taken", 0),
			//    new Tuple<string, int>("Result 2", 20),
			//    new Tuple<string, int>("Date Viewed", 30),
			//    new Tuple<string, int>("Time Taken", 0),
			//    new Tuple<string, int>("Result 3", 20),
			//    new Tuple<string, int>("Date Viewed", 30),
			//    new Tuple<string, int>("Time Taken", 0)
			//};

			//CreateTableDataRowWithStyles(tGrid, HeaderStyle, new[] { "Test" });
			//CreateTableHeader(tGrid, tColumns);

			//foreach (var model in vm.TestInteractions)
			//{
			//    CreateTableDataRow(tGrid,
			//        model.DocumentTitle,
			//        model.Viewed ? "Yes" : "No",
			//        model.DateAssigned.ToLocalTime().ToString("G"),
			//        model.Result1,
			//        model.DateViewed1.HasValue ? model.DateViewed1.Value.ToLocalTime().ToString("G") : string.Empty,
			//        model.TimeTaken1,
			//        model.Result2,
			//        model.DateViewed2.HasValue ? model.DateViewed2.Value.ToLocalTime().ToString("G") : string.Empty,
			//        model.TimeTaken2,
			//        model.Result3,
			//        model.DateViewed3.HasValue ? model.DateViewed3.Value.ToLocalTime().ToString("G") : string.Empty,
			//        model.TimeTaken3);
			//}

			//section.AddElement(tGrid);
			#endregion

			if(vm.TestResultList.Count > 0) { 
			var tests = vm.TestResultList.GroupBy(c=>c.TestId).Select(f=>f.FirstOrDefault()).ToList();

				foreach (var item in tests) {
					var testGrid = CreateGrid();
					var testColumns = new[]
					{
				new Tuple<string, int>("Attempt", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Assigned", 30),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Result", 30),
				new Tuple<string, int>("Final Result",30)

			};

					CreateTableDataRowWithStyles(testGrid, HeaderStyle, new[] { "Test: " + item.Title });
					CreateTableHeader(testGrid, testColumns);
					var results = vm.TestResultList.Where(c => c.TestId == item.TestId).ToList();
					foreach (var model in results) {
						CreateTableDataRow(testGrid,
							model.Attempt,
							model.Viewed ? "Yes" : "No",
							model.DateAssigned.ToLocalTime().ToString("G"),
							model.DateViewed == null ? "Not Available" : model.DateViewed.ToLocalTime().ToString("G"),
							model.Result,
							model.FinalResult

							);
					}
					section.AddElement(testGrid);
				}
			}

			if (EnableCheckListDocument) {
				if (vm.CheckLisInteractions.Count > 0) {
					var checkListGrid = CreateGrid();
					var checkListColumns = new[]
					{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Assigned", 30),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Complted", 30),
				new Tuple<string, int>("Checks Completed",30)

			};

					CreateTableDataRowWithStyles(checkListGrid, HeaderStyle, new[] { "CheckList: " });
					CreateTableHeader(checkListGrid, checkListColumns);
					foreach (var model in vm.CheckLisInteractions) {
						CreateTableDataRow(checkListGrid,
							model.DocumentTitle,
							model.Viewed ? "Yes" : "No",
							model.DateAssigned.ToLocalTime().ToString("G"),
							model.DateViewed == null ? "Not Available" : model.DateViewed.Value.ToLocalTime().ToString("G"),
							model.Completed,
							model.ChecksCompleted

							);
					}
					section.AddElement(checkListGrid);
				}
			}


		}
		#region Global Access
		private void CreateGlobalDocumentInteractionTables(UserActivityAndPerformanceViewModel vm, ReportSection section) {
			var grid = CreateGrid();

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Global Document Interaction" });

			section.AddElement(grid);
			if (vm.MemoGlobalInteractions.Count > 0) {
				var mGrid = CreateGrid();
			var mColumns = new[]
			{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Time Taken", 20)
			};

			CreateTableDataRowWithStyles(mGrid, HeaderStyle, new[] { "Memo" });
			CreateTableHeader(mGrid, mColumns);

				foreach (var model in vm.MemoGlobalInteractions) {
					CreateTableDataRow(mGrid,
						model.DocumentTitle,
						model.Viewed ? "Yes" : "No",
						model.DateViewed.HasValue ? model.DateViewed.Value.ToLocalTime().ToString("G") : "Not Available",
						model.TimeTaken == null ? "Not Available" : model.TimeTaken);
				}

				section.AddElement(mGrid);
			}

			if (vm.TrainingManualGlobalInteractions.Count > 0) {
				var tmGrid = CreateGrid();
				var tmColumns = new[]
				{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Time Taken", 20)
			};

				CreateTableDataRowWithStyles(tmGrid, HeaderStyle, new[] { "Training Manual" });
				CreateTableHeader(tmGrid, tmColumns);

				foreach (var model in vm.TrainingManualGlobalInteractions) {
					CreateTableDataRow(tmGrid,
						model.DocumentTitle,
						model.Viewed ? "Yes" : "No",
						model.DateViewed.HasValue ? model.DateViewed.Value.ToLocalTime().ToString("G") : "Not Available",
						model.TimeTaken == null ? "Not Available" : model.TimeTaken);
				}

				section.AddElement(tmGrid);
			}

			if (vm.PolicyGlobalInteractions.Count > 0) {
				var pGrid = CreateGrid();
				var pColumns = new[]
				{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Time Taken", 20),
				new Tuple<string, int>("Sign Off Status", 20),
				new Tuple<string, int>("Sign Off Date", 30)
			};

				CreateTableDataRowWithStyles(pGrid, HeaderStyle, new[] { "Policy" });
				CreateTableHeader(pGrid, pColumns);

				foreach (var model in vm.PolicyGlobalInteractions) {
					CreateTableDataRow(pGrid,
						model.DocumentTitle,
						model.Viewed ? "Yes" : "No",
						model.DateViewed.HasValue ? model.DateViewed.Value.ToLocalTime().ToString("G") : "Not Available",
						model.TimeTaken == null ? "Not Available" : model.TimeTaken,
						model.Response == "" ? "Not Available" : model.Response,
						model.ResponseDate.HasValue ? model.ResponseDate.Value.ToLocalTime().ToString("G") : "Not Available");
				}

				section.AddElement(pGrid);
			}
			#region code commented after discussion on Task15 Display more than 3 results 
			//var tGrid = CreateGrid();
			//var tColumns = new[]
			//{
			//    new Tuple<string, int>("Title", 40),
			//    new Tuple<string, int>("Viewed", 20),
			//    new Tuple<string, int>("Date Assigned", 30),
			//    new Tuple<string, int>("Result 1", 20),
			//    new Tuple<string, int>("Date Viewed", 30),
			//    new Tuple<string, int>("Time Taken", 0),
			//    new Tuple<string, int>("Result 2", 20),
			//    new Tuple<string, int>("Date Viewed", 30),
			//    new Tuple<string, int>("Time Taken", 0),
			//    new Tuple<string, int>("Result 3", 20),
			//    new Tuple<string, int>("Date Viewed", 30),
			//    new Tuple<string, int>("Time Taken", 0)
			//};

			//CreateTableDataRowWithStyles(tGrid, HeaderStyle, new[] { "Test" });
			//CreateTableHeader(tGrid, tColumns);

			//foreach (var model in vm.TestInteractions)
			//{
			//    CreateTableDataRow(tGrid,
			//        model.DocumentTitle,
			//        model.Viewed ? "Yes" : "No",
			//        model.DateAssigned.ToLocalTime().ToString("G"),
			//        model.Result1,
			//        model.DateViewed1.HasValue ? model.DateViewed1.Value.ToLocalTime().ToString("G") : string.Empty,
			//        model.TimeTaken1,
			//        model.Result2,
			//        model.DateViewed2.HasValue ? model.DateViewed2.Value.ToLocalTime().ToString("G") : string.Empty,
			//        model.TimeTaken2,
			//        model.Result3,
			//        model.DateViewed3.HasValue ? model.DateViewed3.Value.ToLocalTime().ToString("G") : string.Empty,
			//        model.TimeTaken3);
			//}

			//section.AddElement(tGrid);
			#endregion

			if(vm.TestResultGlobalList.Count > 0) { 
			var tests = vm.TestResultGlobalList.GroupBy(c => c.TestId).Select(f => f.FirstOrDefault()).ToList();

				foreach (var item in tests) {
					var testGrid = CreateGrid();
					var testColumns = new[]
					{
				new Tuple<string, int>("Attempt", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Result", 30),
				new Tuple<string, int>("Final Result",30)

			};

					CreateTableDataRowWithStyles(testGrid, HeaderStyle, new[] { "Test: " + item.Title });
					CreateTableHeader(testGrid, testColumns);
					var results = vm.TestResultGlobalList.Where(c => c.TestId == item.TestId).ToList();
					foreach (var model in results) {
						CreateTableDataRow(testGrid,
							model.Attempt,
							model.Viewed ? "Yes" : "No",
							model.DateViewed == null ? "Not Available" : model.DateViewed.ToLocalTime().ToString("G"),
							model.Result,
							model.FinalResult

							);
					}
					section.AddElement(testGrid);
				}
			}

			if (EnableCheckListDocument) {
				if (vm.CheckListGlobalInteractions.Count > 0) {
					var checkListGrid = CreateGrid();
					var checkListColumns = new[]
					{
				new Tuple<string, int>("Title", 40),
				new Tuple<string, int>("Viewed", 20),
				new Tuple<string, int>("Date Viewed", 30),
				new Tuple<string, int>("Complted", 30),
				new Tuple<string, int>("Checks Completed",30)

			};

					CreateTableDataRowWithStyles(checkListGrid, HeaderStyle, new[] { "CheckList: " });
					CreateTableHeader(checkListGrid, checkListColumns);
					foreach (var model in vm.CheckListGlobalInteractions) {
						CreateTableDataRow(checkListGrid,
							model.DocumentTitle,
							model.Viewed ? "Yes" : "No",
							model.DateViewed == null ? "Not Available" : model.DateViewed.Value.ToLocalTime().ToString("G"),
							model.Completed,
							model.ChecksCompleted

							);
					}
					section.AddElement(checkListGrid);
				}
			}


		}
		#endregion
		private void CreatePersonalDetails(UserActivityAndPerformanceViewModel vm, ReportSection section)
        {
            var grid = CreateGrid();
            CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Personal Details" });

            var fields = new List<Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>>();
            fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                "Name", model => model.Name));
            fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                "Mobile Number", model => model.ContactNumber));
            fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                "Email", model => model.Email));
            fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                "ID Number", model => model.IDNumber));
            fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                "Gender", model => model.Gender));
            fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                "Employee Number", model => model.EmployeeNumber));

            if (vm.EnableRaceCode)
            {
                fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                    "Race", model => model.Race));
            }

            fields.Add(new Tuple<string, Func<UserActivityAndPerformanceViewModel.UserViewModel, string>>(
                "Group", model => model.Group));

            foreach (var field in fields)
            {
                var row = new GridRowBlock();
                row.AddElement(new GridCellBlock(field.Item1,
                    new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
                row.AddElement(new GridCellBlock(field.Item2(vm.UserModel)));

                grid.AddElement(row);
            }

            section.AddElement(grid);
        }
    }
}