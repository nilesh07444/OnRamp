using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Policy;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler
{
	public class PolicyContentBoxUserResultQueryHandler : IQueryHandler<PolicyContentBoxUserResultQuery, PolicyContentBoxUserResultViewModel>
	{
		private readonly ITransientRepository<PolicyContentBoxUserResult> _PolicyContentBoxUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public PolicyContentBoxUserResultQueryHandler(ITransientRepository<PolicyContentBoxUserResult> PolicyContentBoxResultRepository,
			ICommandDispatcher commandDispatcher)
		{
			_commandDispatcher = commandDispatcher;
			_PolicyContentBoxUserResultRepository = PolicyContentBoxResultRepository;
		}


		public PolicyContentBoxUserResultViewModel ExecuteQuery(PolicyContentBoxUserResultQuery query)
		{

			if (query.IsGlobalAccessed)
			{
				var PolicyContentUserResult = _PolicyContentBoxUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.PolicyContentBoxId == query.PolicyContentBoxId && x.UserId == query.UserId).FirstOrDefault();
				if (PolicyContentUserResult == null) return null;
				var result = new PolicyContentBoxUserResultViewModel
				{
					AssignedDocumentId = PolicyContentUserResult.AssignedDocumentId,
					PolicyContentBoxId = PolicyContentUserResult.PolicyContentBoxId,
					IsChecked = PolicyContentUserResult.IsChecked,
					CreatedDate = PolicyContentUserResult.CreatedDate,
					IssueDiscription = PolicyContentUserResult.IssueDiscription,
					IsActionNeeded = PolicyContentUserResult.IsActionNeeded

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
			else
			{
				var PolicyContentUserResult = _PolicyContentBoxUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.PolicyContentBoxId == query.PolicyContentBoxId).FirstOrDefault();
				if (PolicyContentUserResult == null) return null;
				var result = new PolicyContentBoxUserResultViewModel
				{
					AssignedDocumentId = PolicyContentUserResult.AssignedDocumentId,
					PolicyContentBoxId = PolicyContentUserResult.PolicyContentBoxId,
					IsChecked = PolicyContentUserResult.IsChecked,
					CreatedDate = PolicyContentUserResult.CreatedDate,
					IssueDiscription = PolicyContentUserResult.IssueDiscription,
					IsActionNeeded = PolicyContentUserResult.IsActionNeeded

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
		}

	}
}
