using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common;
using Common.Data;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.UserFeedback;
using Ramp.Contracts.ViewModel;
using VirtuaCon;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler {
	public class UserFeedbackExportQueryHandler : ReportingQueryHandler<UserFeedbackExportQuery> {
		private readonly IRepository<UserFeedback> _userFeedbackRepository;
		private readonly IRepository<StandardUser> _standardUserRepository;
		private readonly IQueryExecutor _queryExecutor;

		public UserFeedbackExportQueryHandler(
			IRepository<UserFeedback> userFeedbackRepository,
			IRepository<StandardUser> standardUserRepository,
			IQueryExecutor queryExecutor) {
			_userFeedbackRepository = userFeedbackRepository;
			_standardUserRepository = standardUserRepository;
			_queryExecutor = queryExecutor;
		}

		public override void BuildReport(ReportDocument document, out string title, out string RecipRecepients, UserFeedbackExportQuery data) {
			title = "Content Feedback Report";
			RecipRecepients=data.Recepients ;
			var section = CreateSection(title, PageOrientation.Landscape);

			CreateHeader(data, section);
			
				CreateTableData(data, section);

			document.AddElement(section);
		}

		private void CreateTableData(UserFeedbackExportQuery data, ReportSection section) {


			var userFeedback = _userFeedbackRepository.List.Where(f => f.Deleted == false);
			var test = userFeedback.ToList();
			if (data.FromDate.HasValue)
				userFeedback = userFeedback.Where(f => f.Created >= data.FromDate.Value.AtBeginningOfDay());
			if (data.ToDate.HasValue)
				userFeedback = userFeedback.Where(f => f.Created <= data.ToDate.Value.AtEndOfDay());

			if (data.Documents?.Any() ?? false) {
				userFeedback = userFeedback.Where(f =>
					data.Documents.Any(d => d.DocumentId == f.DocumentId) && data.Documents.FirstOrDefault(d => d.DocumentId == f.DocumentId)?.DocumentType == f.DocumentType);
			} else if (data.DocumentTypes?.Any() ?? false) {
				userFeedback = userFeedback.Where(f => data.DocumentTypes.Any(t => t == f.DocumentType));
			}

			if (data.FeedbackTypes?.Any() ?? false) {
				userFeedback = userFeedback.Where(f => data.FeedbackTypes.Any(t => t == f.ContentType));
			}

			var documents = userFeedback.Select(f => new { DocumentType = f.DocumentType, Id = f.DocumentId }).Distinct().Select(
				d => _queryExecutor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery {
					Id = d.Id,
					DocumentType = d.DocumentType
				})).Where(c=>c.IsGlobalAccessed== data.IsGlobalAccess).ToDictionary(d => d.Id);

			var userIds = userFeedback.Select(f => f.CreatedById).Distinct();
			var users = _standardUserRepository.List.Where(u => userIds.Any(id => id == u.Id.ToString())).Select(u => new {
				Id = u.Id,
				Name = $"{u.FirstName} {u.LastName}",
				Group = u.Group?.Title ?? ""
			}).ToDictionary(u => u.Id.ToString());

			if (!string.IsNullOrEmpty(data.Text)) {
				userFeedback = userFeedback.Where(f =>
					$"{VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(f.DocumentType)} Feedback - {f.ContentType.ToString()} from {(users.ContainsKey(f.CreatedById) ? users[f.CreatedById].Name : "Deleted")}"
						.ToLower().Contains(data.Text.ToLower()));
			}

			var grid = CreateGrid();
			var columns = new[]
			{
				new Tuple<string, int>("User", 40),
				new Tuple<string, int>("Date", 50),
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Comment", 60)
			};
			CreateTableHeader(grid, columns);


			// Memo Grid
			var memoFeedback = userFeedback.Where(d => d.DocumentType == DocumentType.Memo ).GroupBy(c => c.DocumentId).Select(f => f.First()).ToList();
			foreach (var model in memoFeedback) {
				var mGrid = CreateGrid();
				var mColumns = new[]
				{
				 new Tuple<string, int>("User", 40),
				new Tuple<string, int>("Date", 50),
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Comment", 60)
			};
				var doc = documents.ContainsKey(model.DocumentId);
				if (doc) {
					var results = userFeedback.Where(c => c.DocumentId == model.DocumentId).ToList();
					CreateTableDataRowWithStyles(mGrid, HeaderStyle, new[] { "Memo: " + documents[model.DocumentId].Title });
					CreateTableHeader(mGrid, mColumns);
					foreach (var item in results) {
						CreateTableDataRow(mGrid,
					users.ContainsKey(item.CreatedById) ? users[item.CreatedById].Name : "Deleted",
												item.Created.ToString("d MMM yyyy HH:mm"),
												item.ContentType.ToString(),
												item.Content
												);
					}
					section.AddElement(mGrid);
				}
			}

			// Training Manual Grid
			var tmFeedback = userFeedback.Where(d => d.DocumentType == DocumentType.TrainingManual).GroupBy(c => c.DocumentId).Select(f => f.First()).ToList();
			foreach (var model in tmFeedback) {
				var tmGrid = CreateGrid();
				var tmColumns = new[]
				{
				 new Tuple<string, int>("User", 40),
				new Tuple<string, int>("Date", 50),
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Comment", 60)
			};
				var results = userFeedback.Where(c => c.DocumentId == model.DocumentId).ToList();
				var doc = documents.ContainsKey(model.DocumentId);
				if (doc) {
					CreateTableDataRowWithStyles(tmGrid, HeaderStyle, new[] { "Training Manual: " + documents[model.DocumentId].Title });
					CreateTableHeader(tmGrid, tmColumns);
					foreach (var item in results) {
						CreateTableDataRow(tmGrid,
					users.ContainsKey(item.CreatedById) ? users[item.CreatedById].Name : "Deleted",
												item.Created.ToString("d MMM yyyy HH:mm"),
												item.ContentType.ToString(),
												item.Content
												);
					}
					section.AddElement(tmGrid);
				}
			}

			// Checklist Grid
			var chkFeedback = userFeedback.Where(d => d.DocumentType == DocumentType.Checklist).GroupBy(c => c.DocumentId).Select(f => f.First()).ToList();
			foreach (var model in chkFeedback) {
				var chkGrid = CreateGrid();
				var chkColumns = new[]
				{
				 new Tuple<string, int>("User", 40),
				new Tuple<string, int>("Date", 50),
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Comment", 60)
			};
				var doc = documents.ContainsKey(model.DocumentId);
				if (doc) {
					CreateTableDataRowWithStyles(chkGrid, HeaderStyle, new[] { "Checklist: " + documents[model.DocumentId].Title });
					CreateTableHeader(chkGrid, chkColumns);
					var results = userFeedback.Where(c => c.DocumentId == model.DocumentId).ToList();
					foreach (var item in results) {
						CreateTableDataRow(chkGrid,
					users.ContainsKey(item.CreatedById) ? users[item.CreatedById].Name : "Deleted",
												item.Created.ToString("d MMM yyyy HH:mm"),
												item.ContentType.ToString(),
												item.Content
												);
					}

					section.AddElement(chkGrid);
				}
			}

			// Test Grid
			var testFeedback = userFeedback.Where(d => d.DocumentType == DocumentType.Test).GroupBy(c => c.DocumentId).Select(f => f.First()).ToList();
			foreach (var model in testFeedback) {
				var testGrid = CreateGrid();
				var testColumns = new[]
				{
				 new Tuple<string, int>("User", 40),
				new Tuple<string, int>("Date", 50),
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Comment", 60)
			};
				var doc = documents.ContainsKey(model.DocumentId);
				if (doc) {
					CreateTableDataRowWithStyles(testGrid, HeaderStyle, new[] { "Test: " + documents[model.DocumentId].Title });
					CreateTableHeader(testGrid, testColumns);
					var results = userFeedback.Where(c => c.DocumentId == model.DocumentId).ToList();
					foreach (var item in results) {
						CreateTableDataRow(testGrid,
					users.ContainsKey(item.CreatedById) ? users[item.CreatedById].Name : "Deleted",
												item.Created.ToString("d MMM yyyy HH:mm"),
												item.ContentType.ToString(),
												item.Content
												);
					}

					section.AddElement(testGrid);
				}
			}

			// Policy Grid
			var policyFeedback = userFeedback.Where(d => d.DocumentType == DocumentType.Policy).GroupBy(c => c.DocumentId).Select(f => f.First()).ToList();
			foreach (var model in policyFeedback) {
				var policyGrid = CreateGrid();
				var policyColumns = new[]
				{
				 new Tuple<string, int>("User", 40),
				new Tuple<string, int>("Date", 50),
				new Tuple<string, int>("Type", 30),
				new Tuple<string, int>("Comment", 60)
			};
				var doc = documents.ContainsKey(model.DocumentId);
				if (doc) {
					CreateTableDataRowWithStyles(policyGrid, HeaderStyle, new[] { "Policy: " + documents[model.DocumentId].Title });
					CreateTableHeader(policyGrid, policyColumns);
					var results = userFeedback.Where(c => c.DocumentId == model.DocumentId).ToList();
					foreach (var item in results) {
						CreateTableDataRow(policyGrid,
					users.ContainsKey(item.CreatedById) ? users[item.CreatedById].Name : "Deleted",
												item.Created.ToString("d MMM yyyy HH:mm"),
												item.ContentType.ToString(),
												item.Content
												);
					}

					section.AddElement(policyGrid);
				}
			}
		}
			

		private void CreateHeader(UserFeedbackExportQuery data, ReportSection section) {
			if (data.FromDate.HasValue || data.ToDate.HasValue) {
				var headerGrid = new GridBlock();

				var dateRangeBlock = new GridRowBlock();
				dateRangeBlock.AddElement(new GridCellBlock("Date Range",
					new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
				if (data.FromDate.HasValue) {
					dateRangeBlock.AddElement(new GridCellBlock($"From: {data.FromDate.Value.ToString("d MMM yyyy")}"));
				}
				if (data.ToDate.HasValue) {
					dateRangeBlock.AddElement(new GridCellBlock($"To: {data.ToDate.Value.ToString("d MMM yyyy")}"));
				}

				headerGrid.AddElement(dateRangeBlock);
				section.AddElement(headerGrid);
			}

			if ((data.Documents?.Any() ?? false) ||
				(data.DocumentTypes?.Any() ?? false) ||
				(data.FeedbackTypes?.Any() ?? false) ||
				!string.IsNullOrWhiteSpace(data.Text)) {
				var filterGrid = new GridBlock();

				if (data.Documents?.Any() ?? false) {
					var documentBlock = new GridRowBlock();
					documentBlock.AddElement(new GridCellBlock("Document",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					var documents = new List<DocumentListModel>();
					data.Documents.ForEach(d => {
						var document = _queryExecutor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery {
							DocumentType = d.DocumentType,
							Id = d.DocumentId
						});
						if (document != null)
							documents.Add(document);
					});
					if (documents.Any())
						documentBlock.AddElement(new GridCellBlock(string.Join(Environment.NewLine, documents.Select(d => d.Title))));

					filterGrid.AddElement(documentBlock);
				} else if (data.DocumentTypes?.Any() ?? false) {
					var documentTypesBlock = new GridRowBlock();
					documentTypesBlock.AddElement(new GridCellBlock("Document Feedback",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					var documentTypes = string.Join(", ", data.DocumentTypes
						.Select(x => VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(x)));
					//documentTypesBlock.AddElement(new GridCellBlock(documentTypes));
					filterGrid.AddElement(documentTypesBlock);
				}

				if (data.FeedbackTypes?.Any() ?? false) {
					var feedbackTypesBlock = new GridRowBlock();
					feedbackTypesBlock.AddElement(new GridCellBlock("Feedback Types",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					var feedbackTypes = string.Join(", ", data.FeedbackTypes
						.Select(x => VirtuaCon.EnumUtility.GetFriendlyName<UserFeedbackContentType>(x)));
					feedbackTypesBlock.AddElement(new GridCellBlock(feedbackTypes));

					filterGrid.AddElement(feedbackTypesBlock);
				}

				if (!string.IsNullOrWhiteSpace(data.Text)) {
					var referenceBlock = new GridRowBlock();
					referenceBlock.AddElement(new GridCellBlock("Reference",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					referenceBlock.AddElement(new GridCellBlock(data.Text));

					filterGrid.AddElement(referenceBlock);
				}

				section.AddElement(filterGrid);
			}
		}
	}
}