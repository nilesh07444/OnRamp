using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.Answer;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement.Answer
{
    public class SaveTestAnswerCommandHandler : ICommandHandlerBase<SaveTestAnswerCommand>
    {
        private readonly IRepository<TestAnswer> _answerRepository;
        public SaveTestAnswerCommandHandler(IRepository<TestAnswer> answerRepository)
        {
            _answerRepository = answerRepository;
        }
        public CommandResponse Execute(SaveTestAnswerCommand command)
        {

            command.Model.TestAnswerId = command.Model.TestAnswerId.Equals(Guid.Empty) ? Guid.NewGuid() : command.Model.TestAnswerId;
            command.Model.TrainingQuestionId = command.Model.TrainingQuestionId.Equals(Guid.Empty) ? command.QuestionId : command.Model.TrainingQuestionId;
            _answerRepository.Add(command.Model.toDomainModel());
            _answerRepository.SaveChanges();
            return null;
        }
    }
}
