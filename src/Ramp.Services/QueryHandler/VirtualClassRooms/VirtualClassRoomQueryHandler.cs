using Common.Query;
using Data.EF.Customer;
using Ramp.Contracts.ViewModel;
using Domain.Customer.Models.VirtualClassRooms;
using Common.Command;
using System;
using Ramp.Services.Projection;
using System.Linq.Expressions;
using LinqKit;
using System.Collections.Generic;
using System.Linq;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Query.VirtualClassroom;
using Common.Data;
using Domain.Customer.Models;

namespace Ramp.Services.QueryHandler.VirtualClassRooms {
	public class VirtualClassRoomQueryHandler : IQueryHandler<FetchByIdQuery, VirtualClassModel>,
			IQueryHandler<FetchAllQuery, IEnumerable<VirtualClassModel>>,
			IQueryHandler<MeetingReportQuery, IEnumerable<MeetingReportUsageModel>> {

		readonly ITransientReadRepository<VirtualClassRoom> _repository;
		private readonly IRepository<StandardUser> _userRepository;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		readonly ICommandDispatcher _commandDispatcher;
		private readonly IQueryExecutor _queryExecutor;
		private readonly ITransientReadRepository<DocumentUsage> _documentUsageRepository;
		public VirtualClassRoomQueryHandler(IRepository<AssignedDocument> assignedDocumentRepository, IRepository<StandardUser> userRepository, ITransientReadRepository<DocumentUsage> documentUsageRepository, ITransientReadRepository<VirtualClassRoom> repository, ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor) {
			_repository = repository;
			_commandDispatcher = commandDispatcher;
			_queryExecutor = queryExecutor;
			_documentUsageRepository = documentUsageRepository;
			_userRepository = userRepository;
			_assignedDocumentRepository = assignedDocumentRepository;
		}

		public IEnumerable<VirtualClassModel> ExecuteQuery(FetchAllQuery query) {
			if (!string.IsNullOrEmpty(query.SearchText) || !string.IsNullOrEmpty(query.Filters)) {
				var searchText = query.SearchText.ToLower();

				var virtualClass = _repository.List.Where(c => !c.Deleted && c.VirtualClassRoomName.ToLower().Contains(searchText)).AsQueryable().Select(Project.VirtualClassRoom_VirtualClassModel).ToList();
				if (!string.IsNullOrEmpty(query.Filters)) {
					var meetingList = new List<VirtualClassModel>();
					var filters = query.Filters.ToLower().Split(',').ToList();
					foreach (var item in filters) {

						meetingList.AddRange(virtualClass.Where(c => c.StatusClass.ToLower().Contains(item)).ToList());
					}
					virtualClass = meetingList;
				}
				return virtualClass;

			} else {
				var virtualClass = _repository.List.Where(c => !c.Deleted).AsQueryable().Select(Project.VirtualClassRoom_VirtualClassModel);

				return virtualClass;
			}

		}
		public VirtualClassModel ExecuteQuery(FetchByIdQuery query) {

			var virtualClass = _repository.Find(Guid.Parse(query.Id.ToString()));
			if (virtualClass == null) return new VirtualClassModel();
			var model = Project.VirtualClassRoom_VirtualClassModel.Invoke(virtualClass);

			return model;
		}
		public IEnumerable<MeetingReportUsageModel> ExecuteQuery(MeetingReportQuery query) {
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
			if (!string.IsNullOrEmpty(query.Status)) {
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
				model = model.Where(c => c.ViewDate.Date >= query.StartDate.Value.Date).ToList();
			} else if (query.EndDate != null) {
				model = model.Where(c => c.ViewDate.Date <= query.EndDate.Value.Date).ToList();
			}

			return model;
		}
	}
}
namespace Ramp.Services.Projection {
	public static partial class Project {
		static DateTime today = DateTime.Now;
		public static readonly Expression<Func<VirtualClassRoom, VirtualClassModel>> VirtualClassRoom_VirtualClassModel =
		  x => new VirtualClassModel {
			  Id = x.Id,
			  VirtualClassRoomName = x.VirtualClassRoomName,
			  Description = x.Description,
			  IsPasswordProtection = x.IsPasswordProtection,
			  Password = x.Password,
			  StartDate = x.StartDate.ToLocalTime().ToString(),
			  EndDate = x.EndDate.ToLocalTime().ToString(),
			  StartDateTime = x.StartDate.ToLocalTime(),
			  EndDateTime = x.EndDate.ToLocalTime(),
			  CreatedOn = x.CreatedOn,
			  CreatedBy = x.CreatedBy,
			  LastEditDate = x.LastEditDate,
			  LastEditedBy = x.LastEditedBy,
			  Deleted = x.Deleted,
			  ReferenceId = x.ReferenceId,
			  IsPublicAccess = x.IsPublicAccess,
			  JitsiServerName = x.JitsiServerName,
			  StatusClass = x.StartDate.ToLocalTime() > today ? "bnotStarted" : x.EndDate.ToLocalTime() < today ? "ended" : "aInprogress",
			  Status = x.StartDate.ToLocalTime() > today ? "PENDING" : x.EndDate.ToLocalTime() < today ? "ENDED" : "IN PROGRESS"
		  };


	}
}
