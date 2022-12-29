using Common.Command;
using Common.Data;
using Common.Events;
using Data.EF.Events;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Handlers
{
    public class CreatedUpdateCustomConfigurationCommandHandler : ICommandHandlerBase<CreatedUpdateCustomConfigurationCommandParameter>, IEventHandler<CreateCustomConfigurationContent>
    {
        private readonly IRepository<CustomConfiguration> _configurationRepository;
        private readonly IRepository<FileUploads> _uploadRepository;

        public CreatedUpdateCustomConfigurationCommandHandler(IRepository<CustomConfiguration> configurationRepository, IRepository<FileUploads> uploadRepository)
        {
            _configurationRepository = configurationRepository;
            _uploadRepository = uploadRepository;
        }

        public CommandResponse Execute(CreatedUpdateCustomConfigurationCommandParameter command)
        {
            if (command.Seach)
            {
                var certC = _uploadRepository.List.Where(obj => obj.Type.Equals("certificate")).FirstOrDefault();
                if (certC != null)
                {
                    _configurationRepository.Add(new CustomConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Type = CustomType.Certificate,
                        Upload = new FileUploads
                        {
                            ContentType = Common.Web.MIMEResolver.GetMIMEType(Path.GetFileName(certC.Name)),
                            Data = certC.Data,
                            Id = Guid.NewGuid(),
                            Name = Path.GetFileName(certC.Name),
                            Description = Path.GetFileName(certC.Name),
                            Type = TrainingDocumentTypeEnum.Certificate.ToString()
                        },
                        Version = _configurationRepository.List.Where(x => x.Type == CustomType.Certificate).Count() + 1
                    });
                    _uploadRepository.Delete(certC);
                }
                return null;
            }
            FileUploads cert = null, css = null;

            if (!string.IsNullOrWhiteSpace(command.CertificatePath))
            {
                if (File.Exists(command.CertificatePath))
                {
                    cert = new FileUploads
                    {
                        ContentType = Common.Web.MIMEResolver.GetMIMEType(Path.GetFileName(command.CertificatePath)),
                        Data = Utility.Convertor.BytesFromFilePath(command.CertificatePath),
                        Id = Guid.NewGuid(),
                        Name = Path.GetFileName(command.CertificatePath),
                        Description = Path.GetFileName(command.CertificatePath),
                        Type = TrainingDocumentTypeEnum.Certificate.ToString()
                    };
                }
            }
            if (command.Cert != null)
            {
                cert = new FileUploads
                {
                    ContentType = Common.Web.MIMEResolver.GetMIMEType(Path.GetFileName(command.Cert.Name)),
                    Data = command.Cert.Data,
                    Id = Guid.NewGuid(),
                    Name = command.Cert.Name,
                    Description = command.Cert.Name,
                    Type = TrainingDocumentTypeEnum.Certificate.ToString()
                };
            }
            if (!string.IsNullOrEmpty(command.CSSPath))
            {
                if (File.Exists(command.CSSPath))
                {
                    css = new FileUploads
                    {
                        ContentType = Common.Web.MIMEResolver.GetMIMEType(Path.GetFileName(command.CSSPath)),
                        Data = Utility.Convertor.BytesFromFilePath(command.CSSPath),
                        Id = Guid.NewGuid(),
                        Name = Path.GetFileName(command.CSSPath),
                        Description = Path.GetFileName(command.CSSPath),
                        Type = TrainingDocumentTypeEnum.CSS.ToString()
                    };
                }
            }
            if (command.CSS != null)
            {
                css = new FileUploads
                {
                    Name = command.CSS.Name,
                    Description = command.CSS.Name,
                    ContentType = Common.Web.MIMEResolver.GetMIMEType(Path.GetFileName(command.CSS.Name)),
                    Data = command.CSS.Data,
                    Id = Guid.NewGuid(),
                    Type = TrainingDocumentTypeEnum.CSS.ToString()
                };
            }
            if (cert != null)
                _configurationRepository.Add(new CustomConfiguration
                {
                    Id = Guid.NewGuid(),
                    Type = CustomType.Certificate,
                    Upload = cert,
                    Version = _configurationRepository.List.Where(x => x.Type == CustomType.Certificate).Count() + 1
                });
            if (css != null)
                _configurationRepository.Add(new CustomConfiguration
                {
                    Id = Guid.NewGuid(),
                    Type = CustomType.CSS,
                    Upload = css,
                    Version = _configurationRepository.List.Where(x => x.Type == CustomType.CSS).Count() + 1
                });
            return null;
        }

        public void Handle(CreateCustomConfigurationContent @event)
        {
            PortalContext.Override(@event.CompanyId);
            new CommandDispatcher().Dispatch(new CreatedUpdateCustomConfigurationCommandParameter
            {
                Seach = true
            });
        }
    }
}