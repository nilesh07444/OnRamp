using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.CommandParameter.TrainingActivity;
using Common.Data;
using Domain.Customer.Models;
using Common;
using Common.Query;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.TrainingLabel;
using Ramp.Contracts.QueryParameter.ExternalTrainingProvider;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.CommandParameter.TrainingLabel;
using System.Linq.Expressions;
using Ramp.Services.Projection;
using VirtuaCon;
using Ramp.Contracts.Query.Document;
using Common.Events;
using Ramp.Contracts.Events;
using Data.EF.Customer;

namespace Ramp.Services.CommandHandler
{
    public class TrainingActivityCommandHandler : ICommandHandlerBase<CreateOrUpdateTrainingActivityCommand>,
												  // ICommandHandlerBase<CreateOrUpdateTrainingActivityLogCommand>,
												  ICommandHandlerBase<DeleteByIdCommand<StandardUserTrainingActivityLog>>,
                                                  IEventHandler<LabelDeletedEvent>
    {
        private readonly ITransientRepository<StandardUserTrainingActivityLog> _trainingActivityRepository;
        private readonly ITransientRepository<BursaryTrainingActivityDetail> _bursaryTrainingActivityRepository;
        private readonly ITransientRepository<InternalTrainingActivityDetail> _internalTrainingActivityRepository;
        private readonly ITransientRepository<ToolboxTalkTrainingActivityDetail> _toolboxTalkingTrainingActivityRepository;
        private readonly ITransientRepository<ExternalTrainingActivityDetail> _externalTrainingActivityRepository;
        private readonly ITransientRepository<MentoringAndCoachingTrainingActivityDetail> _mAndCTrainingActivityRepository;
        private readonly ITransientRepository<Upload> _uploadRepository;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IQueryExecutor _executor;

        public TrainingActivityCommandHandler(
            ITransientRepository<StandardUserTrainingActivityLog> trainingActivityRepository,
            ITransientRepository<BursaryTrainingActivityDetail> bursaryTrainingActivityRepository,
            ITransientRepository<InternalTrainingActivityDetail> internalTrainingActivityRepository,
            ITransientRepository<ToolboxTalkTrainingActivityDetail> toolboxTalkingTrainingActivityRepository,
            ITransientRepository<ExternalTrainingActivityDetail> externalTrainingActivityRepository,
            ITransientRepository<MentoringAndCoachingTrainingActivityDetail> mAndCTrainingActivityRepository,
            ITransientRepository<Upload> uploadRepository,
            ICommandDispatcher dispatcher,
            IQueryExecutor executor)
        {
            _trainingActivityRepository = trainingActivityRepository;
            _bursaryTrainingActivityRepository = bursaryTrainingActivityRepository;
            _internalTrainingActivityRepository = internalTrainingActivityRepository;
            _toolboxTalkingTrainingActivityRepository = toolboxTalkingTrainingActivityRepository;
            _externalTrainingActivityRepository = externalTrainingActivityRepository;
            _mAndCTrainingActivityRepository = mAndCTrainingActivityRepository;
            _uploadRepository = uploadRepository;
            _dispatcher = dispatcher;
            _executor = executor;
        }

		#region TrainingActivity

		//public  CommandResponse Execute(CreateOrUpdateTrainingActivityLogCommand command) {
		//	SyncLabels(command);
		//	if (!string.IsNullOrWhiteSpace(command.Id))
		//		Update(command);
		//	else
		//		_trainingActivityRepository.Add(Create(command));
		//	_trainingActivityRepository.SaveChanges();
		//	return null;
		//}


		public CommandResponse Execute(DeleteByIdCommand<StandardUserTrainingActivityLog> command)
        {
            var e = _trainingActivityRepository.Find(command.Id.ConvertToGuid());
            DeleteDocuments(e);
            DeleteUsers(e);
            _trainingActivityRepository.Delete(e);
            _trainingActivityRepository.SaveChanges();
            return null;
        }
        private void DeleteDocuments(StandardUserTrainingActivityLog e)
        {
            e.Documents.ToList().ForEach(x => _dispatcher.Dispatch(new DeleteUploadCommand { Id = x.Id.ToString() }));
            e.Documents.Clear();

            if (e.TrainingActivityType == TrainingActivityType.Bursary)
            {
                e.BursaryTrainingActivityDetail.Invoices.ToList().ForEach(x => _dispatcher.Dispatch(new DeleteUploadCommand { Id = x.Id.ToString() }));
                e.BursaryTrainingActivityDetail.Invoices.Clear();
            }
            if (e.TrainingActivityType == TrainingActivityType.External)
            {
                e.ExternalTrainingActivityDetail.Invoices.ToList().ForEach(x => _dispatcher.Dispatch(new DeleteUploadCommand { Id = x.Id.ToString() }));
                e.ExternalTrainingActivityDetail.Invoices.Clear();
            }
            _trainingActivityRepository.SaveChanges();
        }
        private void DeleteUsers(StandardUserTrainingActivityLog e)
        {
            e.UsersTrained.Clear();
            switch (e.TrainingActivityType)
            {
                case TrainingActivityType.Bursary:
                    e.BursaryTrainingActivityDetail.ConductedBy.Clear();
                    break;
                case TrainingActivityType.External:
                    e.ExternalTrainingActivityDetail.ConductedBy.Clear();
                    break;
                case TrainingActivityType.Internal:
                    e.InternalTrainingActivityDetail.ConductedBy.Clear();
                    break;
                case TrainingActivityType.MentoringAndCoaching:
                    e.MentoringAndCoachingTrainingActivityDetail.ConductedBy.Clear();
                    break;
                case TrainingActivityType.ToolboxTalk:
                    e.ToolboxTalkTrainingActivityDetail.ConductedBy.Clear();
                    break;
            }
            _trainingActivityRepository.SaveChanges();
        }
        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<StandardUserTrainingActivityLog> argument)
        {
            if (_trainingActivityRepository.Find(argument.Id.ConvertToGuid()) == null)
                yield return new ValidationResult("Id", $"No Training Activity Found With Id {argument.Id}");
        }
        public CommandResponse Execute(CreateOrUpdateTrainingActivityCommand command)
        {
            SyncLabels(command);
            if (!string.IsNullOrWhiteSpace(command.Id))
                Update(command);
            else
                _trainingActivityRepository.Add(Create(command));
            _trainingActivityRepository.SaveChanges();
            return null;
        }
        public IEnumerable<IValidationResult> Validate(CreateOrUpdateTrainingActivityCommand argument)
        {

            if (!string.IsNullOrWhiteSpace(argument.Id))
            {
                //update
                if (_trainingActivityRepository.Find(argument.Id.ConvertToGuid()) == null)
                    yield return new ValidationResult("Id", $"No Training Activity found with Id : {argument.Id}");
            }
            if (argument.From > argument.To)
                yield return new ValidationResult("From", "From cannot be greater than To");
            if (argument.TrainingActivityType == TrainingActivityType.Bursary)
                if (argument.BursaryTrainingActivityDetail != null)
                    if (string.IsNullOrWhiteSpace(argument.BursaryTrainingActivityDetail.BursaryType))
                        yield return new ValidationResult("BursaryTrainingActivityDetail.BursaryType", "Bursary Type Is Required");
            //create
            if (!argument.UsersTrained.Any())
            {
                yield return new ValidationResult(nameof(argument.UsersTrained), "One or More Trainees are Required");
            }

            //NOTE: Relies on naming convention
            var detail = argument.GetType().GetProperty($"{argument.TrainingActivityType.ToString()}TrainingActivityDetail").GetValue(argument);
            bool hasConductedBy = detail.GetType().GetProperty("ConductedBy").GetValue(detail).CastTo<IEnumerable<object>>().Any();

            if (!hasConductedBy)
            {
                yield return new ValidationResult("ConductedBy", "One or More Trainers are Required");
            }
        }
        private void Update(CreateOrUpdateTrainingActivityCommand command)
        {
            var e = _trainingActivityRepository.Find(command.Id.ConvertToGuid());
            e.AdditionalInfo = command.AdditionalInfo;
            e.CostImplication = command.CostImplication;
            e.Description = command.Description;
            e.EditedBy = _executor.Execute<FindUserByIdQuery, StandardUser>(new FindUserByIdQuery { Id = command.EditedBy?.Id.ToString() });
            e.From = command.From.Value;
            e.LastEditDate = DateTime.Now;
            e.RewardPoints = command.RewardPoints;
            e.Time = DateTime.Now;
            e.Title = command.Title;
            e.To = command.To.Value;
            e.TrainingActivityType = command.TrainingActivityType;
            e.Venue = command.Venue;
            e.Documents.Clear();
            e.Documents = _executor.Execute<UploadListQuery, IEnumerable<Upload>>(new UploadListQuery { Ids = command.Documents.Select(x => x.Id.ToString()) }).ToList();
            e.UsersTrained.Clear();
            e.UsersTrained = _executor.Execute<UserListQuery, IEnumerable<StandardUser>>(new UserListQuery { Emails = command.UsersTrained.Select(x => x.Email) }).ToList();

            var values = string.IsNullOrWhiteSpace(command.TrainingLabels) ? new string[0] : command.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var labels = _executor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery
            {
                Values = values,
                ExistingModelIds = string.IsNullOrEmpty(e.TrainingLabels) ? Enumerable.Empty<string>() : e.TrainingLabels.Split(',')
            }).ToArray();
            e.TrainingLabels = labels.Any() ? string.Join(",", labels.Select(x => x.Name).ToArray()) : "";

