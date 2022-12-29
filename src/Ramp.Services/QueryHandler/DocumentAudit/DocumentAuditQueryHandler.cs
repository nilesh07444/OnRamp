using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.DocumentTrack;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqKit;
using Ramp.Contracts.Query.DocumentAudit;
using Domain.Customer;

namespace Ramp.Services.QueryHandler.DocumentAudit {
	public class DocumentAuditQueryHandler : IQueryHandler<FetchByIdQuery, DocumentAuditModel>,
		IQueryHandler<DocumentAuditFilterQuery, AuditReportModel>,
		IQueryHandler<FetchAllQuery, IEnumerable<DocumentAuditModel>> {
		readonly ITransientReadRepository<DocumentAuditTrack> _repository;
		private readonly IRepository<StandardUser> _userRepository;
		readonly ICommandDispatcher _commandDispatcher;
		private readonly IQueryExecutor _queryExecutor;
		public DocumentAuditQueryHandler(IRepository<StandardUser> userRepository, ITransientReadRepository<DocumentAuditTrack> repository, ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor) {
			_repository = repository;
			_commandDispatcher = commandDispatcher;
			_queryExecutor = queryExecutor;
			_userRepository = userRepository;
		}

		public AuditReportModel ExecuteQuery(DocumentAuditFilterQuery query) {

			var documentAudit = _repository.List.Where(c => query.Documents.Split(',').ToList().Contains(c.DocumentId)).AsQueryable().Select(Project.DocumentAudit_DocumentAuditModel).ToList();
			if (query.StartDate.HasValue && query.EndDate.HasValue) {
				documentAudit = documentAudit.Where(c => c.LastEditDate.Value <= query.EndDate.Value && c.LastEditDate.Value >= query.StartDate.Value).ToList();
			} else if (query.StartDate.HasValue) {
				documentAudit = documentAudit.Where(c => c.LastEditDate.Value >= query.StartDate.Value).ToList();
			} else if (query.EndDate.HasValue) {
				documentAudit = documentAudit.Where(c => c.LastEditDate.Value <= query.EndDate.Value).ToList();
			}
			var model = new AuditReportModel() {
				TrainigManualList = documentAudit.Where(c => c.DocumentType == DocumentType.TrainingManual.ToString()).ToList(),
				CheckList= documentAudit.Where(c => c.DocumentType == DocumentType.Checklist.ToString()).ToList(),
				MemoList= documentAudit.Where(c => c.DocumentType == DocumentType.Memo.ToString()).ToList(),
				TestList= documentAudit.Where(c => c.DocumentType == DocumentType.Test.ToString()).ToList(),
				PolicyList= documentAudit.Where(c => c.DocumentType == DocumentType.Policy.ToString()).ToList()
			};
			
			return model;

		}


		public IEnumerable<DocumentAuditModel> ExecuteQuery(FetchAllQuery query) {
			var documentAudit = _repository.List.AsQueryable().Select(Project.DocumentAudit_DocumentAuditModel).ToList();

			return documentAudit;

		}
		public DocumentAuditModel ExecuteQuery(FetchByIdQuery query) {

			var documentAudit = _repository.Find(Guid.Parse(query.Id.ToString()));
			if (documentAudit == null) return new DocumentAuditModel();
			var model = Project.DocumentAudit_DocumentAuditModel.Invoke(documentAudit);

			return model;
		}
	}

}
namespace Ramp.Services.Projection {
	public static partial class Project {

		public static readonly Expression<Func<DocumentAuditTrack, DocumentAuditModel>> DocumentAudit_DocumentAuditModel =
		  x => new DocumentAuditModel {
			  Id = x.Id,
			  LastEditDate = x.LastEditDate.Value.ToLocalTime(),
			  LastEditedBy = x.LastEditedBy,
			  UserId = x.User.Id,
			  DocumentId = x.DocumentId,
			  DocumentStatus = x.DocumentStatus,
			  DocumentType=x.DocumentType,
			  DocumentName=x.DocumentName,
			  UserName = x.UserName

		  };


	}
}
