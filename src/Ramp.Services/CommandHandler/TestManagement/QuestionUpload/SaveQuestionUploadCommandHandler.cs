using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;

namespace Ramp.Services.CommandHandler.TestManagement.QuestionUpload
{
    public class SaveQuestionUploadCommandHandler : ICommandHandlerBase<SaveQuestionUploadCommand>
    {
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        private readonly IRepository<TrainingQuestion> _questionRespository;
        private readonly IRepository<FileUploads> _fileUploadRepository;
        public SaveQuestionUploadCommandHandler(IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository,
            IRepository<TrainingQuestion> questionRespository,
            IRepository<FileUploads> fileUploadRepository)
        {
            _questionUploadRepository = questionUploadRepository;
            _questionRespository = questionRespository;
            _fileUploadRepository = fileUploadRepository;
        }
        public CommandResponse Execute(SaveQuestionUploadCommand command)
        {
            var upload = new Domain.Customer.Models.QuestionUpload
            {
                Id = Guid.NewGuid(),
                TrainingQuestion = _questionRespository.Find(command.QuestionId),
                Upload = _fileUploadRepository.Find(command.Model.Id)
            };
            _questionUploadRepository.Add(upload);
            _questionUploadRepository.SaveChanges();
            if (command.Type == QuestionUploadType.Image)
                _questionRespository.Find(command.QuestionId).Image = upload;
            else if (command.Type == QuestionUploadType.Video)
                _questionRespository.Find(command.QuestionId).Video = upload;
            else if (command.Type == QuestionUploadType.Audio)
                _questionRespository.Find(command.QuestionId).Audio = upload;
            _questionRespository.SaveChanges();
            return null;
        }
    }
}
