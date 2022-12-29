using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Icon;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class IconCommandHandler : ICommandHandlerBase<SaveIconSetCommand>,
                                      IValidator<SaveIconSetCommand>,
                                      ICommandHandlerBase<DeleteIconSetCommand>,
                                      IValidator<DeleteIconSetCommand>
    {
        private readonly IRepository<IconSet> _iconSetRepository;
        private readonly IRepository<Icon> _iconRepository;
        private readonly IRepository<FileUpload> _mainUploadRespository;
        public IconCommandHandler(IRepository<IconSet> iconSetRepository, IRepository<Icon> iconRepository, IRepository<FileUpload> mainUploadRespository)
        {
            _iconRepository = iconRepository;
            _iconSetRepository = iconSetRepository;
            _mainUploadRespository = mainUploadRespository;
        }
        public CommandResponse Execute(SaveIconSetCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Id))
            {
                Guid Id;

                if (!Guid.TryParse(command.Id, out Id))
                    return null;
                updateSet(_iconSetRepository.Find(Id), command);
            }
            else
                createSet(command);

            return null;
        }

        public IEnumerable<IValidationResult> Validate(SaveIconSetCommand argument)
        {
            if (string.IsNullOrWhiteSpace(argument.Name))
                yield return new ValidationResult { MemberName = "Name", Message = "No Name Specified" };
        }
        private void updateSet(IconSet set, SaveIconSetCommand command)
        {
            var existingIcons = command.Icons.Where(x => !string.IsNullOrWhiteSpace(x.Id));
            var newIcons = command.Icons.Where(x => string.IsNullOrWhiteSpace(x.Id));
            set.Name = command.Name;
            foreach (var icon in existingIcons)
            {
                Guid gId;
                if (!Guid.TryParse(icon.Id, out gId))
                    continue;
                var newUploadId = icon.UploadModel?.Id;
                var previousUpload = _iconRepository.Find(gId);
                if (newUploadId == Guid.Empty && previousUpload.Id != Guid.Empty)
                    _iconRepository.Delete(previousUpload);
                else
                {
                    if (newUploadId != previousUpload.Upload?.Id)
                    {
                        var uentity = previousUpload.Upload;
                        previousUpload.Upload = null;
                        _iconRepository.SaveChanges();
                        _mainUploadRespository.Delete(uentity);
                    }
                    previousUpload.Upload = _mainUploadRespository.Find(newUploadId);
                    _iconRepository.SaveChanges();
                }
            }
            foreach(var icon in newIcons.Where(x => x.UploadModel != null && x.UploadModel.Id != Guid.Empty))
            {
                set.Icons.Add(new Icon
                {
                    IconType = icon.IconType,
                    Id = Guid.NewGuid(),
                    Upload = _mainUploadRespository.Find(icon.UploadModel.Id)
                });
            }
            _mainUploadRespository.SaveChanges();
            _iconRepository.SaveChanges();

        }
        private void createSet(SaveIconSetCommand command)
        {
            var set = new IconSet
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
            };
            foreach(var icon in command.Icons.Where(x => x.UploadModel != null && x.UploadModel.Id != Guid.Empty))
            {
                set.Icons.Add(new Icon
                {
                    IconType = icon.IconType,
                    Id = Guid.NewGuid(),
                    Upload = _mainUploadRespository.Find(icon.UploadModel?.Id)
                });
            }
            _iconSetRepository.Add(set);
            _iconSetRepository.SaveChanges();
        }

        public CommandResponse Execute(DeleteIconSetCommand command)
        {
            Guid id;
            if (!Guid.TryParse(command.Id, out id))
                return null;
            var set = _iconSetRepository.Find(id);
            set.Deleted = true;
            _iconSetRepository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(DeleteIconSetCommand argument)
        {
            Guid id;
            if (!Guid.TryParse(argument.Id, out id))
                yield return new ValidationResult("Icon Set not found");
            if (_iconSetRepository.Find(id) == null)
                yield return new ValidationResult("Icon Set not found");
        }
    }
}
