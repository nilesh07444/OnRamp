using Common.Data;
using Common.Query;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.IO;
using Ramp.Services.Helpers;
using System.Web;
using TikaOnDotNet;
using Domain.Models;
using Ramp.Services.Projection;
using Ramp.Contracts.QueryParameter;
using Domain.Customer.Models;
using Common;
using Data.EF.Customer;
using System.Configuration;
using Common.Command;
using Ramp.Contracts.Query.Upload;

namespace Ramp.Services.QueryHandler
{
    public class UploadQueryHandler :
        IQueryHandler<FetchUploadQueryParameter, UploadModel>,
        IQueryHandler<FetchUploadQueryParameter, FileUploadViewModel>,
        IQueryHandler<FetchUploadFromCompanyQuery, UploadModel>,
        IQueryHandler<GetFileUploadFromPostedFileQuery, UploadModel>,
        IQueryHandler<GetFileUploadFromPostedFileQuery, FileUploadViewModel>,
        IQueryHandler<GetUploadContentQueryParameter, string>,
        IQueryHandler<FileUploadListQuery, IEnumerable<Domain.Customer.Models.FileUploads>>,
        IQueryHandler<UploadListQuery,IEnumerable<Upload>>,
        IQueryHandler<FetchByIdQuery,Upload>
    {
        private readonly ITransientRepository<Upload> _uploadRepository;
        private readonly IRepository<Domain.Customer.Models.FileUploads> _fileUploadsRepository;
        private readonly IRepository<FileUpload> _mainUploadRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public UploadQueryHandler(ITransientRepository<Upload> uploadRepository, IRepository<Domain.Customer.Models.FileUploads> fileUploadsRepository, IRepository<FileUpload> mainUploadRepository, ICommandDispatcher commandDispatcher)
        {
            _uploadRepository = uploadRepository;
            _fileUploadsRepository = fileUploadsRepository;
            _mainUploadRepository = mainUploadRepository;
            _commandDispatcher = commandDispatcher;
        }
        private Upload Find(string id)
        {
            Upload result = null;
            result = _uploadRepository.Find(id);
            if (result == null)
            {
                _uploadRepository.SetCustomerCompany(ConfigurationManager.AppSettings["TemplatePortalId"].ToString());
                result = _uploadRepository.Find(id);
                _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
            }
            return result;
        }
        public string ExecuteQuery(GetUploadContentQueryParameter query)
        {
            var upload = Find(query.Id.ToString());
            if (upload == null)
                return string.Empty;

            //hack to load the correct DLLs
            var t = typeof(com.sun.codemodel.@internal.ClassType); // IKVM.OpenJDK.Tools
            t = typeof(com.sun.org.apache.xalan.@internal.xsltc.trax.TransformerFactoryImpl); // IKVM.OpenJDK.XML.Transform
            t = typeof(com.sun.org.glassfish.external.amx.AMX); // IKVM.OpenJDK.XML.WebServices

            TextExtractor extractor = new TextExtractor();
            TextExtractionResult result = extractor.Extract(upload.Data);

            return result.Text;
        }

        public UploadModel ExecuteQuery(GetFileUploadFromPostedFileQuery query)
        {
            if (query.File == null || query.Id.Equals(Guid.Empty))
                return null;

            var buffer = new byte[query.File.ContentLength];
            query.File.InputStream.Read(buffer, 0, buffer.Length);
            return new UploadModel
            {
                ContentType = query.File.ContentType,
                Id = query.Id.ToString(),
                Name = query.File.FileName.RemoveSpecialCharacters(),
                Type = CommonHelper.GetDocumentType(query.File.FileName).ToString(),
                Data = buffer
            };
        }

        FileUploadViewModel IQueryHandler<GetFileUploadFromPostedFileQuery, FileUploadViewModel>.ExecuteQuery(GetFileUploadFromPostedFileQuery query)
        {
            if (query.File == null || query.Id.Equals(Guid.Empty))
                return null;
            var buffer = new byte[query.File.ContentLength];
            query.File.InputStream.Read(buffer, 0, buffer.Length);
            return new FileUploadViewModel
            {
                ContentType = query.File.ContentType,
                Id = query.Id,
                Name = query.File.FileName.RemoveSpecialCharacters(),
                Type = CommonHelper.GetDocumentType(query.File.FileName).ToString(),
                Data = buffer
            };
        }

        public UploadModel ExecuteQuery(FetchUploadQueryParameter query)
        {
            UploadModel upload = null;
            if (query.MainContext.HasValue && query.MainContext.Value)
            {
                var entity = _mainUploadRepository.Find(query.Id.ConvertToGuid());
                if (entity == null)
                    return null;
                upload = Project.FileUpload_UploadModel.Compile().Invoke(entity);
                if (entity.FileType == Domain.Enums.FileUploadType.Image)
                {
                    using (var ms = new MemoryStream(entity.Data))
                    {
                        var image = System.Drawing.Image.FromStream(ms);
                        upload.Height = image.Height;
                        upload.Width = image.Width;
                    }
                }
                return upload;
            }
            var u = Find(query.Id);
            if (u == null)
                return null;
            if (query.ExcludeBytes)
                upload = Project.Upload_UploadModelWithoutData.Compile().Invoke(u);
            else
            {
                upload = Project.Upload_UploadModel.Compile().Invoke(u);
                if (!string.IsNullOrWhiteSpace(u.ContentType) && u.ContentType.Contains("image"))
                {
                    try
                    {
                        using (var ms = new MemoryStream(u.Data))
                        {
                            var image = System.Drawing.Image.FromStream(ms);
                            upload.Height = image.Height;
                            upload.Width = image.Width;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        upload.Height = 100;
                        upload.Width = 100;
                    }
                }
             }
            return upload;
        }

        public UploadModel ExecuteQuery(FetchUploadFromCompanyQuery query)
        {
            UploadModel upload = null;
            _uploadRepository.SetCustomerCompany(query.CompanyId);
            var u = Find(query.Id);
            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());
            if (u == null)
                return null;
            if (query.ExcludeBytes)
                upload = Project.Upload_UploadModelWithoutData.Compile().Invoke(u);
            else
            {
                upload = Project.Upload_UploadModel.Compile().Invoke(u);
                if (!string.IsNullOrWhiteSpace(u.ContentType) && u.ContentType.Contains("image"))
                {
                    try
                    {
                        using (var ms = new MemoryStream(u.Data))
                        {
                            var image = System.Drawing.Image.FromStream(ms);
                            upload.Height = image.Height;
                            upload.Width = image.Width;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        upload.Height = 100;
                        upload.Width = 100;
                    }
                }
            }
            return upload;
        }

        public IEnumerable<Domain.Customer.Models.FileUploads> ExecuteQuery(FileUploadListQuery query)
        {
            return _fileUploadsRepository.List.AsQueryable().Where(x => query.Ids.Contains(x.Id.ToString()));
        }

        public IEnumerable<Upload> ExecuteQuery(UploadListQuery query)
        {
            return _uploadRepository.List.AsQueryable().Where(x => query.Ids.Contains(x.Id.ToString()));
        }

        public Upload ExecuteQuery(FetchByIdQuery query)
        {
            if (query.Id == null)
                return null;
            return Find(query.Id.ToString());
        }

        FileUploadViewModel IQueryHandler<FetchUploadQueryParameter, FileUploadViewModel>.ExecuteQuery(FetchUploadQueryParameter query)
        {
            FileUploadViewModel upload = null;
            if (query.MainContext.HasValue && query.MainContext.Value)
            {
                var entity = _mainUploadRepository.Find((Guid)query.Id.ConvertToGuid());
                if (entity == null)
                    return null;
                upload = Project.ToUploadModel_Domain.Compile().Invoke(entity);
                if (entity.FileType == Domain.Enums.FileUploadType.Image)
                {
                    using (var ms = new MemoryStream(entity.Data))
                    {
                        var image = System.Drawing.Image.FromStream(ms);
                        upload.Height = image.Height;
                        upload.Width = image.Width;
                    }
                }
                return upload;
            }
            var u = _fileUploadsRepository.Find((Guid)query.Id.ConvertToGuid());
            if (u == null)
                return null;
            if (query.ExcludeBytes)
                upload = Project.ToUploadModelWithoutData.Compile().Invoke(u);
            else
            {
                upload = Project.ToUploadModel.Compile().Invoke(u);
                if (u.ContentType.Contains("image"))
                {
                    try
                    {
                        using (var ms = new MemoryStream(u.Data))
                        {
                            var image = System.Drawing.Image.FromStream(ms);
                            upload.Height = image.Height;
                            upload.Width = image.Width;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        upload.Height = 100;
                        upload.Width = 100;
                    }
                }
            }
            return upload;
        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<Domain.Customer.Models.FileUploads, FileUploadViewModel>> ToUploadModel
            = x => new FileUploadViewModel
            {
                ContentType = x.ContentType,
                Data = x.Data,
                Description = x.Description,
                Id = x.Id,
                Name = x.Name.RemoveSpecialCharacters(),
                Type = x.Type
            };
        public static readonly Expression<Func<Upload, UploadModel>> Upload_UploadModel
            = x => new UploadModel
            {
                ContentType = x.ContentType,
                Data = x.Data,
                Description = x.Description,
                Id = x.Id.ToString(),
                Name = x.Name.RemoveSpecialCharacters(),
                Type = x.Type
            };
        public static readonly Expression<Func<Domain.Customer.Models.FileUploads, FileUploadViewModel>> ToUploadModelWithoutData
            = x => new FileUploadViewModel
            {
                ContentType = x.ContentType,
                Description = x.Description,
                Id = x.Id,
                Name = x.Name.RemoveSpecialCharacters(),
                Type = x.Type
            };
        public static readonly Expression<Func<Upload, UploadModel>> Upload_UploadModelWithoutData
          = x => new UploadModel
          {
              ContentType = x.ContentType,
              Description = x.Description,
              Id = x.Id.ToString(),
              Name = x.Name.RemoveSpecialCharacters(),
              Type = x.Type
          };


    }
}
