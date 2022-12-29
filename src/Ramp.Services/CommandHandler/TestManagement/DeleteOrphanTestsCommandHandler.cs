using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class DeleteOrphanTestsCommandHandler : ICommandHandlerBase<DeleteOrphanTests>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public DeleteOrphanTestsCommandHandler(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public CommandResponse Execute(DeleteOrphanTests command)
        {
            var allTestIds = _trainingTestRepository.List.Select(t => t.Id).ToList();
            var allChildrenIds = _trainingTestRepository.List.Where(x => !x.ParentTrainingTestId.Equals(Guid.Empty) || (x.DraftEditDate.HasValue && !x.ActiveStatus)).Select(t => new { Id = t.Id, ParentId = t.ParentTrainingTestId }).ToList();
            var allOrphansIds = allChildrenIds.Where(x => !allTestIds.Contains(x.ParentId)).ToList();
            foreach (var orphan in allOrphansIds)
            {
                var test = _trainingTestRepository.Find(orphan.Id);
                if (test != null)
                {
                    _trainingTestRepository.Delete(test);
                }
            }

            return null;
        }
    }
}