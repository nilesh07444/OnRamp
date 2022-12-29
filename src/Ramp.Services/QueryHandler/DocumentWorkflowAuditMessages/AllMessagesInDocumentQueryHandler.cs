

using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Ramp.Services.QueryHandler {
	public class AllMessagesInDocumentQueryHandler :
		QueryHandlerBase<FetchAllQuery, List<DocumentWorkflowAuditMessagesViewModel>> {

		private readonly IRepository<DocumentWorkflowAuditMessages> _audit;
		private readonly IRepository<StandardUser> _user;

		public AllMessagesInDocumentQueryHandler(IRepository<DocumentWorkflowAuditMessages> audit, IRepository<StandardUser> user)
		{
			_audit = audit;
			_user = user;
	}

		public override List<DocumentWorkflowAuditMessagesViewModel> ExecuteQuery(FetchAllQuery queryParameters)
		{
			var response = new List<DocumentWorkflowAuditMessagesViewModel>();
			var users = _user.List.ToList();

			var messages =  _audit.List.Where(x=> x.DocumentId.ToString() == queryParameters.Id.ToString() && x.CreatorId == Guid.Parse(queryParameters.Filters)).ToList().OrderByDescending(x=>x.DateCreated).ToList();

			var temp = messages.ToList();
			foreach (var m in messages)
			{
				foreach (var u in users)
				{
					if (m.ApproverId == u.Id && m.ApproverId != Guid.Empty)
					{

						response.Add(new DocumentWorkflowAuditMessagesViewModel
						{
							Id = m.Id,
							CreatorId = m.CreatorId,
							ApproverId = m.ApproverId,
							DocumentId = m.DocumentId,
							Message = m.Message,

							ApproverName = u.FirstName + " " + u.LastName,
							ApproverEmail = u.EmailAddress,

							DateCreated = m.DateCreated
						});
					}
					else if(m.CreatorId == u.Id && m.ApproverId == Guid.Empty) {
						response.Add(new DocumentWorkflowAuditMessagesViewModel
						{
							Id = m.Id,
							CreatorId = m.CreatorId,
							ApproverId = m.ApproverId,
							DocumentId = m.DocumentId,
							Message = m.Message,

							CreatorName = u.FirstName + " " + u.LastName,
							CreatorEmail = u.EmailAddress,

							DateCreated = m.DateCreated
						});
					}
				}
			}

			return response.Distinct().ToList();
		}
	}
}
