using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.Test;
using Domain.Enums;
using Ramp.Contracts.CommandParameter.Certificate;

namespace Ramp.Services.CommandHandler
{
    public class CertificateCommandHandler : ICommandHandlerAndValidator<CreateOrUpdateCertificateCommand>,
                                             ICommandHandlerAndValidator<RemoveCertificateUploadCommand>,
                                             ICommandHandlerAndValidator<DeleteByIdCommand<Certificate>>
    {
        private readonly IRepository<Certificate> _repository;
        private readonly IRepository<Upload> _uploadRepository;
        private readonly IRepository<Test> _testRepository;

        public CertificateCommandHandler(IRepository<Certificate> repository,
                                         IRepository<Upload> uploadRepository,
                                         IRepository<Test> testRepository)
        {
            _repository = repository;
            _uploadRepository = uploadRepository;
            _testRepository = testRepository;
        }

        public IEnumerable<IValidationResult> Validate(CreateOrUpdateCertificateCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Id))
                if (_repository.Find(command.Id) == null)
                    yield return new ValidationResult(nameof(Certificate.Id), $"No entity with Id : {command.Id} found.");
            if (command.Upload == null)
            {
                yield return new ValidationResult(nameof(Certificate.Upload), "No Upload Attached");
            }
            else
            {
                if (_uploadRepository.Find(command.Upload.Id) == null)
                    yield return new ValidationResult("Upload.Id", $"No upload found with Id : {command.Upload.Id}");
            }
        }

        public CommandResponse Execute(CreateOrUpdateCertificateCommand command)
        {
            var certificate = string.IsNullOrWhiteSpace(command.Id) ?
                new Certificate { Id = Guid.NewGuid().ToString(), CreatedOn = DateTime.Now, Title = command.Title, Type = "Certificate"} :
                _repository.Find(command.Id);

            if (certificate.Upload != null)
            {
                certificate.Upload.Deleted = true;
            }
            certificate.Upload = _uploadRepository.Find(command.Upload.Id);
            certificate.Upload.Type = FileUploadType.Certificate.ToString();
            _uploadRepository.SaveChanges();

            if (string.IsNullOrEmpty(command.Id))
            {
                _repository.Add(certificate);
            }
            _repository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(RemoveCertificateUploadCommand command)
        {
            var e = _repository.Find(command.Id);
            var upload = _uploadRepository.Find(command.UploadId);
            if (e == null)
                yield return new ValidationResult(nameof(Certificate.Id), $"No entity with Id : {command.Id} found.");
            if (upload == null)
                yield return new ValidationResult(nameof(Certificate.UploadId),
                    $"No entity with Id : {command.UploadId} found.");
            if (_repository.List.Count() - 1 <= 0)
                yield return new ValidationResult(nameof(Certificate.UploadId), $"Cannot delete master certificate: {command.Id}.");
            if (_testRepository.List.AsQueryable().Any(t => t.CertificateId == command.Id && !t.Deleted))
                yield return new ValidationResult(nameof(Certificate.UploadId), $"Cannot delete certificate: {command.Id} linked to active Tests.");
        }

        public CommandResponse Execute(RemoveCertificateUploadCommand command)
        {
            var e = _repository.Find(command.Id);
            e.Upload.Deleted = true;
            _repository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<Certificate> command)
        {
            var e = _repository.Find(command.Id);
            if (e == null)
                yield return new ValidationResult(nameof(Certificate.Id), $"No entity with Id : {command.Id} found.");
            else if (e.Upload == null)
                yield return new ValidationResult(nameof(Certificate.UploadId),
                    $"No entity with Id : {e.UploadId} found.");
            if (_repository.List.Count() - 1 <= 0)
                yield return new ValidationResult(nameof(Certificate.UploadId), $"Cannot delete master certificate: {command.Id}.");
            if (_testRepository.List.AsQueryable().Any(t => t.CertificateId == command.Id && !t.Deleted))
                yield return new ValidationResult(nameof(Certificate.UploadId), $"Cannot delete certificate: {command.Id} linked to active Tests.");
        }

        public CommandResponse Execute(DeleteByIdCommand<Certificate> command)
        {
            var e = _repository.Find(command.Id);
            e.Upload.Deleted = true;
            _repository.SaveChanges();
            return null;
        }
    }
}