using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.FileUploads;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.FileUploads
{
    public class FileUploadQueryHandler : QueryHandlerBase<FileUploadsQueryParameter, FileUploadViewModel>,
                                          IQueryHandler<FetchByIdQuery,Domain.Customer.Models.FileUploads>
    {
        private readonly IRepository<Domain.Customer.Models.FileUploads> _fileUploadsRepository;

        public FileUploadQueryHandler(IRepository<Domain.Customer.Models.FileUploads> fileUploadsRepository)
        {
            _fileUploadsRepository = fileUploadsRepository;
        }

        public override FileUploadViewModel ExecuteQuery(FileUploadsQueryParameter queryParameters)
        {
            var fileUploadViewModel = new FileUploadViewModel();
            var fileUpload = _fileUploadsRepository.List.AsQueryable().First(obj => obj.Type == queryParameters.Type);

            if (fileUpload != null)
            {
                fileUploadViewModel.ContentType = fileUpload.ContentType;
                fileUploadViewModel.Data = fileUpload.Data;
                fileUploadViewModel.Name = fileUpload.Name;
                fileUploadViewModel.Description = fileUpload.Description;
            }

            return fileUploadViewModel;
        }

        public Domain.Customer.Models.FileUploads ExecuteQuery(FetchByIdQuery query)
        {
            if (query.Id != null)
                return _fileUploadsRepository.Find(query.Id.ToString().ConvertToGuid());
            return null;
        }
    }
}