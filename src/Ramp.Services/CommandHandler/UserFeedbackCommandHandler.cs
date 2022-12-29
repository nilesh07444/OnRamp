using Common.Command;
using Ramp.Contracts.Command.UserFeedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Data;
using Common.Events;
using Common.Query;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using Ramp.Contracts.Events.Feedback;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Contracts.Events.Account;

namespace Ramp.Services.CommandHandler
{
    public class UserFeedbackCommandHandler : ICommandHandlerAndValidator<CreateOrUpdateUserFeedbackCommand>,IEventHandler<StandardUserDeletedEvent>
    {
        private readonly IRepository<UserFeedback> _userFeedbackRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IQueryExecutor _queryExecutor;

        public UserFeedbackCommandHandler(
            IRepository<UserFeedback> userFeedbackRepository,
            IRepository<StandardUser> standardUserRepository,
            IQueryExecutor queryExecutor)
        {
            _userFeedbackRepository = userFeedbackRepository;
            _standardUserRepository = standardUserRepository;
            _queryExecutor = queryExecutor;
        }

        public CommandResponse Execute(CreateOrUpdateUserFeedbackCommand command)
        {
            var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            var userFeedback = new UserFeedback
            {
                Id = Guid.NewGuid().ToString(),
                Content = command.Content,
                Type = command.Type,
                ContentType = command.ContentType,
                Created = DateTime.Now,
                CreatedById = userId,
                DocumentType = command.DocumentType,
                DocumentId = command.DocumentId,
                UserFeedbackReads = new List<UserFeedbackRead>()
            };

            _userFeedbackRepository.Add(userFeedback);
            _userFeedbackRepository.SaveChanges();

            var standardUser = _standardUserRepository.Find((Guid)userId.ConvertToGuid());

            //new EventPublisher().Publish(new UserFeedbackCreatedEvent
            //{
            //    Id = userFeedback.Id,
            //    UserFeedbackViewModel = new UserFeedbackViewModel
            //    {
            //        UserFeedback = userFeedback,
            //        UserViewModel = Project.UserViewModelFrom(standardUser),
            //        DocumentListModel = _queryExecutor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery
            //        {
            //            Id = command.DocumentId,
            //            DocumentType = command.DocumentType
            //        })
            //    }
            //});

            return null;
        }

        public void Handle(StandardUserDeletedEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.Id))
            {
                _userFeedbackRepository.List.AsQueryable().Where(x => x.CreatedById == @event.Id).ToList().ForEach(x => _userFeedbackRepository.Delete(x));
                _userFeedbackRepository.SaveChanges();
            }
        }

        public IEnumerable<IValidationResult> Validate(CreateOrUpdateUserFeedbackCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Content))
                yield return new ValidationResult(nameof(command.Content), "Content is required");
            if (string.IsNullOrWhiteSpace(command.DocumentId))
                yield return new ValidationResult(nameof(command.DocumentId), "DocumentId is required");
        }
    }
}
