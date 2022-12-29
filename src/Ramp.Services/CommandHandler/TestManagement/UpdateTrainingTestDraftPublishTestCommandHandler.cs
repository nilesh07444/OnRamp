using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Linq;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class UpdateTrainingTestDraftPublishTestCommandHandler : CommandHandlerBase<UpdateTrainingTestDraftPublishTestCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;

        public UpdateTrainingTestDraftPublishTestCommandHandler(IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestAssigned> assignedTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _assignedTestRepository = assignedTestRepository;
        }

        public override CommandResponse Execute(UpdateTrainingTestDraftPublishTestCommand command)
        {
            var trainingTestActive = _trainingTestRepository.Find(command.TrainingTestId);
            Guid activeTrainingTestId = command.TrainingTestId;

            var allUsersWhomTestIsAlreadyAssigned =
                _assignedTestRepository.GetAll().Where(t => t.TestId == trainingTestActive.Id).ToList();

            TrainingTest trainingTestInDraftStatus = _trainingTestRepository.GetAll().FirstOrDefault(a => a.ParentTrainingTestId == activeTrainingTestId);

            if (trainingTestInDraftStatus != null)
            {
                foreach (TestAssigned testAssigned in allUsersWhomTestIsAlreadyAssigned)
                {
                    var testAssignedModel = new TestAssigned
                    {
                        Id = Guid.NewGuid(),
                        //GroupId = groupViewModel.GroupId,
                        UserId = testAssigned.UserId,
                        TestId = trainingTestInDraftStatus.Id,
                        AssignedBy = command.CurrentUserId
                    };
                    _assignedTestRepository.Add(testAssignedModel);
                    _assignedTestRepository.SaveChanges();
                }
                trainingTestInDraftStatus.ActiveStatus = true;
                trainingTestInDraftStatus.DraftEditDate = null;
                trainingTestInDraftStatus.ParentTrainingTestId = Guid.Empty;
                trainingTestInDraftStatus.ActivePublishDate = DateTime.Now;
                trainingTestInDraftStatus.DraftStatus = false;
            }
            _trainingTestRepository.SaveChanges();
            _trainingTestRepository.Delete(trainingTestActive);
            _trainingTestRepository.SaveChanges();
            return null;
        }
    }
}