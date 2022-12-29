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
    public class DeleteOrphanTestResultsCommandHandler : ICommandHandlerBase<DeleteOrphanTestResultsCommandParameter>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;

        public DeleteOrphanTestResultsCommandHandler(
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestResult> testResultRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _testResultRepository = testResultRepository;
        }

        public CommandResponse Execute(DeleteOrphanTestResultsCommandParameter command)
        {
            var resultTests = _testResultRepository.List.ToList();
            foreach (var r in resultTests)
            {
                var test = _trainingTestRepository.Find(r.TrainingTestId);
                if (test == null)
                {
                    _testResultRepository.Delete(r);
                }
            }
            return null;
        }
    }
}