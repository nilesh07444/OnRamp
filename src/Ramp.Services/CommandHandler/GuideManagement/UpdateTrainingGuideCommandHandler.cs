using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Events.GuideManagement;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class UpdateTrainingGuideCommandHandler :
        ICommandHandlerBase<UpdateTrainingGuideCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public UpdateTrainingGuideCommandHandler(
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
        }

        public CommandResponse Execute(UpdateTrainingGuideCommand command)
        {
            if (command.TrainingGuide != null)
            {
                var trainingGuide = _trainingGuideRepository.Find(command.TrainingGuide.TrainingGuidId);

                new CommandDispatcher().Dispatch(new SaveTrainingGuideCommand
                {
                    CurrentlyLoggedInUserId = command.CurrentlyLoggedInUserId,
                    TrainingGuide = command.TrainingGuide
                });
            }

            return null;
        }
    }
}