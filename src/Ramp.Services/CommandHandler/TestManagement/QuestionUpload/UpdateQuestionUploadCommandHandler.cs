using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;
using Ramp.Contracts.CommandParameter.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement.QuestionUpload
{
    public class UpdateQuestionUploadCommandHandler : ICommandHandlerBase<UpdateQuestionUploadCommand>
    {
        private readonly IRepository<FileUploads> _fileUploadRepository;
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        private readonly ICommandDispatcher _dispatcher;
        public UpdateQuestionUploadCommandHandler(IRepository<FileUploads> fileUploadRepository,
            IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository,
            ICommandDispatcher dispatcher)
        {
            _fileUploadRepository = fileUploadRepository;
            _questionUploadRepository = questionUploadRepository;
            _dispatcher = dispatcher;
        }
        public CommandResponse Execute(UpdateQuestionUploadCommand command)
        {
            var qDomain = _questionUploadRepository.Find(command.QuestionUploadId);
            var oldUpload = qDomain.Upload;
            qDomain.Upload = null;
            _questionUploadRepository.SaveChanges();
            var result = _dispatcher.Dispatch(new DeleteUploadCommand { Id = oldUpload.Id.ToString() });
            if (result.Validation.Any())
                throw new Exception(result.Validation.First().Message);
            var newUpload = _fileUploadRepository.Find(command.Model.Id);
            qDomain.Upload = newUpload;
            _questionUploadRepository.SaveChanges();
            return null;
        }
    }
}
