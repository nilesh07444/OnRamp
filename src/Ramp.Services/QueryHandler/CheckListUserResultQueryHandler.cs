using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.CheckLists;
using Ramp.Contracts.Query.CheckListUserResult;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public	class CheckListUserResultQueryHandler : IQueryHandler<CheckListUserResultQuery, CheckListUserResultViewModel> {

		private readonly ITransientRepository<CheckListUserResult> _checkListUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public CheckListUserResultQueryHandler(ITransientRepository<CheckListUserResult> checkListUserResultRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_checkListUserResultRepository = checkListUserResultRepository;
		}

		public CheckListUserResultViewModel ExecuteQuery(CheckListUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var checkListUserResult =
								_checkListUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.IsGlobalAccessed==query.IsGlobalAccessed && x.UserId== query.UserId).FirstOrDefault();
				if (checkListUserResult == null) return null;
				var result = new CheckListUserResultViewModel {
					AssignedDocumentId = checkListUserResult.AssignedDocumentId,
					Status = checkListUserResult.Status,
					SubmittedDate = checkListUserResult.SubmittedDate,
					Id = checkListUserResult.Id,
				};
				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			} else {
				var checkListUserResult =
								_checkListUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId).FirstOrDefault();
				if (checkListUserResult == null) return null;
				var result = new CheckListUserResultViewModel {
					AssignedDocumentId = checkListUserResult.AssignedDocumentId,
					Status = checkListUserResult.Status,
					SubmittedDate = checkListUserResult.SubmittedDate,
					Id = checkListUserResult.Id
				};
				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
			

			
		}

	}
}
