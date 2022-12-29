using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using LinqKit;
using Ramp.Contracts.Query.AcrobatField;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public	class AcrobatFieldChapterUserUploadResultsQueryHandler : IQueryHandler<AcrobatFieldChapterUserResultQuery, List<UploadResultViewModel>> {

		private readonly ITransientRepository<AcrobatFieldChapterUserUploadResult> _AcrobatFieldChapterUserUploadResultRepository;

		private readonly IRepository<Upload> _uploadRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public AcrobatFieldChapterUserUploadResultsQueryHandler(ITransientRepository<AcrobatFieldChapterUserUploadResult> AcrobatFieldChapterUserUploadResultRepository, IRepository<Upload> uploadRepository,
			ICommandDispatcher commandDispatcher) {
			_commandDispatcher = commandDispatcher;
			_uploadRepository = uploadRepository;
			_AcrobatFieldChapterUserUploadResultRepository = AcrobatFieldChapterUserUploadResultRepository;
		}

		public List<UploadResultViewModel> ExecuteQuery(AcrobatFieldChapterUserResultQuery query) {

			if (query.IsGlobalAccessed) {
				var UploadResultViewModelList = new List<UploadResultViewModel>();
				var AcrobatFieldUserUploadResult =
					_AcrobatFieldChapterUserUploadResultRepository.List.Where(x => x.UserId==query.UserId && x.DocumentId == query.DocumentId && x.AcrobatFieldChapterId == query.AcrobatFieldChapterId && x.IsGlobalAccessed).Select(c => new AcrobatFieldChapterUserUploadResult{

						AcrobatFieldChapterId = c.AcrobatFieldChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id,
						DocumentId=c.DocumentId,
						UserId=c.UserId,
						isSignOff = c.isSignOff

					}).ToList();
				foreach (var item in AcrobatFieldUserUploadResult)
				{
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
				var AcrobatFieldUserUploadResult =
					_AcrobatFieldChapterUserUploadResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.AcrobatFieldChapterId == query.AcrobatFieldChapterId).Select(c => new AcrobatFieldChapterUserUploadResult{

						AcrobatFieldChapterId = c.AcrobatFieldChapterId,
						UploadId = c.UploadId,
						AssignedDocumentId = c.AssignedDocumentId,
						Id = c.Id, isSignOff=c.isSignOff

					}).ToList();
				foreach (var item in AcrobatFieldUserUploadResult) {
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
