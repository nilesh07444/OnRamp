using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.ViewModel;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler.Reporting {
	public class InteractionReportExportQueryHandler :
		ReportingQueryHandler<InteractionReportExportQuery> {
		private readonly IQueryExecutor _queryExecutor;
		private readonly IRepository<CustomerGroup> _groupRepository;
		private readonly IRepository<DocumentCategory> _categoryRepository;
		private bool EnableGlobalAccessDocuments { get; set; } = false;
		public InteractionReportExportQueryHandler(
			IQueryExecutor queryExecutor,
			IRepository<CustomerGroup> groupRepository,
			IRepository<DocumentCategory> categoryRepository) {
			_queryExecutor = queryExecutor;
			_groupRepository = groupRepository;
			_categoryRepository = categoryRepository;
		}

		public override void BuildReport(ReportDocument document, out string title,out string recepitent, InteractionReportExportQuery data) {
			title = "Interaction Report";
			recepitent = string.Empty;
			EnableGlobalAccessDocuments = data.PortalContext.UserCompany.EnableGlobalAccessDocuments;
			var vm = _queryExecutor.Execute<InteractionReportQuery, InteractionReportViewModel>(data);

			var section = CreateSection(title);

			CreateFilterDetails(data, section);

			if (vm.TrainingManualInteractions.Any()) {
				
					CreateTrainingManualMemoTable(vm.TrainingManualInteractions, section, "Training Manual");
				
			}

			if (vm.MemoInteractions.Any()) {
				
					CreateTrainingManualMemoTable(vm.MemoInteractions, section, "Memo");
				
			}
				

			if (vm.PolicyInteractions.Any()) {
				
				  CreatePolicyTable(vm.PolicyInteractions, section,"Policy");
				
			}

			if (vm.TestInteractions.Any()) {
				
				CreateTestTable(vm.TestInteractions, section,"Test");
				
			}

			if (vm.CheckListInteractions.Any()) {
				
					CreateTrainingManualMemoTable(vm.CheckListInteractions, section, "Checklist");
				
			}

			#region for Global docs
			if (vm.GlobalTrainingManualInteractions.Any()) {
				
					CreateGlobalTrainingManualMemoTable(vm.GlobalTrainingManualInteractions, section, "Global Training Manual");
			}

			if (vm.GlobalMemoInteractions.Any()) {
				
					CreateGlobalTrainingManualMemoTable(vm.GlobalMemoInteractions, section, "Global Memo");
			}


			if (vm.GlobalPolicyInteractions.Any()) {
				
					CreateGlobalPolicyTable(vm.GlobalPolicyInteractions, section,"Global Policy");
			}

			if (vm.GlobalTestInteractions.Any()) {
				
					CreateGlobalTestTable(vm.GlobalTestInteractions, section,"Global Test");
			}

			if (vm.GlobalCheckListInteractions.Any()) {
				
					CreateGlobalTrainingManualMemoTable(vm.GlobalCheckListInteractions, section, "Global Checklist");
			}

			#endregion


			document.AddElement(section);
		}

		private void CreateTestTable(IList<InteractionReportViewModel.TestInteractionModel> interactions, ReportSection section, string header) {
			var grid = CreateGrid();

			var columns = new[]
			{
				new Tuple<string, int>("Title", 10),
				new Tuple<string, int>("Total Allocated", 10),
				new Tuple<string, int>("Yet to Interact", 10),
				new Tuple<string, int>("Passed", 10),
				new Tuple<string, int>("Failed", 10)

			};

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { header});
			CreateTableHeader(grid, columns);

			foreach (var interaction in interactions) {
				CreateTableDataRow(grid,
					interaction.Title,
					interaction.Allocated,
					interaction.YetToInteract,
					interaction.Passed,
					interaction.Failed);
			}

			section.AddElement(grid);
		}

		private void CreatePolicyTable(IList<InteractionReportViewModel.PolicyInteractionModel> interactions, ReportSection section, string header) {
			var grid = CreateGrid();

			var columns = new[]
			{
				new Tuple<string, int>("Title", 10),
				new Tuple<string, int>("Total Allocated", 10),
				new Tuple<string, int>("Yet to Interact", 10),
				new Tuple<string, int>("View Later", 10),
				new Tuple<string, int>("Not Accepted", 10),
				new Tuple<string, int>("Accepted", 10)

			};

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { header });
			CreateTableHeader(grid, columns);

			foreach (var interaction in interactions) {
				CreateTableDataRow(grid,
					interaction.Title,
					interaction.Allocated,
					interaction.YetToInteract,
					interaction.ViewLater,
					interaction.Rejected,
					interaction.Accepted);
			}

			section.AddElement(grid);
		}

		private void CreateTrainingManualMemoTable(IList<InteractionReportViewModel.InteractionModel> interactions, ReportSection section, string header) {
			var grid = CreateGrid();

			var columns = new[]
			{
				new Tuple<string, int>("Title", 10),
				new Tuple<string, int>("Total Allocated", 10),
				new Tuple<string, int>("Interacted", 10),
				new Tuple<string, int>("Yet to Interact", 10),

			};

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { header });
			CreateTableHeader(grid, columns);

			foreach (var interaction in interactions) {
				CreateTableDataRow(grid,
					interaction.Title,
					interaction.Allocated,
					interaction.Interacted,
					interaction.YetToInteract);
			}

			section.AddElement(grid);
		}

		#region Global Access
		private void CreateGlobalTestTable(IList<InteractionReportViewModel.TestInteractionModel> interactions, ReportSection section, string header) {
			var grid = CreateGrid();

			var columns = new[]
			{
				new Tuple<string, int>("Title", 10),
				new Tuple<string, int>("Yet to Interact", 10),
				new Tuple<string, int>("Passed", 10),
				new Tuple<string, int>("Failed", 10)

			};

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { header });
			CreateTableHeader(grid, columns);

			foreach (var interaction in interactions) {
				CreateTableDataRow(grid,
					interaction.Title,
					interaction.YetToInteract,
					interaction.Passed,
					interaction.Failed);
			}

			section.AddElement(grid);
		}

		private void CreateGlobalPolicyTable(IList<InteractionReportViewModel.PolicyInteractionModel> interactions, ReportSection section, string header) {
			var grid = CreateGrid();

			var columns = new[]
			{
				new Tuple<string, int>("Title", 10),
				new Tuple<string, int>("Yet to Interact", 10),
				new Tuple<string, int>("View Later", 10),
				new Tuple<string, int>("Not Accepted", 10),
				new Tuple<string, int>("Accepted", 10)

			};

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { header });
			CreateTableHeader(grid, columns);

			foreach (var interaction in interactions) {
				CreateTableDataRow(grid,
					interaction.Title,
					interaction.YetToInteract,
					interaction.ViewLater,
					interaction.Rejected,
					interaction.Accepted);
			}

			section.AddElement(grid);
		}

		private void CreateGlobalTrainingManualMemoTable(IList<InteractionReportViewModel.InteractionModel> interactions, ReportSection section, string header) {
			var grid = CreateGrid();

			var columns = new[]
			{
				new Tuple<string, int>("Title", 10),
				new Tuple<string, int>("Interacted", 10),
				new Tuple<string, int>("Yet to Interact", 10),

			};

			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { header });
			CreateTableHeader(grid, columns);

			foreach (var interaction in interactions) {
				CreateTableDataRow(grid,
					interaction.Title,
					interaction.Interacted,
					interaction.YetToInteract
					);
			}

			section.AddElement(grid);
		}
		#endregion


		private void CreateFilterDetails(InteractionReportExportQuery data, ReportSection section) {
			var grid = CreateGrid();

			var dateRow = new GridRowBlock();
			dateRow.AddElement(new GridCellBlock("Date Range",
				new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
			dateRow.AddElement(new GridCellBlock($"From: {data.FromDate.ToString("d MMM yyyy")}"));
			dateRow.AddElement(new GridCellBlock($"To: {data.ToDate.ToString("d MMM yyyy")}"));

			grid.AddElement(dateRow);

			if (data.GroupIds?.Any() ?? false) {
				var groupsTitles = _groupRepository.List
					.Where(g => data.GroupIds.Contains(g.Id.ToString()))
					.Select(g => g.Title)
					.OrderBy(g => g).ToList();
				var groupRow = new GridRowBlock();
				groupRow.AddElement(new GridCellBlock("Groups",
					new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
				groupRow.AddElement(new GridCellBlock(string.Join(", ", groupsTitles)));
				groupRow.AddElement(new GridCellBlock());

				grid.AddElement(groupRow);
			}

			if (data.CategoryIds?.Any() ?? false) {
				var categoryTitles = _categoryRepository.List
					.Where(c => data.CategoryIds.Contains(c.Id))
					.Select(c => c.Title)
					.OrderBy(c => c).ToList();
				var categoryRow = new GridRowBlock();
				categoryRow.AddElement(new GridCellBlock("Categories",
					new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
				categoryRow.AddElement(new GridCellBlock(string.Join(", ", categoryTitles)));
				categoryRow.AddElement(new GridCellBlock());

				grid.AddElement(categoryRow);
			}

			if (data.DocumentTypes?.Any() ?? false) {
				var documentTypes = data.DocumentTypes
					.Select(t => VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(t))
					.OrderBy(t => t);
				var typeRow = new GridRowBlock();
				typeRow.AddElement(new GridCellBlock("Document Types",
					new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
				typeRow.AddElement(new GridCellBlock(string.Join(", ", documentTypes)));
				typeRow.AddElement(new GridCellBlock());

				grid.AddElement(typeRow);
			}

			section.AddElement(grid);
		}
	}
}