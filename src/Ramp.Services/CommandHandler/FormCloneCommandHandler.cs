using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Forms;
using Domain.Customer.Models.Test;
using LinqKit;
using Ramp.Contracts.Command.Form;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class FormCloneCommandHandler: ICommandHandlerAndValidator<CloneCommand<Form>>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ITransientRepository<Form> _repository;
        private readonly ITransientRepository<DocumentCategory> _categoryRepository;
        private readonly ITransientRepository<Upload> _uploadRepository;
        private readonly ITransientRepository<Certificate> _certificateRepository;

        public FormCloneCommandHandler(IQueryExecutor queryExecutor,
                                        ICommandDispatcher commandDispatcher,
                                        ITransientRepository<Form> repository,
                                        ITransientRepository<DocumentCategory> categoryRepository,
                                        ITransientRepository<Upload> uploadRepository,
                                        ITransientRepository<Certificate> certificateRepository)
        {
            _queryExecutor = queryExecutor;
            _commandDispatcher = commandDispatcher;
            _repository = repository;
            _categoryRepository = categoryRepository;
            _uploadRepository = uploadRepository;
            _certificateRepository = certificateRepository;
        }

        public CommandResponse Execute(CloneCommand<Form> command)
        {
            if (!string.IsNullOrWhiteSpace(command.SourceCompanyId))
                _repository.SetCustomerCompany(command.SourceCompanyId);
            var e = _repository.Find(command.Id);
            var clonedUploads = e.FormChapters.Where(x => !x.Deleted).Select(x => new { x.Id, x.Uploads }).Select(x => x.Uploads.Clone(x.Id)).ToList();
            clonedUploads.AddRange(e.FormChapters.Where(x => !x.Deleted).Select(x => new { x.Id, x.FormFields, x.ContentToolsUploads }).Select(x => x.ContentToolsUploads.Clone(x.Id, command.SourceCompanyId, command.TargetCompanyId)).ToList());
            if (e.CoverPicture != null)
                clonedUploads.Add(e.CoverPicture.Clone());
            var title = e.Title;
            if (command.SourceCompanyId == command.TargetCompanyId)
            {
                var matchText = e.Title.IndexOf("/") != -1 ? e.Title.Substring(0, e.Title.LastIndexOf("/")) : e.Title;
                var num = _queryExecutor.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery { MatchText = command.NewVersion ? matchText + "/V" : matchText + "/D" }).Count();
                e.Title = num == 0 && command.NewVersion ? $"{e.Title}/V{++num}" : e.Title;
                e.DocumentStatus = command.NewVersion ? Domain.Customer.DocumentStatus.Recalled : e.DocumentStatus;
                title = command.NewVersion ? $"{matchText}/V{++num}" : $"{matchText}/D{++num}";
            }
            _repository.SetCustomerCompany(command.TargetCompanyId);
            _certificateRepository.SetCustomerCompany(command.TargetCompanyId);
            clonedUploads.SelectMany(x => x.Uploads).ToList().ForEach(u => _uploadRepository.Add(u.Upload));
            _repository.SaveChanges();
            var cloneCommand = Clone(e, clonedUploads, title);
           
            _commandDispatcher.Dispatch(cloneCommand);
            command.Id = cloneCommand.Id;
            _repository.SetCustomerCompany(command.SourceCompanyId);
            return null;
        }

        private CreateOrUpdateFormCommand Clone(Form e, IList<UploadContentCloneMapping> uploadCloneMappings, string title)
        {
            var command = new CreateOrUpdateFormCommand
            {
                Id = Guid.NewGuid().ToString(),
                DocumentStatus = Domain.Customer.DocumentStatus.Draft,
                Deleted = false,
                Description = string.IsNullOrEmpty(e.Description) ? "Enter description" : e.Description,
                Points = e.Points,
                Title = title,                
            };
            var targetCategory = _categoryRepository.List.AsQueryable().First(x => x.Title == "Default");  
           
            foreach (var c in e.FormChapters.Where(x => !x.Deleted).OrderBy(x => x.Number).ToArray())
            {
                var contentToolUploads = uploadCloneMappings.FirstOrDefault(x => x.Type == UploadCloneMappingType.Html && x.Id == c.Id);

                var question = new TestQuestionModel
                {
                    Id = Guid.NewGuid().ToString(),                   
                    Deleted = false
                };               
            }
            return command;
        }

        public IEnumerable<IValidationResult> Validate(CloneCommand<Form> command)
        {
            return Enumerable.Empty<IValidationResult>();
        }
    }
}
