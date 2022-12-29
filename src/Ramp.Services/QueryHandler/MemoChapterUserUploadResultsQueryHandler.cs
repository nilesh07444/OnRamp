using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Memo;
using LinqKit;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public	class MemoChapterUserUploadResultsQueryHandler : IQueryHandler<MemoChapterUserResultQuery, List<UploadResultViewModel>> {

		private readonly ITransientRepository<MemoChapterUserUploadResult> _MemoChapterUserUploadResultRepository;

		private readonly IRepository<Upload> _uploadRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public MemoChapterUserUploadResultsQueryHandler(ITransientRepository<MemoChapterUserUploadResult> MemoChapterUserUploadResultRepository, IRepository<Upload> uploadRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_uploadRepository = uploadRepository;
			_MemoChapterUserUploadResultRepository = MemoChapterUserUploadResultRepository;
		}

		public List<UploadResultViewModel> ExecuteQuery(MemoChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var MemoUserUploadResult =
					_MemoChapterUserUploadResultRepository.List.Where(x => x.UserId==query.UserId && x.DocumentId == query.DocumentId && x.MemoChapterId == query.MemoChapterId && x.IsGlobalAccessed).Select(c => new MemoChapterUserUploadResult{

						MemoChapterId = c.MemoChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						DocumentId=c.DocumentId,
						UserId=c.UserId,
						isSignOff = c.isSignOff

					}).ToList();
				foreach (var item in MemoUserUploadResult) {
					var upload = _uploadRepository.Find(item.UploadId);
					var data = Project.Upload_UploadResultViewModel.Invoke(upload);
					data.isSignOff = item.isSignOff;
					UploadResultViewModelList.Add(data);

				}

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return UploadResultViewModelList;
			} else {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var MemoUserUploadResult =
					_MemoChapterUserUploadResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.MemoChapterId == query.MemoChapterId).Select(c => new MemoChapterUserUploadResult{

						MemoChapterId = c.MemoChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						isSignOff = c.isSignOff

					}).ToList();
				foreach (var item in MemoUserUploadResult) {
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
