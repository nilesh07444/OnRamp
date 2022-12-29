using Common;
using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Enums;
using Ramp.Contracts.CommandParameter.Certificate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class BEECertificateCommandHandler : ICommandHandlerAndValidator<CreateOrUpdateBEECertificateCommand>,
                                                ICommandHandlerAndValidator<DeleteByIdCommand<BEECertificate>>,
                                                ICommandHandlerAndValidator<RemoveUploadFromCertificateCommand>
    {
        private readonly IRepository<ExternalTrainingProvider> _providerRepository;
        private readonly IRepository<BEECertificate> _repository;
        private readonly IRepository<FileUploads> _uploadRepository;
        public BEECertificateCommandHandler(IRepository<ExternalTrainingProvider> providerRepository,
                                            IRepository<BEECertificate> repository,
                                            IRepository<FileUploads> uploadRepository)
        {
            _providerRepository = providerRepository;
            _repository = repository;
            _uploadRepository = uploadRepository;
        }
        public CommandResponse Execute(CreateOrUpdateBEECertificateCommand command)
        {
            BEECertificate certificate = null;
            if (string.IsNullOrWhiteSpace(command.Id))
                certificate = new BEECertificate { Id = Guid.NewGuid() };
            else { certificate = _repository.Find(command.Id.ConvertToGuid()); }
            certificate.Year = command.Year;
            certificate.Upload = _uploadRepository.Find(command.Upload.Id);
            certificate.Upload.Type = FileUploadType.BEECertificate.ToString();
            _uploadRepository.SaveChanges();
            
            var provider = _providerRepository.Find(command.ExternalTrainingProviderId.ConvertToGuid());
            if (string.IsNullOrEmpty(command.Id))
            {
                provider.BEECertificates.Add(certificate);
                _providerRepository.SaveChanges();
            }
            _repository.SaveChanges();
            return null;
        }

        public CommandResponse Execute(DeleteByIdCommand<BEECertificate> command)
        {
            var e = _repository.Find(command.Id.ConvertToGuid());
            var provider = _providerRepository.List.AsQueryable().FirstOrDefault(x => x.BEECertificates.Any(y => y.Id == e.Id));
            if (provider.BEECertificates.Remove(e))
            {
                _providerRepository.SaveChanges();
                _repository.Delete(e);
                _repository.SaveChanges();
            }
            return null;
        }

        public CommandResponse Execute(RemoveUploadFromCertificateCommand command)
        {
            var e = _repository.Find(command.Id.ConvertToGuid());
            e.Upload = null;
            _repository.SaveChanges();
            var u = _uploadRepository.Find(command.UploadId.ConvertToGuid());
            _uploadRepository.Delete(u);
            _uploadRepository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(CreateOrUpdateBEECertificateCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Id))
                if (_repository.Find(command.Id.ConvertToGuid()) == null)
                    yield return new ValidationResult("Id", $"No entity with Id : {command.Id} found.");
            if (_providerRepository.Find(command.ExternalTrainingProviderId.ConvertToGuid()) == null)
                yield return new ValidationResult("ExternalTrainingProviderId", $"No external training provider found with id : {command.ExternalTrainingProviderId}");
            else if (_providerRepository.Find(command.ExternalTrainingProviderId.ConvertToGuid()).BEECertificates.Any(x => x.Year == command.Year && x.Id != command.Id.ConvertToGuid()))
                yield return new ValidationResult("Year", $"Year : {command.Year} already has a certificate uploaded");
            if (command.Upload == null)
                yield return new ValidationResult("Upload", "No Upload Attached");
            if (command.Upload != null)
                if (_uploadRepository.Find(command.Upload.Id) == null)
                    yield return new ValidationResult("Upload.Id", $"No upload found with Id : {command.Upload.Id}");
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<BEECertificate> command)
        {
            var e = _repository.Find(command.Id.ConvertToGuid());
            if ( e == null)
                yield return new ValidationResult("Id", $"No entity with Id : {command.Id} found.");
            else if(_providerRepository.List.AsQueryable().FirstOrDefault(x => x.BEECertificates.Any(y => y.Id == e.Id)) == null)
                yield return new ValidationResult("ExternalProvider", $"No provider found with certificate Id : {command.Id}");
        }

        public IEnumerable<IValidationResult> Validate(RemoveUploadFromCertificateCommand command)
        {
            var e = _repository.Find(command.Id.ConvertToGuid());
            var upload = _uploadRepository.Find(command.UploadId.ConvertToGuid());
            if (e == null)
                yield return new ValidationResult("Id", $"No entity with Id : {command.Id} found.");
            if (upload == null)
                yield return new ValidationResult("UploadId", $"No entity with Id : {command.UploadId} found.");
        }
    }
}
