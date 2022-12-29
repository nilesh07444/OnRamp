using Common.Data;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.VirtualClassRooms;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using VirtuaCon.Reporting;

namespace Ramp.Services.QueryHandler {
	public class MeetingExportReportQueryHandler : ReportingQueryHandler<MeetingReportQuery> {
		readonly ITransientReadRepository<VirtualClassRoom> _repository;
	private readonly IRepository<StandardUser> _userRepository;
	private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
	private readonly ITransientReadRepository<DocumentUsage> _documentUsageRepository;

	public MeetingExportReportQueryHandler(IRepository<AssignedDocument> assignedDocumentRepository, IRepository<StandardUser> userRepository, ITransientReadRepository<DocumentUsage> documentUsageRepository, ITransientReadRepository<VirtualClassRoom> repository) {
		_repository = repository;
		_documentUsageRepository = documentUsageRepository;
		_userRepository = userRepository;
		_assignedDocumentRepository = assignedDocumentRepository;
	}

	public override void BuildReport(ReportDocument document, out string title,out string recepitent, MeetingReportQuery query) {
			var meeting = GetMeetingList( query);
			title = "MeetingReport";
			recepitent = "";
			query.AddOnrampBranding = true;
			var section = CreateSection(title, PageOrientation.Landscape);
			CreateTableData(meeting, query.ToggleFilter, section);
			document.AddElement(section);

		}

		private void CreateTableData(IEnumerable<MeetingReportUsageModel> data, string ToggleFilter, ReportSection section) {

			var grid = CreateGrid();
			var columns = new List<Tuple<string, int>>();
			if (!string.IsNullOrEmpty(ToggleFilter)) {
				var headers = ToggleFilter.Split(',').ToList();
				foreach (var header in headers) {
					columns.Add(new Tuple<string, int>(header, 30));
				}

				CreateTableHeader(grid, columns.ToArray());

				foreach (var dataItem in data.ToList()) {
					List<object> list = new List<object>();
					if (headers.Contains("User Name")) {
						list.Add(dataItem.UserName);
					}
					if (headers.Contains("Document")) {
						list.Add(dataItem.DocumentName);
					}
					if (headers.Contains("Attendance")) {
						list.Add(dataItem.Status);
					}
					if (headers.Contains("Date Joined")) {
						list.Add(dataItem.DateJoined);
					}

					CreateTableDataRow(grid, list.ToArray());
				}
			}

			section.AddElement(grid);

		}


		public IEnumerable<MeetingReportUsageModel> GetMeetingList(MeetingReportQuery query) {
			var meetings = _repository.List.AsQueryable().Where(x => !x.Deleted && query.MeetingIds.Contains(x.Id.ToString())).ToList();
			var assignedDocs = _assignedDocumentRepository.GetAll().Where(c => query.MeetingIds.Contains(c.DocumentId) && !c.Deleted).ToList();
			var docUses = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => query.MeetingIds.Contains(c.DocumentId)).ToList();

			var result = (
				from a in assignedDocs
				join u in _userRepository.GetAll() on Guid.Parse(a.UserId) equals u.Id
				join v in _repository.List.AsQueryable() on Guid.Parse(a.DocumentId) equals v.Id
				select new {
					DocumentName = v.VirtualClassRoomName,
					DocumentId = a.DocumentId,
					UserId = u.Id,
					UserName = u.FirstName + " " + u.LastName,
					AssignedId = a.Id,

				}).ToList();
			var model = new List<MeetingReportUsageModel>();
			foreach (var item in result) {
				var viewDocs = docUses.Where(c => c.UserId == item.UserId.ToString()).ToList();
				var meetingReport = new MeetingReportUsageModel() {
					UserId = item.UserId.ToString(),
					UserName = item.UserName,
					DocumentId = item.DocumentId,
					DocumentName = item.DocumentName
				};
				meetingReport.ViewDate = (viewDocs != null && viewDocs.Any()) ? viewDocs.FirstOrDefault().ViewDate : DateTime.UtcNow;
				meetingReport.DateJoined = (viewDocs != null && viewDocs.Any()) ? viewDocs.FirstOrDefault().ViewDate.ToLocalTime().ToString() : "";
				meetingReport.Status = (viewDocs != null && viewDocs.Any()) ? "Yes" : "No";
				model.Add(meetingReport);
			}
			if ( !string.IsNullOrEmpty(query.Status)) {
				var item = query.Status.Split(',');
				if (item.Count() == 1) {
					if (item[0] == "1") {
						model = model.Where(c => c.Status == "Yes").ToList();
					} else if (item[0] == "0") {
						model = model.Where(c => c.Status == "No").ToList();
					}
				}
			}
			if (query.StartDate != null && query.EndDate != null) {
				model = model.Where(c => c.ViewDate.Date >= query.StartDate.Value.Date && c.ViewDate.Date <= query.EndDate.Value.Date).ToList();
			} else if (query.StartDate != null) {
				model = model.Where(c => c.ViewDate >= query.StartDate.Value).ToList();
			} else if (query.EndDate != null) {
				model = model.Where(c => c.ViewDate <= query.EndDate.Value).ToList();
			}
			return model;
		}
	}
}
