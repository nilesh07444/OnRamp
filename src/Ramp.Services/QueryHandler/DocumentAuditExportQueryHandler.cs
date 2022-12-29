using Common.Data;
using Domain.Customer;
using Domain.Customer.Models.DocumentTrack;
using Ramp.Contracts.Query.DocumentAudit;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Linq;
using VirtuaCon.Reporting;

namespace Ramp.Services.QueryHandler {
	public class DocumentAuditExportQueryHandler : ReportingQueryHandler<DocumentAuditFilterQuery> {

		readonly IRepository<DocumentAuditTrack> _repository;
		public DocumentAuditExportQueryHandler(IRepository<DocumentAuditTrack> repository) {
			_repository = repository;
		}

		public override void BuildReport(ReportDocument document, out string title,out string recepitent, DocumentAuditFilterQuery query) {
			var audits = GetDocumentAuditDetails(query);
			title = "DocumentAuditReport";
			recepitent = query.Recepients;
			query.AddOnrampBranding = true;
			var section = CreateSection(title, PageOrientation.Landscape);

			if (audits.TrainigManualList.Any())
				CreateManualTableData(audits, section);
			if (audits.TestList.Any())
				CreateTestTableData(audits, section);
			if (audits.MemoList.Any())
				CreateMemoTableData(audits, section);
			if (audits.CheckList.Any())
				CreateCheckListTableData(audits, section);
			if (audits.PolicyList.Any())
				CreatePolicyTableData(audits, section);

			document.AddElement(section);

		}
		private void CreateManualTableData(AuditReportModel data, ReportSection section) {
			
			var columns = new[]
			{
				new Tuple<string, int>("Date", 30),
				new Tuple<string, int>("Type", 40),
				new Tuple<string, int>("Administrator", 40)

			};
			var trainings = data.TrainigManualList.GroupBy(c => c.DocumentId).ToList();
			foreach (var item in trainings) {
				var grid = CreateGrid();
				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Manual's: " + item.FirstOrDefault().DocumentName });
				CreateTableHeader(grid, columns);
				foreach (var manual in item.ToList()) {
					
					CreateTableDataRow(grid,
											manual.LastEditDate.ToString(),
											manual.DocumentStatus,
											manual.UserName
											);
				}
				section.AddElement(grid);
			}

		}
		private void CreateTestTableData(AuditReportModel data, ReportSection section) {
			
			var columns = new[]
			{
				new Tuple<string, int>("Date", 30),
				new Tuple<string, int>("Type", 40),
				new Tuple<string, int>("Administrator", 40)

			};

			var tests = data.TestList.GroupBy(c => c.DocumentId).ToList();
			foreach (var item in tests) {
				var grid = CreateGrid();
				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Test's: " + item.FirstOrDefault().DocumentName });
				CreateTableHeader(grid, columns);
				foreach (var test in item.ToList()) {
					
					CreateTableDataRow(grid,
											test.LastEditDate.ToString(),
											test.DocumentStatus,
											test.UserName
											);
				}
				section.AddElement(grid);
			}


		}
		private void CreateMemoTableData(AuditReportModel data, ReportSection section) {
			
			var columns = new[]
			{
				new Tuple<string, int>("Date", 30),
				new Tuple<string, int>("Type", 40),
				new Tuple<string, int>("Administrator", 40)

			};

			var memos = data.MemoList.GroupBy(c => c.DocumentId).ToList();
			foreach (var item in memos) {
				var grid = CreateGrid();
				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Memo's: " + item.FirstOrDefault().DocumentName });
				CreateTableHeader(grid, columns);
				foreach (var memo in item.ToList()) {
					
					CreateTableDataRow(grid,
											memo.LastEditDate.ToString(),
											memo.DocumentStatus,
											memo.UserName
											);
				}
				section.AddElement(grid);
			}
		}
		private void CreateCheckListTableData(AuditReportModel data, ReportSection section) {
			
			var columns = new[]
			{
				new Tuple<string, int>("Date", 30),
				new Tuple<string, int>("Type", 40),
				new Tuple<string, int>("Administrator", 40)

			};

			var checllists = data.CheckList.GroupBy(c => c.DocumentId).ToList();
			foreach (var item in checllists) {
				var grid = CreateGrid();
				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Checklist's: " + item.FirstOrDefault().DocumentName });
				CreateTableHeader(grid, columns);

				foreach (var checklist in item.ToList()) {
					CreateTableDataRow(grid,
											checklist.LastEditDate.ToString(),
											checklist.DocumentStatus,
											checklist.UserName
											);
				}
				section.AddElement(grid);
			}


		}
		private void CreatePolicyTableData(AuditReportModel data, ReportSection section) {
			
			var columns = new[]
			{
				new Tuple<string, int>("Date", 30),
				new Tuple<string, int>("Type", 40),
				new Tuple<string, int>("Administrator", 40)

			};

			var policies = data.PolicyList.GroupBy(c => c.DocumentId).ToList();
			foreach (var item in policies) {
				var grid = CreateGrid();
				CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Policy's: " + item.FirstOrDefault().DocumentName });
				CreateTableHeader(grid, columns);
				foreach (var policy in item.ToList()) {
					
					CreateTableDataRow(grid,
											policy.LastEditDate.ToString(),
											policy.DocumentStatus,
											policy.UserName
											);
				}
				section.AddElement(grid);
			}


		}
		public AuditReportModel GetDocumentAuditDetails(DocumentAuditFilterQuery query) {


			var documentAudit = _repository.List.Where(c => query.DocumentList.Contains(c.DocumentId)).AsQueryable().Select(Project.DocumentAudit_DocumentAuditModel).ToList();
			if (query.StartDate.HasValue && query.EndDate.HasValue) {
				documentAudit = documentAudit.Where(c => c.LastEditDate.Value <= query.EndDate.Value && c.LastEditDate.Value >= query.StartDate.Value).ToList();
			} else if (query.StartDate.HasValue) {
				documentAudit = documentAudit.Where(c => c.LastEditDate.Value >= query.StartDate.Value).ToList();
			} else if (query.EndDate.HasValue) {
				documentAudit = documentAudit.Where(c => c.LastEditDate.Value <= query.EndDate.Value).ToList();
			}
			var model = new AuditReportModel() {
				TrainigManualList = documentAudit.Where(c => c.DocumentType == DocumentType.TrainingManual.ToString()).ToList(),
				CheckList = documentAudit.Where(c => c.DocumentType == DocumentType.Checklist.ToString()).ToList(),
				MemoList = documentAudit.Where(c => c.DocumentType == DocumentType.Memo.ToString()).ToList(),
				TestList = documentAudit.Where(c => c.DocumentType == DocumentType.Test.ToString()).ToList(),
				PolicyList = documentAudit.Where(c => c.DocumentType == DocumentType.Policy.ToString()).ToList()
			};

			return model;

		}

	}
}
