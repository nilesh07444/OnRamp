using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Memos;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public	class MemoUserResultQueryHandler : IQueryHandler<MemoUserResultQuery, MemoUserResultViewModel> {

		private readonly ITransientRepository<MemoUserResult> _MemoUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public MemoUserResultQueryHandler(ITransientRepository<MemoUserResult> MemoUserResultRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_MemoUserResultRepository = MemoUserResultRepository;
		}

		public MemoUserResultViewModel ExecuteQuery(MemoUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var MemoUserResult =
								_MemoUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.IsGlobalAccessed==query.IsGlobalAccessed && x.UserId== query.UserId).FirstOrDefault();
				if (MemoUserResult == null) return null;
				var result = new MemoUserResultViewModel {
					AssignedDocumentId = MemoUserResult.AssignedDocumentId,
					Status = MemoUserResult.Status,
					SubmittedDate = MemoUserResult.SubmittedDate,
					Id = MemoUserResult.Id,
				};
				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			} else {
				var MemoUserResult =
								_MemoUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId).FirstOrDefault();
				if (MemoUserResult == null) return null;
				var result = new MemoUserResultViewModel {
					AssignedDocumentId = MemoUserResult.AssignedDocumentId,
					Status = MemoUserResult.Status,
					SubmittedDate = MemoUserResult.SubmittedDate,
					Id = MemoUserResult.Id
				};
				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
			

			
		}

	}
}
