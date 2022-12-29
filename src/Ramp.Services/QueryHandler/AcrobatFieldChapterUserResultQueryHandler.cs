using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.AcrobatField;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public class AcrobatFieldChapterUserResultQueryHandler : IQueryHandler<AcrobatFieldChapterUserResultQuery, AcrobatFieldChapterUserResultViewModel> {
		private readonly ITransientRepository<AcrobatFieldChapterUserResult> _AcrobatFieldChapterUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public AcrobatFieldChapterUserResultQueryHandler(ITransientRepository<AcrobatFieldChapterUserResult> AcrobatFieldChapterUserResultRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_AcrobatFieldChapterUserResultRepository = AcrobatFieldChapterUserResultRepository;
		}

		public AcrobatFieldChapterUserResultViewModel ExecuteQuery(AcrobatFieldChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var AcrobatFieldUserResult =
				_AcrobatFieldChapterUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.AcrobatFieldChapterId == query.AcrobatFieldChapterId && x.UserId==query.UserId).FirstOrDefault();
				if (AcrobatFieldUserResult == null) return null;
				var result = new AcrobatFieldChapterUserResultViewModel {
					AssignedDocumentId = AcrobatFieldUserResult.AssignedDocumentId,
					AcrobatFieldChapterId = AcrobatFieldUserResult.AcrobatFieldChapterId,
					IsChecked = AcrobatFieldUserResult.IsChecked,
					CreatedDate = AcrobatFieldUserResult.CreatedDate,
					IssueDiscription = AcrobatFieldUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			} else {
				var AcrobatFieldUserResult =
				_AcrobatFieldChapterUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.AcrobatFieldChapterId == query.AcrobatFieldChapterId).FirstOrDefault();
				if (AcrobatFieldUserResult == null) return null;
				var result = new AcrobatFieldChapterUserResultViewModel {
					AssignedDocumentId = AcrobatFieldUserResult.AssignedDocumentId,
					AcrobatFieldChapterId = AcrobatFieldUserResult.AcrobatFieldChapterId,
					IsChecked = AcrobatFieldUserResult.IsChecked,
					CreatedDate = AcrobatFieldUserResult.CreatedDate,
					IssueDiscription = AcrobatFieldUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
		}

	}
}
