using Common;
using Common.Command;
using Common.Data;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Upload;
using System;
using System.Linq;

namespace Ramp.Services.CommandHandler.Uploads {
	public class UploadCommandHandler : ICommandHandlerBase<SaveUploadCommand>, ICommandHandlerBase<UpdateUploadCommand>, ICommandHandlerBase<UploadLogTrainingCommand>,

		ICommandHandlerBase<SaveFileUploadCommand> {
		private readonly ITransientRepository<Upload> _customerUploadRepository;
		private readonly IRepository<FileUpload> _mainUploadRepository;
		private readonly IRepository<Upload> _uploadRepository;
		public UploadCommandHandler(
			ITransientRepository<Upload> customerUploadRepository, IRepository<Upload> uploadRepository,
			IRepository<FileUpload> mainUploadRepository) {
			_customerUploadRepository = customerUploadRepository;
			_mainUploadRepository = mainUploadRepository;
			_uploadRepository = uploadRepository;
		}

		public CommandResponse Execute(UploadLogTrainingCommand command) {

			var upload = new Upload {
				Id = command.Model.Id,
				ContentType = command.Model.ContentType,
				Data = command.Model.Data,
				Name = command.Model.Name,
				Description = command.Model.Description ?? command.Model.Name,
				Type = command.Model.Type
			};
			_uploadRepository.Add(upload);
			_mainUploadRepository.SaveChanges();

			return null;
		}

		public CommandResponse Execute(UpdateUploadCommand command) {
			if (command.MainContext.HasValue && command.MainContext.Value) {
				var upload = _mainUploadRepository.Find(command.Model.Id.ConvertToGuid());
				if (upload == null)
					throw new Exception($"No found");
				upload.MIMEType = command.Model.ContentType;
				upload.Data = command.Model.Data;
				upload.Name = command.Model.Name;
				upload.FileType = command.Model.FileUploadType.HasValue ? command.Model.FileUploadType.Value : Domain.Enums.FileUploadType.Other;
				_mainUploadRepository.SaveChanges();
			} else {
				var upload = _customerUploadRepository.List.AsQueryable().FirstOrDefault(x => x.Id == command.Model.Id);
				if (upload == null)
					throw new Exception($"No found");
				upload.ContentType = command.Model.ContentType;
				upload.Data = command.Model.Data;
				upload.Name = command.Model.Name;
				upload.Description = command.Model.Description ?? command.Model.Name;
				upload.Type = command.Model.Type;
				_customerUploadRepository.SaveChanges();
			}
			return null;
		}

		public CommandResponse Execute(SaveUploadCommand command) {
			if (command.MainContext.HasValue && command.MainContext.Value) {
				_mainUploadRepository.Add(new FileUpload {
					MIMEType = command.FileUploadV.ContentType,
					Name = command.FileUploadV.Name,
					Data = command.FileUploadV.Data,
					Id = command.FileUploadV.Id.ConvertToGuid().Value,
					FileType = command.FileUploadV.FileUploadType.HasValue ? command.FileUploadV.FileUploadType.Value : Domain.Enums.FileUploadType.Other
				});
				_mainUploadRepository.SaveChanges();
			} else {
				_customerUploadRepository.Add(new Upload {
					ContentType = command.FileUploadV.ContentType,
					Name = command.FileUploadV.Name,
					Description = command.FileUploadV.Description ?? command.FileUploadV.Name,
					Data = command.FileUploadV.Data,
					Id = command.FileUploadV.Id,
					Type = command.FileUploadV.Type
				});
				_customerUploadRepository.SaveChanges();
			}
			return null;
		}

		public CommandResponse Execute(SaveFileUploadCommand command) {
			throw new NotImplementedException();			
		}
	}
}