using Common.Command;
using Common.Data;
using Common.Events;
using Data.EF.Events;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Handlers
{
    public class CreateDefaultContentCommandHandler : ICommandHandlerBase<CreateDefaultContentCommandParameter>, IEventHandler<CreateDefaultConfigurationContent>
    {
        private readonly IRepository<FileUpload> _uploadRepository;
        private readonly IRepository<DefaultConfiguration> _defaultConfigurationRepository;

        public CreateDefaultContentCommandHandler(
            IRepository<FileUpload> uploadRepository,
            IRepository<DefaultConfiguration> defaultConfigurationRepository)
        {
            _uploadRepository = uploadRepository;
            _defaultConfigurationRepository = defaultConfigurationRepository;
        }

        public CommandResponse Execute(CreateDefaultContentCommandParameter command)
        {
            var currentConfig = _defaultConfigurationRepository.List.FirstOrDefault();
            FileUpload cert = null;
            List<FileUpload> trophys = new List<FileUpload>();

            if (!string.IsNullOrWhiteSpace(command.CertificatePath))
            {
                if (File.Exists(command.CertificatePath))
                {
                    cert = new FileUpload
                    {
                        Data = Utility.Convertor.BytesFromFilePath(command.CertificatePath),
                        FileType = FileUploadType.Certificate,
                        Id = Guid.NewGuid(),
                        MIMEType = Common.Web.MIMEResolver.GetMIMEType(Path.GetFileName(command.CertificatePath)),
                        Name = Path.GetFileName(command.CertificatePath)
                    };
                }
            }
            if (command.TrophyPaths.Count > 0)
            {
                foreach (var t in command.TrophyPaths)
                {
                    if (File.Exists(t))
                    {
                        trophys.Add(new FileUpload
                        {
                            Data = Utility.Convertor.BytesFromFilePath(t),
                            FileType = FileUploadType.Trophy,
                            Id = Guid.NewGuid(),
                            MIMEType = Common.Web.MIMEResolver.GetMIMEType(Path.GetFileName(t)),
                            Name = Path.GetFileName(t)
                        });
                    }
                }
            }

            var defaultC = new DefaultConfiguration { Id = Guid.NewGuid() };
            if (cert != null)
            {
                defaultC.Certificate = cert;
            }
            if (trophys.Count > 0)
            {
                trophys.ForEach(t => defaultC.Trophys.Add(t));
            }
            if (currentConfig != null)
            {
                _defaultConfigurationRepository.Delete(currentConfig);
            }
            _defaultConfigurationRepository.Add(defaultC);
            return null;
        }

        public void Handle(CreateDefaultConfigurationContent @event)
        {
            var certificatePath = HttpContext.Current.Server.MapPath("~/Content/images/Certificate.jpg");
            var trophyDirPath = HttpContext.Current.Server.MapPath("~/Content/TrophyPicDir");
            var trophys = Directory.GetFiles(trophyDirPath);
            Execute(new CreateDefaultContentCommandParameter
            {
                CertificatePath = certificatePath,
                TrophyPaths = new List<string>(trophys)
            });
        }
    }
}