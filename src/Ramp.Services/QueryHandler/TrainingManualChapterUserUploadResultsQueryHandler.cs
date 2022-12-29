using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.TrainingManual;
using LinqKit;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public	class TrainingManualChapterUserUploadResultsQueryHandler : IQueryHandler<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>> {

		private readonly ITransientRepository<TrainingManualChapterUserUploadResult> _TrainingManualChapterUserUploadResultRepository;

		private readonly IRepository<Upload> _uploadRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public TrainingManualChapterUserUploadResultsQueryHandler(ITransientRepository<TrainingManualChapterUserUploadResult> TrainingManualChapterUserUploadResultRepository, IRepository<Upload> uploadRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_uploadRepository = uploadRepository;
			_TrainingManualChapterUserUploadResultRepository = TrainingManualChapterUserUploadResultRepository;
		}

		public List<UploadResultViewModel> ExecuteQuery(TrainingManualChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var TrainingManualUserUploadResult =
					_TrainingManualChapterUserUploadResultRepository.List.Where(x => x.UserId==query.UserId && x.DocumentId == query.DocumentId && x.TrainingManualChapterId == query.TrainingManualChapterId && x.IsGlobalAccessed).Select(c => new TrainingManualChapterUserUploadResult{

						TrainingManualChapterId = c.TrainingManualChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						DocumentId=c.DocumentId,
						UserId=c.UserId,isSignOff=c.isSignOff

					}).ToList();
				foreach (var item in TrainingManualUserUploadResult) {
					if (!string.IsNullOrEmpty(item.UploadId))
					{
						var upload = _uploadRepository.Find(item.UploadId);
						var data = Project.Upload_UploadResultViewModel.Invoke(upload);
						data.isSignOff = item.isSignOff;
						UploadResultViewModelList.Add(data);
					}

				}

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return UploadResultViewModelList;
			} else {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var TrainingManualUserUploadResult =
					_TrainingManualChapterUserUploadResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.TrainingManualChapterId == query.TrainingManualChapterId).Select(c => new TrainingManualChapterUserUploadResult{

						TrainingManualChapterId = c.TrainingManualChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id
,isSignOff=c.isSignOff
					}).ToList();
				foreach (var item in TrainingManualUserUploadResult) {
					if (!string.IsNullOrEmpty(item.UploadId))
					{
						var upload = _uploadRepository.Find(item.UploadId);
						var data = Project.Upload_UploadResultViewModel.Invoke(upload);
						data.isSignOff = item.isSignOff;
						UploadResultViewModelList.Add(data);
					}
				}

				_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

				return UploadResultViewModelList;
			}
		}
	}
}
