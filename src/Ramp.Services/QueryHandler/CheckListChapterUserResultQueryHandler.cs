using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.CheckLists;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public class CheckListChapterUserResultQueryHandler : IQueryHandler<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel> {
		private readonly ITransientRepository<CheckListChapterUserResult> _checkListChapterUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public CheckListChapterUserResultQueryHandler(ITransientRepository<CheckListChapterUserResult> checkListChapterUserResultRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_checkListChapterUserResultRepository = checkListChapterUserResultRepository;
		}

		public CheckListChapterUserResultViewModel ExecuteQuery(CheckListChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var checkListUserResult =
				_checkListChapterUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.CheckListChapterId == query.CheckListChapterId && x.UserId==query.UserId).FirstOrDefault();
				if (checkListUserResult == null) return null;
				var result = new CheckListChapterUserResultViewModel {
					AssignedDocumentId = checkListUserResult.AssignedDocumentId,
					CheckListChapterId = checkListUserResult.CheckListChapterId,
					IsChecked = checkListUserResult.IsChecked,
					CreatedDate = checkListUserResult.CreatedDate,
					IssueDiscription = checkListUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			} else {
				var checkListUserResult =
				_checkListChapterUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.CheckListChapterId == query.CheckListChapterId).FirstOrDefault();
				if (checkListUserResult == null) return null;
				var result = new CheckListChapterUserResultViewModel {
					AssignedDocumentId = checkListUserResult.AssignedDocumentId,
					CheckListChapterId = checkListUserResult.CheckListChapterId,
					IsChecked = checkListUserResult.IsChecked,
					CreatedDate = checkListUserResult.CreatedDate,
					IssueDiscription = checkListUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
		}

	}
}
