using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public class TestChapterUserResultQueryHandler : IQueryHandler<TestChapterUserResultQuery, TestChapterUserResultViewModel> {
		private readonly ITransientRepository<TestChapterUserResult> _TestChapterUserResultRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public TestChapterUserResultQueryHandler(ITransientRepository<TestChapterUserResult> TestChapterUserResultRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_TestChapterUserResultRepository = TestChapterUserResultRepository;
		}

		public TestChapterUserResultViewModel ExecuteQuery(TestChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var TestUserResult =
				_TestChapterUserResultRepository.List.Where(x => x.DocumentId == query.DocumentId && x.TestChapterId == query.TestChapterId && x.UserId==query.UserId).FirstOrDefault();
				if (TestUserResult == null) return null;
				var result = new TestChapterUserResultViewModel {
					AssignedDocumentId = TestUserResult.AssignedDocumentId,
					TestChapterId = TestUserResult.TestChapterId,
					IsChecked = TestUserResult.IsChecked,
					CreatedDate = TestUserResult.CreatedDate,
					IssueDiscription = TestUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			} else {
				var TestUserResult =
				_TestChapterUserResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.TestChapterId == query.TestChapterId).FirstOrDefault();
				if (TestUserResult == null) return null;
				var result = new TestChapterUserResultViewModel {
					AssignedDocumentId = TestUserResult.AssignedDocumentId,
					TestChapterId = TestUserResult.TestChapterId,
					IsChecked = TestUserResult.IsChecked,
					CreatedDate = TestUserResult.CreatedDate,
					IssueDiscription = TestUserResult.IssueDiscription

				};

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return result;
			}
		}

	}
}
