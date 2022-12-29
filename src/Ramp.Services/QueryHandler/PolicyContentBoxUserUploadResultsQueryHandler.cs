using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Policy;
using LinqKit;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler
{
    public class PolicyContentBoxUserUploadResultsQueryHandler : IQueryHandler<PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>
    {
        private readonly ITransientRepository<PolicyContentBoxUserUploadResult> _PolicyContentBoxUserUploadResultRepository;

        private readonly IRepository<Upload> _uploadRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public PolicyContentBoxUserUploadResultsQueryHandler(ITransientRepository<PolicyContentBoxUserUploadResult> PolicyContentBoxUserUploadResultRepository, IRepository<Upload> uploadRepository,
            ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _uploadRepository = uploadRepository;
            _PolicyContentBoxUserUploadResultRepository = PolicyContentBoxUserUploadResultRepository;
        }


        public List<UploadResultViewModel> ExecuteQuery(PolicyContentBoxUserResultQuery query)
        {

            if (query.IsGlobalAccessed)
            {
                var UploadResultViewModelList = new List<UploadResultViewModel>();
                var PolicyContentUserUploadResult =
                    _PolicyContentBoxUserUploadResultRepository.List.Where(x => x.UserId == query.UserId && x.DocumentId == query.DocumentId && x.PolicyContentBoxId == query.PolicyContentBoxId && x.IsGlobalAccessed).Select(c => new PolicyContentBoxUserUploadResult
                    {

                        PolicyContentBoxId = c.PolicyContentBoxId,
                        UploadId = c.UploadId,
                        AssignedDocumentId = c.AssignedDocumentId,
                        Id = c.Id,
                        DocumentId = c.DocumentId,
                        UserId = c.UserId,
                        isSignOff = c.isSignOff

                    }).ToList();
                foreach (var item in PolicyContentUserUploadResult)
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
            }
            else
            {
                var UploadResultViewModelList = new List<UploadResultViewModel>();
                var PolicyContentUserUploadResult =
                    _PolicyContentBoxUserUploadResultRepository.List.Where(x => x.AssignedDocumentId == query.AssignedDocumentId && x.PolicyContentBoxId == query.PolicyContentBoxId).Select(c => new PolicyContentBoxUserUploadResult
                    {

                        PolicyContentBoxId = c.PolicyContentBoxId,
                        UploadId = c.UploadId,
                        AssignedDocumentId = c.AssignedDocumentId,
                        Id = c.Id
,
                        isSignOff = c.isSignOff
                    }).ToList();
                foreach (var item in PolicyContentUserUploadResult)
                {
                    if (!string.IsNullOrEmpty(item.UploadId))
                    {
                        var upload = _uploadRepository.Find(item.UploadId);
                        var data = Project.Upload_UploadResultViewModel.Invoke(upload);
                        data.isSignOff = item.isSignOff;
                        //data.CreatedDate = item.CreatedDate;
                        UploadResultViewModelList.Add(data);
                    }
                }

                _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

                return UploadResultViewModelList;
            }
        }

    }
}
