using Domain.Customer.Models.Forms;
using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Test;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Common;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer;
using Domain.Customer.Models.Restore;
using Common.Data;
using Domain.Customer.Models.Document;
using ikvm.extensions;
using Domain.Enums;
namespace Ramp.Services.CommandHandler
{
    public class FormCommandHandler: ICommandHandlerAndValidator<DeleteByIdCommand<Form>>,
        ICommandHandlerAndValidator<DeleteByIdCommand<FormChapter>>
    {
        private readonly ITransientRepository<Form> _repository;
        private readonly ITransientRepository<FormChapter> _formChapterRepository;

        public FormCommandHandler(
            IQueryExecutor queryExecutor,
            ITransientRepository<Form> repository,
            ITransientRepository<FormChapter> formChapterRepository
            )
        {
            _repository = repository;
            _formChapterRepository = formChapterRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<Form> command)
        {
            if (_repository.Find(command.Id) == null)
                yield return new ValidationResult("Id", $"Cannot find Test with id : {command.Id}");
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<FormChapter> command)
        {
            if (_formChapterRepository.Find(command.Id) == null)
                yield return new ValidationResult("Id", $"Cannot find TestQuestion with id : {command.Id}");
        }
        public CommandResponse Execute(DeleteByIdCommand<FormChapter> command)
        {
            var q = _formChapterRepository.Find(command.Id);           
            q.Deleted = true;
            _formChapterRepository.SaveChanges();            
            return null;
        }
        public CommandResponse Execute(DeleteByIdCommand<Form> command)
        {
            var e = _repository.Find(command.Id);
            e.Deleted = true;
            e.LastEditDate = DateTime.UtcNow;
            e.CategoryId = null;
            e.Category = null;
            
            _repository.SaveChanges();
            return null;
        }
    }
}
