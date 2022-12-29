using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using LinqKit;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public	class CheckListChapterUserUploadResultsQueryHandler : IQueryHandler<CheckListChapterUserResultQuery, List<UploadResultViewModel>>
		, IQueryHandler<FetchUserQuery, List<CheckListChapterUserUploadResult>>
	{

		private readonly ITransientRepository<CheckListChapterUserUploadResult> _checkListChapterUserUploadResultRepository;

		private readonly IRepository<Upload> _uploadRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public CheckListChapterUserUploadResultsQueryHandler(ITransientRepository<CheckListChapterUserUploadResult> checkListChapterUserUploadResultRepository, IRepository<Upload> uploadRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_uploadRepository = uploadRepository;
			_checkListChapterUserUploadResultRepository = checkListChapterUserUploadResultRepository;
		}

		public List<UploadResultViewModel> ExecuteQuery(CheckListChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var checkListUserUploadResult =
					_checkListChapterUserUploadResultRepository.List.Where(x => x.UserId==query.UserId && x.DocumentId == query.DocumentId && x.CheckListChapterId == query.CheckListChapterId && x.IsGlobalAccessed).Select(c => new CheckListChapterUserUploadResultsModel {

						CheckListChapterId = c.CheckListChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						DocumentId=c.DocumentId,
						UserId=c.UserId,
						isSignOff = c.isSignOff

					}).ToList();
				foreach (var item in checkListUserUploadResult) {
					var upload = _uploadRepository.Find(item.UploadId);
					var data = Project.Upload_UploadResultViewModel.Invoke(upload);
					data.isSignOff = item.isSignOff;
					UploadResultViewModelList.Add(data);

				}

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return UploadResultViewModelList;
			} else {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var checkListUserUploadResult =
					_checkListChapterUserUploadResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.CheckListChapterId == query.CheckListChapterId).Select(c => new CheckListChapterUserUploadResultsModel {

						CheckListChapterId = c.CheckListChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						isSignOff = c.isSignOff
					}).ToList();
				foreach (var item in checkListUserUploadResult) {
					var upload = _uploadRepository.Find(item.UploadId);
					var data = Project.Upload_UploadResultViewModel.Invoke(upload);
					data.isSignOff = item.isSignOff;
					UploadResultViewModelList.Add(data);

				}

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return UploadResultViewModelList;
			}
		}

		public List<CheckListChapterUserUploadResult> ExecuteQuery(FetchUserQuery query)
		{
			var UploadResultViewModelList = new List<UploadResultViewModel>();
			var checkListUserUploadResult = _checkListChapterUserUploadResultRepository.List.Where(x => x.CheckListChapterId == query.Id.ToString() && x.UserId == query.UserId.ToString()).ToList();
			return checkListUserUploadResult;
		}
	}
}
