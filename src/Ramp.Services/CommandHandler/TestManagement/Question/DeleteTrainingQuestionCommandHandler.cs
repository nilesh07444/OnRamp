using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.TestManagement.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.Answer;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;

namespace Ramp.Services.CommandHandler.TestManagement.Question
{
    public class DeleteTrainingQuestionCommandHandler : ICommandHandlerBase<DeleteTrainingQuestionCommand>
    {
        private readonly IRepository<TrainingQuestion> _questionRepository;
        private readonly ICommandDispatcher _dispatcher;
        public DeleteTrainingQuestionCommandHandler(IRepository<TrainingQuestion> questionRepository,
            ICommandDispatcher dispatcher)
        {
            _questionRepository = questionRepository;
            _dispatcher = dispatcher;
        }
        public CommandResponse Execute(DeleteTrainingQuestionCommand command)
        {
            var q = _questionRepository.Find(command.Model.TrainingTestQuestionId);
            if (q.Image != null)
                _dispatcher.Dispatch(new DeleteQuestionUploadCommand { QuestionUploadId = q.Image.Id, Type = QuestionUploadType.Image });

            if (q.Video != null)
                _dispatcher.Dispatch(new DeleteQuestionUploadCommand { QuestionUploadId = q.Video.Id, Type = QuestionUploadType.Video });
            q.Image = null;
            q.Video = null;
            _questionRepository.SaveChanges();
            foreach(var a in command.Model.TestAnswerList)
            {
                _dispatcher.Dispatch(new DeleteTestAnswerCommand { Model = a});
            }
            _questionRepository.SaveChanges();
            _questionRepository.Delete(_questionRepository.Find(command.Model.TrainingTestQuestionId));
            return null;
        }
    }
}
