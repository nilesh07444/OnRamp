using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Enums;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.ViewModel;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler.Reporting {
	public class PointsStatementExportQueryHandler :
		ReportingQueryHandler<PointsStatementExportQuery> {
		private readonly IQueryExecutor _queryExecutor;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ITransientRepository<StandardUser> _standardUserRepository;
		private readonly ITransientRepository<CustomerGroup> _groupRepository;
		private readonly ITransientRepository<DocumentCategory> _categoryRepository;

		public PointsStatementExportQueryHandler(
			IQueryExecutor queryExecutor,
			ICommandDispatcher commandDispatcher,
			ITransientRepository<StandardUser> standardUserRepository,
			ITransientRepository<CustomerGroup> groupRepository,
			ITransientRepository<DocumentCategory> categoryRepository) {
			_queryExecutor = queryExecutor;
			_commandDispatcher = commandDispatcher;
			_standardUserRepository = standardUserRepository;
			_groupRepository = groupRepository;
			_categoryRepository = categoryRepository;
		}

		public override void BuildReport(ReportDocument document, out string title, out string recepitent,PointsStatementExportQuery data) {
			title = "Points Statement";
			recepitent = string.Empty;
			var section = CreateSection(title, PageOrientation.Landscape);

			if (data.CompanyId.HasValue) {
				var companyId = data.CompanyId.Value.ToString();
				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand { CompanyId = companyId });
				_standardUserRepository.SetCustomerCompany(companyId);
				_groupRepository.SetCustomerCompany(companyId);
				_categoryRepository.SetCustomerCompany(companyId);
			}

			CreateHeader(data, section);

			CreateTableData(data, section);

			document.AddElement(section);

			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
		}

		private void CreateTableData(PointsStatementExportQuery data, ReportSection section) {
			
			var userIds = new List<string>();
			if (data.UserId == null) {
				userIds.AddRange(data.UserIds);
			} else {
				userIds.Add(data.UserId.ToString());
			}
			var statement = _queryExecutor.Execute<PointsStatementQuery, PointsStatementViewModel>(new PointsStatementQuery {
				ProvisionalCompanyId = data.ProvisionalCompanyId,
				CompanyId = data.CompanyId,
				//GroupId = data.GroupId,
				UserIds= userIds,
				TrainingLabels=data.TrainingLabels,
				CategoryId = data.CategoryId,
				DocumentTypes = data.DocumentTypes,
				FromDate = data.FromDate,
				ToDate = data.ToDate,
				EnableGlobalAccessDocuments=data.EnableGlobalAccessDocuments
			}).Data;

			var user = new PointsStatementViewModel.UserDetail();
			foreach (var dataItem in statement) {
				user = dataItem.User;
			}

			var grid = CreateGrid();
			var columns = new List<Tuple<string, int>>();
			var headers = data.ToggleFilter.Split(',').ToList();

			var a = new List<string>();
			a.Add("Date");
			a.Add("Duration");
			a.Add("Title");
			a.Add("Document Type");
			a.Add("Access");
			a.Add("Result");
			a.Add("Points");
			a.Add("Category");

			var result = headers.Intersect(a).ToList();
			foreach (var header in result) {
				columns.Add(new Tuple<string, int>(header, 30));
			}
			CreatePersonalDetails(user, section, headers);
			//foreach (var header in headers) {
			//	columns.Add(new Tuple<string, int>(header, 30));
			//}
			CreateTableHeader(grid, columns.ToArray());

			foreach (var dataItem in statement) {
				List<object> list = new List<object>();

				if (headers.Contains("Date")) {
					list.Add(dataItem.Date);
				}
				if (headers.Contains("Duration")) {
					list.Add(dataItem.Duration);
				}
				if (headers.Contains("Title")) {
					list.Add(dataItem.DocumentTitle);
				}
				if (headers.Contains("Document Type")) {
					list.Add(VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(dataItem.DocumentType));
				}
				if (headers.Contains("Access")) {
					list.Add(dataItem.IsGlobalAccessed ? "Global" : "Assigned");
				}
				if (headers.Contains("Result")) {
					list.Add(VirtuaCon.EnumUtility.GetFriendlyName<PointsStatementResult>(dataItem.Result));
				}
				if (headers.Contains("Points")) {
					list.Add(dataItem.Points);
				}
				if (headers.Contains("Category")) {
					list.Add(dataItem.Category);
				}
				//if (headers.Contains("Employee Code")) {
				//	list.Add(dataItem.User.EmployeeNo);
				//}
				//if (headers.Contains("User")) {
				//	list.Add(dataItem.User.FullName);
				//}
				//if (headers.Contains("Category")) {
				//	list.Add(dataItem.Category);
				//}
				//if (headers.Contains("Document Type")) {
				//	list.Add(VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(dataItem.DocumentType));
				//}
				//if (headers.Contains("Title")) {
				//	list.Add(dataItem.DocumentTitle);
				//}
				//if (headers.Contains("Date")) {
				//	list.Add(dataItem.Date);
				//}
				//if (headers.Contains("Access")) {
				//	list.Add(dataItem.IsGlobalAccessed ? "Global" : "Assigned");
				//}
				//if (headers.Contains("Result")) {
				//	list.Add(VirtuaCon.EnumUtility.GetFriendlyName<PointsStatementResult>(dataItem.Result));
				//}
				//if (headers.Contains("Points")) {
				//	list.Add(dataItem.Points);
				//}
				CreateTableDataRow(grid, list.ToArray());
			}


			section.AddElement(grid);
		}

		private void CreatePersonalDetails(PointsStatementViewModel.UserDetail vm, ReportSection section, List<string> header) {
			var grid = CreateGrid();
			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Personal Details" });

			var fields = new List<Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>>();
			if (header.Contains("User")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				"Name", model => model.FullName));
			}
			if (header.Contains("Mobile Number")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				"Mobile Number", model => model.MobileNumber));
			}
			if (header.Contains("Email")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				"Email", model => model.Email));
			}
			if (header.Contains("ID Number")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				"ID Number", model => model.IDNumber));
			}
			if (header.Contains("Gender")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				"Gender", model => model.Gender));
			}
			if (header.Contains("Employee Code")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				"Employee Number", model => model.EmployeeNo));
			}
			if (header.Contains("Race")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				   "Race", model => model.Race));
			}
			if (header.Contains("Group")) {
				fields.Add(new Tuple<string, Func<PointsStatementViewModel.UserDetail, string>>(
				"Group", model => model.GroupTitle));
			}
			foreach (var field in fields) {
				var row = new GridRowBlock();
				row.AddElement(new GridCellBlock(field.Item1,
					new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
					row.AddElement(new GridCellBlock(field.Item2(vm)));
				grid.AddElement(row);
			}

			section.AddElement(grid);
		}

		private void CreateHeader(PointsStatementExportQuery data, ReportSection section) {
			if (data.FromDate.HasValue || data.ToDate.HasValue) {
				var firstGrid = new GridBlock();

				if (data.FromDate.HasValue || data.ToDate.HasValue || data.GroupId.HasValue || data.UserId.HasValue) {
					var dateRangeBlock = new GridRowBlock();
					dateRangeBlock.AddElement(new GridCellBlock("Date Range",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					if (data.FromDate.HasValue) {
						dateRangeBlock.AddElement(new GridCellBlock($"From: {data.FromDate.Value.ToString("d MMM yyyy")}"));
					}

					if (data.ToDate.HasValue) {
						dateRangeBlock.AddElement(new GridCellBlock($"To: {data.ToDate.Value.ToString("d MMM yyyy")}"));
					}

					firstGrid.AddElement(dateRangeBlock);
				}

				if (data.GroupId.HasValue) {
					var groupBlock = new GridRowBlock();
					groupBlock.AddElement(new GridCellBlock("Group",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					var group = _groupRepository.Find(data.GroupId.Value);
					if (group != null)
						groupBlock.AddElement(new GridCellBlock(group.Title));
					firstGrid.AddElement(groupBlock);
				}

				if (data.UserId.HasValue) {
					var userBlock = new GridRowBlock();
					userBlock.AddElement(new GridCellBlock("User",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					var user = _standardUserRepository.Find(data.UserId.Value);
					if (user != null)
						userBlock.AddElement(new GridCellBlock($"{user.FirstName} {user.LastName}"));
					firstGrid.AddElement(userBlock);
				}

				section.AddElement(firstGrid);
			}

			if (data.CategoryId.HasValue || (data.DocumentTypes != null && data.DocumentTypes.Any())) {
				var filterGrid = new GridBlock();

				if (data.CategoryId.HasValue) {
					var categoryBlock = new GridRowBlock();
					categoryBlock.AddElement(new GridCellBlock("Category",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					var category = _categoryRepository.Find(data.CategoryId.ToString());
					if (category != null)
						categoryBlock.AddElement(new GridCellBlock(category.Title));
					filterGrid.AddElement(categoryBlock);
				}

				if (data.DocumentTypes != null && data.DocumentTypes.Any()) {
					var documentTypeBlock = new GridRowBlock();
					documentTypeBlock.AddElement(new GridCellBlock("Document Types",
						new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))));
					documentTypeBlock.AddElement(new GridCellBlock(string.Join(", ",
						data.DocumentTypes.AsQueryable()
							.Select(x => VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(x)))));
					filterGrid.AddElement(documentTypeBlock);
				}

				section.AddElement(filterGrid);
			}
		}
	}

	

}