            ExtractDetail(e, command);
            SyncUploadDescriptions(e, command);
        }
        private void SyncLabels(CreateOrUpdateTrainingActivityCommand command)
        {
            var values = string.IsNullOrWhiteSpace(command.TrainingLabels) ? new string[0] : command.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var labels = _executor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery
            {
                Values = values,
                ExistingModelIds = Enumerable.Empty<string>()
            }).ToArray();
        }
        private void  SyncUploadDescriptions(StandardUserTrainingActivityLog e, CreateOrUpdateTrainingActivityCommand command)
        {
            e.Documents.ToList().ForEach(x =>
            {
                UpdateUpload(x, command.Documents.FirstOrDefault(u => u.Id.Equals(x.Id)));
            });
            if (e.TrainingActivityType == TrainingActivityType.External && e.ExternalTrainingActivityDetail != null)
                e.ExternalTrainingActivityDetail.Invoices.ToList().ForEach(x =>
                {
                    UpdateUpload(x, command.ExternalTrainingActivityDetail.Invoices.FirstOrDefault(u => u.Id.Equals(x.Id)));
                });
            if(e.TrainingActivityType == TrainingActivityType.Bursary && e.BursaryTrainingActivityDetail != null)
                e.BursaryTrainingActivityDetail.Invoices.ToList().ForEach(x =>
                {
                    UpdateUpload(x, command.BursaryTrainingActivityDetail.Invoices.FirstOrDefault(u => u.Id.Equals(x.Id)));
                });
            _uploadRepository.SaveChanges();
        }
        private void UpdateUpload(Upload x,UploadResultViewModel model)
        {
            if (model == null || x == null)
                return;
            x.Description = System.IO.Path.GetFileNameWithoutExtension(model.Description);
            var ext = System.IO.Path.GetExtension(x.Name);
            x.Name = x.Description  + ext;
        }
        private StandardUserTrainingActivityLog Create(CreateOrUpdateTrainingActivityCommand command)
        {
            var e = new StandardUserTrainingActivityLog
            {
                Id = Guid.NewGuid(),
                AdditionalInfo = command.AdditionalInfo,
                CostImplication = command.CostImplication,
                Created = DateTime.Now,
                CreatedBy = _executor.Execute<FindUserByIdQuery, StandardUser>(new FindUserByIdQuery { Id = command.CreatedBy?.Id.ToString() }),
                Description = command.Description,
                EditedBy = _executor.Execute<FindUserByIdQuery, StandardUser>(new FindUserByIdQuery { Id = command.EditedBy?.Id.ToString() }),
                From = command.From.Value,
                LastEditDate = DateTime.Now,
                RewardPoints = command.RewardPoints,
                Title = command.Title,
                To = command.To.Value,
                TrainingActivityType = command.TrainingActivityType,
                Venue = command.Venue,
                UsersTrained = _executor.Execute<UserListQuery, IEnumerable<StandardUser>>(new UserListQuery { Emails = command.UsersTrained.Select(x => x.Email) }).ToList(),
                Documents = _executor.Execute<UploadListQuery, IEnumerable<Upload>>(new UploadListQuery { Ids = command.Documents.Select(x => x.Id.ToString()) }).ToList(),
                Time = DateTime.Now
            };
            var values = string.IsNullOrWhiteSpace(command.TrainingLabels) ? new string[0] : command.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var labels = _executor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery
            {
                Values = values,
                ExistingModelIds = string.IsNullOrEmpty(e.TrainingLabels) ? Enumerable.Empty<string>() : e.TrainingLabels.Split(',')
            }).ToArray();
            e.TrainingLabels = labels.Any() ? string.Join(",", labels.Select(x => x.Name).ToArray()) : "";

            ExtractDetail(e, command,true);
            SyncUploadDescriptions(e, command);
            return e;
        }
        private void ExtractDetail(StandardUserTrainingActivityLog entity, CreateOrUpdateTrainingActivityCommand command,bool create = false)
        {
            switch (command.TrainingActivityType)
            {
                case TrainingActivityType.Bursary:
                    if (create)
                        entity.BursaryTrainingActivityDetail = new BursaryTrainingActivityDetail { Id = Guid.NewGuid() };
                    else
                    {
                        entity.BursaryTrainingActivityDetail.BursaryType = command.BursaryTrainingActivityDetail.BursaryType;
                        entity.BursaryTrainingActivityDetail.ConductedBy.Clear();
                        entity.BursaryTrainingActivityDetail.Invoices.Clear();
                    }
                    entity.BursaryTrainingActivityDetail.BursaryType = command.BursaryTrainingActivityDetail.BursaryType;
                    entity.BursaryTrainingActivityDetail.ConductedBy = _executor.Execute<UserListQuery, IEnumerable<StandardUser>>(new UserListQuery { Emails = command.BursaryTrainingActivityDetail.ConductedBy.Select(x => x.Email) }).ToList();
                    entity.BursaryTrainingActivityDetail.Invoices = _executor.Execute<UploadListQuery, IEnumerable<Upload>>(new UploadListQuery { Ids = command.BursaryTrainingActivityDetail.Invoices.Select(x => x.Id.ToString())}).ToList();
                    break;
                case TrainingActivityType.External:
                    if (create)
                        entity.ExternalTrainingActivityDetail = new ExternalTrainingActivityDetail { Id = Guid.NewGuid() };
                    else
                    {
                        entity.ExternalTrainingActivityDetail.ConductedBy.Clear();
                        entity.ExternalTrainingActivityDetail.Invoices.Clear();
                    }
                    entity.ExternalTrainingActivityDetail.ConductedBy = _executor.Execute<ExternalTrainingProviderListQuery, IEnumerable<ExternalTrainingProvider>>(new ExternalTrainingProviderListQuery{ CompanyNames = command.ExternalTrainingActivityDetail.ConductedBy.Select(x => x.CompanyName) }).ToList();
                    entity.ExternalTrainingActivityDetail.Invoices = _executor.Execute<UploadListQuery, IEnumerable<Upload>>(new UploadListQuery { Ids = command.ExternalTrainingActivityDetail.Invoices.Select(x => x.Id.ToString())}).ToList();
                    break;
                case TrainingActivityType.Internal:
                    if (create)
                        entity.InternalTrainingActivityDetail = new InternalTrainingActivityDetail { Id = Guid.NewGuid() };
                    else
                        entity.InternalTrainingActivityDetail.ConductedBy.Clear();
                    entity.InternalTrainingActivityDetail.ConductedBy = _executor.Execute<UserListQuery, IEnumerable<StandardUser>>(new UserListQuery { Emails = command.InternalTrainingActivityDetail.ConductedBy.Select(x => x.Email) }).ToList();
                    break;
                case TrainingActivityType.MentoringAndCoaching:
                    if (create)
                        entity.MentoringAndCoachingTrainingActivityDetail = new MentoringAndCoachingTrainingActivityDetail { Id = Guid.NewGuid() };
                    else
                        entity.MentoringAndCoachingTrainingActivityDetail.ConductedBy.Clear();
                    entity.MentoringAndCoachingTrainingActivityDetail.ConductedBy = _executor.Execute<UserListQuery, IEnumerable<StandardUser>>(new UserListQuery { Emails = command.MentoringAndCoachingTrainingActivityDetail.ConductedBy.Select(x => x.Email) }).ToList();
                    break;
                case TrainingActivityType.ToolboxTalk:
                    if (create)
                        entity.ToolboxTalkTrainingActivityDetail = new ToolboxTalkTrainingActivityDetail { Id = Guid.NewGuid() };
                    else
                        entity.ToolboxTalkTrainingActivityDetail.ConductedBy.Clear();
                    entity.ToolboxTalkTrainingActivityDetail.ConductedBy = _executor.Execute<UserListQuery, IEnumerable<StandardUser>>(new UserListQuery { Emails = command.ToolboxTalkTrainingActivityDetail.ConductedBy.Select(x => x.Email) }).ToList();
                    break;
                default:
                    throw new ArgumentException($"No Activity Type found on Training Activity : {entity.Id}");
            }
        }
        private void ExecuteOnTrainingActivityType(StandardUserTrainingActivityLog e, TrainingActivityType targetType,Action action)
        {
            if (e.TrainingActivityType == targetType)
                action();
        }

        public void Handle(LabelDeletedEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.Name))
            {
                foreach (var x in _trainingActivityRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
                _trainingActivityRepository.SaveChanges();
            }
        }
        #endregion

    }
}
