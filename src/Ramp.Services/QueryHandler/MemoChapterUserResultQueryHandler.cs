using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public class MemoChapterUserResultQueryHandler : IQueryHandler<MemoChapterUserResultQuery, MemoChapterUserResultViewModel> {
		private readonly ITransientRepository<MemoChapterUserResult> _MemoChapterUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public MemoChapterUserResultQueryHandler(ITransientRepository<MemoChapterUserResult> MemoChapterUserResultRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_MemoChapterUserResultRepository = MemoChapterUserResultRepository;
		}

		public MemoChapterUserResultViewModel ExecuteQuery(MemoChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var MemoUserResult =
				_MemoChapterUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.MemoChapterId == query.MemoChapterId && x.UserId==query.UserId).FirstOrDefault();
				if (MemoUserResult == null) return null;
				var result = new MemoChapterUserResultViewModel {
					AssignedDocumentId = MemoUserResult.AssignedDocumentId,
					MemoChapterId = MemoUserResult.MemoChapterId,
					IsChecked = MemoUserResult.IsChecked,
					CreatedDate = MemoUserResult.CreatedDate,
					IssueDiscription = MemoUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			} else {
				var MemoUserResult =
				_MemoChapterUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.MemoChapterId == query.MemoChapterId).FirstOrDefault();
				if (MemoUserResult == null) return null;
				var result = new MemoChapterUserResultViewModel {
					AssignedDocumentId = MemoUserResult.AssignedDocumentId,
					MemoChapterId = MemoUserResult.MemoChapterId,
					IsChecked = MemoUserResult.IsChecked,
					CreatedDate = MemoUserResult.CreatedDate,
					IssueDiscription = MemoUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
		}

	}
}
