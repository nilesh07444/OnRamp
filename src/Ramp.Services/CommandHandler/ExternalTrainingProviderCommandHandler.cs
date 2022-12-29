using Common;
using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.ExternalTrainingProvider;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class ExternalTrainingProviderCommandHandler : ICommandHandlerAndValidator<CreateAndUpdateExternalTrainingProviderCommand>,
                                                          ICommandHandlerAndValidator<DeleteByIdCommand<ExternalTrainingProvider>>
    {
        private readonly IRepository<ExternalTrainingProvider> _repository;
        private readonly IRepository<FileUploads> _fileUploadsRepository;
        private readonly IRepository<ExternalTrainingActivityDetail> _externalTrainingActivityRepository;
        private readonly ICommandDispatcher _dispatcher;
        public ExternalTrainingProviderCommandHandler(IRepository<ExternalTrainingProvider> repository,
            IRepository<FileUploads> fileUploadsRepository,
            IRepository<ExternalTrainingActivityDetail> externalTrainingActivityRepository,
            ICommandDispatcher dispatcher)
        {
            _repository = repository;
            _fileUploadsRepository = fileUploadsRepository;
            _externalTrainingActivityRepository = externalTrainingActivityRepository;
            _dispatcher = dispatcher;
        }
        public CommandResponse Execute(CreateAndUpdateExternalTrainingProviderCommand command)
        {
            ExternalTrainingProvider provider = string.IsNullOrWhiteSpace(command.Id) ? new ExternalTrainingProvider { Id = Guid.NewGuid() } : _repository.Find(command.Id.ConvertToGuid());
			if (command.Id != null) {
				provider.Id = Guid.Parse(command.Id);
			}
			provider.Address = command.Address;
            provider.BEEStatusLevel = command.BEEStatusLevel;
            provider.CompanyName = command.CompanyName;
            provider.ContactNumber = command.ContactNumber;
            provider.ContactPerson = command.ContactPerson;
            provider.EmialAddress = command.EmailAddress;
            provider.MobileNumber = command.MobileNumber;
			provider.CertificateUploadId = command.CertificateUploadId;
            if (string.IsNullOrEmpty(command.Id))
                _repository.Add(provider);
            _repository.SaveChanges();
            return null;
        }

        public CommandResponse Execute(DeleteByIdCommand<ExternalTrainingProvider> command)
        {
            var e = _repository.Find(command.Id.ConvertToGuid());
            RemoveReferences(e);
            _repository.Delete(e);
            _repository.SaveChanges();
            return null;

        }
        private void RemoveReferences(ExternalTrainingProvider e)
        {
            _externalTrainingActivityRepository.List.AsQueryable().Where(x => x.ConductedBy.Any(provider => provider.Id == e.Id)).ToList().ForEach(delegate (ExternalTrainingActivityDetail activity)
            {
                activity.ConductedBy.Remove(e);
                _externalTrainingActivityRepository.SaveChanges();
            });
            e.BEECertificates.ToList().ForEach(x => _dispatcher.Dispatch(new DeleteUploadCommand { Id = x.Id.ToString() }));
            _repository.SaveChanges();
        }
        public IEnumerable<IValidationResult> Validate(CreateAndUpdateExternalTrainingProviderCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Id))
            {
                if (_repository.Find(command.Id.ConvertToGuid()) == null)
                    yield return new ValidationResult("Id", $"No item found with id : {command.Id}");
            }
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<ExternalTrainingProvider> command)
        {
           if(_repository.Find(command.Id.ConvertToGuid()) == null)
                yield return new ValidationResult("Id", $"No item found with id : {command.Id}");
        }
    }
}
