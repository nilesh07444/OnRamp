using Common;
using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Upload;
using System.Linq;

namespace Ramp.Services.CommandHandler.Uploads {
	public class DeleteUploadCommandHandler : ICommandHandlerBase<DeleteUploadCommand>
    {
        private readonly IRepository<FileUploads> _fileUploadsRepository;
        private readonly IRepository<QuestionUpload> _questionUploadRepository;
        private readonly IRepository<ChapterUpload> _chapterUploadRepository;
        readonly IRepository<Upload> _uploadRepository;
        public DeleteUploadCommandHandler(
            IRepository<FileUploads> fileUploadsRepository,
            IRepository<ChapterUpload> chapterUploadRepository,
            IRepository<QuestionUpload> questionUploadRepository,
            IRepository<Upload> uploadRepository)
        {
            _fileUploadsRepository = fileUploadsRepository;
            _questionUploadRepository = questionUploadRepository;
            _chapterUploadRepository = chapterUploadRepository;
            _uploadRepository = uploadRepository;
        }

        public CommandResponse Execute(DeleteUploadCommand command)
        {
            if (_fileUploadsRepository.Find(command.Id.ConvertToGuid()) != null)
            {
                var chapterUpload = _chapterUploadRepository.List.AsQueryable().SingleOrDefault(u => u.Upload != null && u.Upload.Id.Equals(command.Id));
                var questionUpload = _questionUploadRepository.List.AsQueryable().SingleOrDefault(u => u.Upload != null && u.Upload.Id.Equals(command.Id));
                if (chapterUpload != null)
                {
                    _chapterUploadRepository.Delete(chapterUpload);
                }
                else if (questionUpload != null)
                {
                    _questionUploadRepository.Delete(questionUpload);
                }
                else
                {
                    var upload = _fileUploadsRepository.Find(command.Id.ConvertToGuid());
                    _fileUploadsRepository.Delete(upload);
                }
            }
            if (_uploadRepository.Find(command.Id) != null)
                _uploadRepository.Delete(_uploadRepository.Find(command.Id));
            return null;
        }
    }
}