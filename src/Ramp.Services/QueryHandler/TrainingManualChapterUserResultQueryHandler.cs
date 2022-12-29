using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public class TrainingManualChapterUserResultQueryHandler : IQueryHandler<TrainingManualChapterUserResultQuery, TrainingManualChapterUserResultViewModel> {
		private readonly ITransientRepository<TrainingManualChapterUserResult> _TrainingManualChapterUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public TrainingManualChapterUserResultQueryHandler(ITransientRepository<TrainingManualChapterUserResult> TrainingManualChapterUserResultRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_TrainingManualChapterUserResultRepository = TrainingManualChapterUserResultRepository;
		}

		public TrainingManualChapterUserResultViewModel ExecuteQuery(TrainingManualChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var TrainingManualUserResult =
				_TrainingManualChapterUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.TrainingManualChapterId == query.TrainingManualChapterId && x.UserId==query.UserId).FirstOrDefault();
				if (TrainingManualUserResult == null) return null;
				var result = new TrainingManualChapterUserResultViewModel {
					AssignedDocumentId = TrainingManualUserResult.AssignedDocumentId,
					TrainingManualChapterId = TrainingManualUserResult.TrainingManualChapterId,
					IsChecked = TrainingManualUserResult.IsChecked,
					CreatedDate = TrainingManualUserResult.CreatedDate,
					IssueDiscription = TrainingManualUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			} else {
				var TrainingManualUserResult =
				_TrainingManualChapterUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.TrainingManualChapterId == query.TrainingManualChapterId).FirstOrDefault();
				if (TrainingManualUserResult == null) return null;
				var result = new TrainingManualChapterUserResultViewModel {
					AssignedDocumentId = TrainingManualUserResult.AssignedDocumentId,
					TrainingManualChapterId = TrainingManualUserResult.TrainingManualChapterId,
					IsChecked = TrainingManualUserResult.IsChecked,
					CreatedDate = TrainingManualUserResult.CreatedDate,
					IssueDiscription = TrainingManualUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
		}

	}
}
