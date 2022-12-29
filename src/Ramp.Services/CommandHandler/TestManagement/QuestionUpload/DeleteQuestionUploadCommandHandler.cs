using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;

namespace Ramp.Services.CommandHandler.TestManagement.QuestionUpload
{
    public class DeleteQuestionUploadCommandHandler : ICommandHandlerBase<DeleteQuestionUploadCommand>
    {
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        private readonly IRepository<TrainingQuestion> _questionRepository;
        public DeleteQuestionUploadCommandHandler(IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository,
            IRepository<TrainingQuestion> questionRepository)
        {
            _questionUploadRepository = questionUploadRepository;
            _questionRepository = questionRepository;
        }

        public CommandResponse Execute(DeleteQuestionUploadCommand command)
        {
            var upload = _questionUploadRepository.Find(command.QuestionUploadId);
            var question = _questionRepository.Find(upload.TrainingQuestion.Id);
            if(command.Type == QuestionUploadType.Image)
            {
                question.Image = null;
                _questionRepository.SaveChanges();
            }
            else if(command.Type == QuestionUploadType.Video)
            {
                question.Video = null;
                _questionRepository.SaveChanges();
            }
            _questionUploadRepository.Delete(_questionUploadRepository.Find(command.QuestionUploadId));
            _questionUploadRepository.SaveChanges();
            return null;
        }
    }
}