using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common.Query;
using Domain.Customer;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.ViewModel;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler.Reporting {
	public class InteractionReportDetailExportQueryHandler :
		ReportingQueryHandler<InteractionReportDetailExportQuery> {
		private readonly IQueryExecutor _queryExecutor;

		public InteractionReportDetailExportQueryHandler(IQueryExecutor queryExecutor) {
			_queryExecutor = queryExecutor;
		}

		public override void BuildReport(ReportDocument document, out string title,out string recepitent, InteractionReportDetailExportQuery data) {
			title = "Interaction Report Detail";
			recepitent = string.Empty;
			var vm = _queryExecutor.Execute<InteractionReportDetailQuery, InteractionReportDetailViewModel>(data);

			var section = CreateSection(title);

			CreateHeader(vm, section);

			CreateInteractionsTable(vm, section);

			if (data.PortalContext!=null && data.PortalContext.UserCompany.EnableGlobalAccessDocuments)
				CreateGlobalInteractionsTable(vm, section);

			document.AddElement(section);
		}

		private void CreateInteractionsTable(InteractionReportDetailViewModel vm, ReportSection section) {
			var grid = CreateGrid();

			var columns = new List<Tuple<string, int>>
			{
				new Tuple<string, int>("Name", 10),
				new Tuple<string, int>("ID Number", 10),
				new Tuple<string, int>("Group", 10),
				new Tuple<string, int>("Status", 10),
				new Tuple<string, int>("Viewed Date", 10),
			};
			if (vm.DocumentType == DocumentType.Test || vm.DocumentType == DocumentType.TrainingManual) {
				columns.Add(new Tuple<string, int>("Duration", 10));
			}

			if (vm.DocumentType == DocumentType.Test) {
				columns.Add(new Tuple<string, int>("Date Completed", 10));
				columns.Add(new Tuple<string, int>("Result 1", 10));
				columns.Add(new Tuple<string, int>("Result 2", 10));
				columns.Add(new Tuple<string, int>("Result 3", 10));
			}

			if (vm.DocumentType == DocumentType.Checklist) {
				columns.Add(new Tuple<string, int>("Checks Completed", 10));
				columns.Add(new Tuple<string, int>("Date Submitted", 10));
			}

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Interactions" });
			CreateTableHeader(grid, columns.ToArray());

			foreach (var interaction in vm.Interactions) {
				var data = new List<object>
				{
					interaction.Name,
					interaction.IDNumber,
					interaction.Group,
					interaction.Status
				};
				if (interaction.ViewDate.ToString() == "1/1/0001 12:00:00 AM") {
					data.Add("");
				} else {
					data.Add(interaction.ViewDate);
				}
				if (vm.DocumentType == DocumentType.TrainingManual || vm.DocumentType == DocumentType.Test) {
					data.Add(interaction.Duration);
				}
				
				if (vm.DocumentType == DocumentType.Test) {
					if(interaction.DateCompleted.ToString() == "1/1/0001 12:00:00 AM") {
						data.Add("");
					} else {
						data.Add(interaction.DateCompleted);
					}
					data.Add(interaction.Result1);
					data.Add(interaction.Result2);
					data.Add(interaction.Result3);
				}
				if (vm.DocumentType == DocumentType.Checklist) {
					data.Add(interaction.ChecksCompleted);
					if (interaction.DateSubmitted.ToString() == "1/1/0001 12:00:00 AM") {
						data.Add("");
					} else {
						data.Add(interaction.DateSubmitted);
					}
				}
				CreateTableDataRow(grid, data.ToArray());
			}

			section.AddElement(grid);
		}
		private void CreateGlobalInteractionsTable(InteractionReportDetailViewModel vm, ReportSection section) {
			var grid = CreateGrid();

			var columns = new List<Tuple<string, int>>
			{
				new Tuple<string, int>("Name", 10),
				new Tuple<string, int>("ID Number", 10),
				new Tuple<string, int>("Group", 10),
				new Tuple<string, int>("Status", 10),
				new Tuple<string, int>("Viewed Date", 10),
			};
			if (vm.DocumentType == DocumentType.Test || vm.DocumentType == DocumentType.TrainingManual) {
				columns.Add(new Tuple<string, int>("Duration", 10));
			}

			if (vm.DocumentType == DocumentType.Test) {
				columns.Add(new Tuple<string, int>("Date Completed", 10));
				columns.Add(new Tuple<string, int>("Result 1", 10));
				columns.Add(new Tuple<string, int>("Result 2", 10));
				columns.Add(new Tuple<string, int>("Result 3", 10));
			}

			if (vm.DocumentType == DocumentType.Checklist) {
				columns.Add(new Tuple<string, int>("Checks Completed", 10));
				columns.Add(new Tuple<string, int>("Date Submitted", 10));
			}

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Global Interactions" });
			CreateTableHeader(grid, columns.ToArray());

			foreach (var interaction in vm.GlobalInteractions) {
				var data = new List<object>
				{
					interaction.Name,
					interaction.IDNumber,
					interaction.Group,
					interaction.Status
				};
				if (interaction.ViewDate.ToString() == "1/1/0001 12:00:00 AM") {
					data.Add("");
				} else {
					data.Add(interaction.ViewDate);
				}
				if (vm.DocumentType == DocumentType.TrainingManual || vm.DocumentType == DocumentType.Test) {
					data.Add(interaction.Duration);
				}
				if (vm.DocumentType == DocumentType.Test) {
					if (interaction.DateCompleted.ToString() == "1/1/0001 12:00:00 AM") {
						data.Add("");
					} else {
						data.Add(interaction.DateCompleted);
					}
					data.Add(interaction.Result1);
					data.Add(interaction.Result2);
					data.Add(interaction.Result3);
				}
				if (vm.DocumentType == DocumentType.Checklist) {
					data.Add(interaction.ChecksCompleted);
					if (interaction.DateSubmitted.ToString() == "1/1/0001 12:00:00 AM") {
						data.Add("");
					} else {
						data.Add(interaction.DateSubmitted);
					}
				}
				CreateTableDataRow(grid, data.ToArray());
			}

			section.AddElement(grid);
		}

		private void CreateHeader(InteractionReportDetailViewModel vm, ReportSection section) {
			var grid = CreateGrid();

			var genDateRow = new GridRowBlock();
			genDateRow.AddElement(new GridCellBlock("Generated Date",
				new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
			genDateRow.AddElement(new GridCellBlock(vm.GeneratedDate.ToString("d MMM yyyy")));
			genDateRow.AddElement(new GridCellBlock());

			grid.AddElement(genDateRow);

			var dateRow = new GridRowBlock();
			dateRow.AddElement(new GridCellBlock("Date Range",
				new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
			dateRow.AddElement(new GridCellBlock($"From: {vm.FromDate.ToString("d MMM yyyy")}"));
			dateRow.AddElement(new GridCellBlock($"To: {vm.ToDate.ToString("d MMM yyyy")}"));

			grid.AddElement(dateRow);

			var typeRow = new GridRowBlock();
			typeRow.AddElement(new GridCellBlock("Document Type",
				new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
			typeRow.AddElement(new GridCellBlock(VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(vm.DocumentType)));
			typeRow.AddElement(new GridCellBlock());

			grid.AddElement(typeRow);

			var titleRow = new GridRowBlock();
			titleRow.AddElement(new GridCellBlock("Title",
				new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
			titleRow.AddElement(new GridCellBlock(vm.DocumentTitle));
			titleRow.AddElement(new GridCellBlock());

			grid.AddElement(titleRow);

			if (vm.Groups.Any()) {
				var groupRow = new GridRowBlock();
				groupRow.AddElement(new GridCellBlock("Groups",
					new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
				groupRow.AddElement(new GridCellBlock(string.Join(", ", vm.Groups)));
				groupRow.AddElement(new GridCellBlock());

				grid.AddElement(groupRow);
			}

			if (vm.DocumentType == DocumentType.Test) {
				var passReqRow = new GridRowBlock();
				passReqRow.AddElement(new GridCellBlock("Pass Requirement",
					new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
				passReqRow.AddElement(new GridCellBlock(vm.PassRequirement));
				passReqRow.AddElement(new GridCellBlock());

				grid.AddElement(passReqRow);
			}

			section.AddElement(grid);
		}
	}
}