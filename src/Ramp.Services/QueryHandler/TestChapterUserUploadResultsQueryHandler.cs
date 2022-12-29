using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Test;
using LinqKit;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public	class TestChapterUserUploadResultsQueryHandler : IQueryHandler<TestChapterUserResultQuery, List<UploadResultViewModel>> {

		private readonly ITransientRepository<TestChapterUserUploadResult> _TestChapterUserUploadResultRepository;

		private readonly IRepository<Upload> _uploadRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public TestChapterUserUploadResultsQueryHandler(ITransientRepository<TestChapterUserUploadResult> TestChapterUserUploadResultRepository, IRepository<Upload> uploadRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_uploadRepository = uploadRepository;
			_TestChapterUserUploadResultRepository = TestChapterUserUploadResultRepository;
		}

		public List<UploadResultViewModel> ExecuteQuery(TestChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var TestUserUploadResult =
					_TestChapterUserUploadResultRepository.List.Where(x => x.UserId==query.UserId && x.DocumentId == query.DocumentId && x.TestChapterId == query.TestChapterId && x.IsGlobalAccessed).Select(c => new TestChapterUserUploadResult{

						TestChapterId = c.TestChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						DocumentId=c.DocumentId,
						UserId=c.UserId,isSignOff=c.isSignOff

					}).ToList();
				foreach (var item in TestUserUploadResult) {
					var upload = _uploadRepository.Find(item.UploadId);
					var data = Project.Upload_UploadResultViewModel.Invoke(upload);
					data.isSignOff = item.isSignOff;
					UploadResultViewModelList.Add(data);

				}

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return UploadResultViewModelList;
			} else {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var TestUserUploadResult =
					_TestChapterUserUploadResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.TestChapterId == query.TestChapterId).Select(c => new TestChapterUserUploadResult{

						TestChapterId = c.TestChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						isSignOff = c.isSignOff

					}).ToList();
				foreach (var item in TestUserUploadResult) {
					var upload = _uploadRepository.Find(item.UploadId);
					var data = Project.Upload_UploadResultViewModel.Invoke(upload);
					data.isSignOff = item.isSignOff;
					UploadResultViewModelList.Add(data);

				}

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return UploadResultViewModelList;
			}
		}
	}
}
