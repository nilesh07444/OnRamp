using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Common;
using Common.Command;
using Common.Data;
using Common.Events;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Events;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;

namespace Ramp.Services.CommandHandler
{
    public class DocumentCommandHandler : ICommandHandlerBase<PostProcessDocumentListModelCommand>,
                                          ICommandHandlerBase<CopyDocumentsToAnotherCustomerCommand>,
                                          ICommandHandlerBase<UpdateDocumentCollaboratorsCommand>,
                                          IEventHandler<LabelDeletedEvent>
    {
        private readonly ICommandDispatcher _commandDispatcher;
        readonly ITransientRepository<StandardUser> _userRepository;
        readonly ITransientRepository<DocumentCategory> _categoryRepository;
        readonly ITransientRepository<Upload> _uploadRepository;
        private readonly ITransientRepository<Memo> _memoRepository;
        private readonly ITransientRepository<AcrobatField> _acrobatFieldRepository;
        private readonly ITransientRepository<Policy> _policyRepository;
        private readonly ITransientRepository<Test> _testRepository;
        private readonly ITransientRepository<TrainingManual> _trainingManualRepository;
        private readonly ITransientRepository<CheckList> _checkListRepository;
        private readonly ITransientRepository<Label> _labelRepository;
        private readonly ITransientRepository<CustomDocument> _CustomDocumentRepository;


        public DocumentCommandHandler(
            ICommandDispatcher commandDispatcher,
            ITransientRepository<StandardUser> userRepository,
            ITransientRepository<DocumentCategory> categoryRepository,
            ITransientRepository<Upload> uploadRepository,
            ITransientRepository<Memo> memoRepository,
              ITransientRepository<AcrobatField> acrobatFieldRepository,
           ITransientRepository<Policy> policyRepository,
            ITransientRepository<Test> testRepository,
            ITransientRepository<TrainingManual> trainingManualRepository,
            ITransientRepository<CheckList> checkListRepository,
            ITransientRepository<Label> labelRepository,
            ITransientRepository<CustomDocument> CustomDocumentRepository
            )
        {
            _commandDispatcher = commandDispatcher;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _uploadRepository = uploadRepository;
            _memoRepository = memoRepository;
            _acrobatFieldRepository = acrobatFieldRepository;
            _policyRepository = policyRepository;
            _testRepository = testRepository;
            _trainingManualRepository = trainingManualRepository;
            _checkListRepository = checkListRepository;
            _labelRepository = labelRepository;
            _CustomDocumentRepository = CustomDocumentRepository;


        }
        
        public CommandResponse Execute(PostProcessDocumentListModelCommand command)
        {
            var categorylist = _categoryRepository.Find(command.Model.CategoryId);

            if (command.Model.CreatedByModel == null)
                command.Model.CreatedByModel = FindUser(command.Model.CreatedBy);
            if (command.Model.LastEditedByModel == null)
                command.Model.LastEditedByModel = FindUser(command.Model.LastEditedBy ?? command.Model.CreatedBy);
            if (command.Model.Category == null && categorylist != null)
                command.Model.Category = Project.Category_CategoryViewModelShort.Invoke(_categoryRepository.Find(command.Model.CategoryId));
            if (command.Model.CoverPicture == null)
                command.Model.CoverPicture = command.Model.CoverPictureId == null ? null : Project.Upload_UploadResultViewModel.Invoke(_uploadRepository.Find(command.Model.CoverPictureId));
            if (!string.IsNullOrWhiteSpace(command.Model.TrainingLabels))
            {
                var labels = _labelRepository.List.AsQueryable().Where(x => !x.Deleted).Select(x => x.Name).ToList();
                var filtered = command.Model.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Where(x => labels.Contains(x)).ToArray();
                if (filtered.Any())
                    command.Model.TrainingLabels = string.Join(",", filtered);
                else
                    command.Model.TrainingLabels = "none";
            }

            return null;
        }
        private UserModelShort FindUser(string id)
        {
            var gId = id.ConvertToGuid();
            if (gId.HasValue)
            {
                var user = _userRepository.Find(gId);
                if (user != null)
                    return Project.StandardUserToUserModelShort_Firstname_LastNames.Invoke(user);
            }
            return null;
        }

        public CommandResponse Execute(CopyDocumentsToAnotherCustomerCommand command)
        {
            var responses = new List<CommandResponse>();

            foreach (var memo in command.CopyDocumentsFromCustomerCompanyViewModel.Memos)
            {
                var cm = new CloneCommand<Memo>
                {
                    Id = memo.Id,
                    SourceCompanyId = command.FromCustomerCompanyId,
                    TargetCompanyId = command.ToCustomerCompanyId
                };
                responses.Add(_commandDispatcher.Dispatch(cm));
            }
            foreach (var acrobatField in command.CopyDocumentsFromCustomerCompanyViewModel.AcrobatField)
            {
                var cm = new CloneCommand<AcrobatField>
                {
                    Id = acrobatField.Id,
                    SourceCompanyId = command.FromCustomerCompanyId,
                    TargetCompanyId = command.ToCustomerCompanyId
                };
                responses.Add(_commandDispatcher.Dispatch(cm));
            }

            foreach (var policy in command.CopyDocumentsFromCustomerCompanyViewModel.Policies)
            {
                var cp = new CloneCommand<Policy>
                {
                    Id = policy.Id,
                    SourceCompanyId = command.FromCustomerCompanyId,
                    TargetCompanyId = command.ToCustomerCompanyId
                };
                responses.Add(_commandDispatcher.Dispatch(cp));
            }

            foreach (var test in command.CopyDocumentsFromCustomerCompanyViewModel.Tests)
            {
                var ct = new CloneCommand<Test>
                {
                    Id = test.Id,
                    SourceCompanyId = command.FromCustomerCompanyId,
                    TargetCompanyId = command.ToCustomerCompanyId
                };
                responses.Add(_commandDispatcher.Dispatch(ct));
            }

            foreach (var trainingManual in command.CopyDocumentsFromCustomerCompanyViewModel.TrainingManuals)
            {
                var ctr = new CloneCommand<TrainingManual>
                {
                    Id = trainingManual.Id,
                    SourceCompanyId = command.FromCustomerCompanyId,
                    TargetCompanyId = command.ToCustomerCompanyId
                };
                responses.Add(_commandDispatcher.Dispatch(ctr));
            }

            foreach (var checkList in command.CopyDocumentsFromCustomerCompanyViewModel.CheckLists)
            {
                var ctr = new CloneCommand<CheckList>
                {
                    Id = checkList.Id,
                    SourceCompanyId = command.FromCustomerCompanyId,
                    TargetCompanyId = command.ToCustomerCompanyId
                };
                responses.Add(_commandDispatcher.Dispatch(ctr));
            }
            foreach (var customDocument in command.CopyDocumentsFromCustomerCompanyViewModel.CustomDocument)
            {
                var ctr = new CloneCommand<CustomDocument>
                {
                    Id = customDocument.Id,
                    SourceCompanyId = command.FromCustomerCompanyId,
                    TargetCompanyId = command.ToCustomerCompanyId
                };
                responses.Add(_commandDispatcher.Dispatch(ctr));
            }
            return new CommandResponse
            {
                Validation = responses.SelectMany(r => r.Validation)
            }; ;
        }

        public CommandResponse Execute(UpdateDocumentCollaboratorsCommand command)
        {
            var collaborators = _userRepository.List.Where(u => command.UserIds.Any(id => (Guid)id.ConvertToGuid() == u.Id)).ToList();
            if (command.CurrentUser != null)
            {
                collaborators.Add(_userRepository.Find(command.CurrentUser.ConvertToGuid()));
            }

            switch (command.DocumentType)
            {
                case DocumentType.Memo:
                    var memo = _memoRepository.Find(command.DocumentId);
                    SyncCollaborators(memo, collaborators);
                    _memoRepository.SaveChanges();
                    break;
               //case DocumentType.AcrobatField:
               //     var acrobatField = _acrobatFieldRepository.Find(command.DocumentId);
               //     SyncCollaborators(acrobatField, collaborators);
               //     _acrobatFieldRepository.SaveChanges();
               //     break;
              case DocumentType.Policy:
                    var policy = _policyRepository.Find(command.DocumentId);
                    SyncCollaborators(policy, collaborators);
                    _policyRepository.SaveChanges();
                    break;
                case DocumentType.Test:
                    var test = _testRepository.Find(command.DocumentId);
                    SyncCollaborators(test, collaborators);
                    _testRepository.SaveChanges();
                    break;
                case DocumentType.TrainingManual:
                    var trainingManual = _trainingManualRepository.Find(command.DocumentId);
                    SyncCollaborators(trainingManual, collaborators);
                    _trainingManualRepository.SaveChanges();
                    break;
                case DocumentType.Checklist:
                    var checkList = _checkListRepository.Find(command.DocumentId);
                    SyncCollaborators(checkList, collaborators);
                    _checkListRepository.SaveChanges();
                    break;
                case DocumentType.custom:
                    var CustomDocumentList = _checkListRepository.Find(command.DocumentId);
                    SyncCollaborators(CustomDocumentList, collaborators);
                    _checkListRepository.SaveChanges();
                    break;
            }

            return new CommandResponse();
        }

        public static void SyncCollaborators(IDocument document, IList<StandardUser> collaborators)
        {
            if (document.Collaborators == null)
            {
                document.Collaborators = collaborators.ToList();
                return;
            }
            var current = document.Collaborators;
            var toRemove = current.Except(collaborators).ToList();
            foreach (var standardUser in toRemove)
            {
                document.Collaborators.Remove(standardUser);
            }
            current = document.Collaborators;
            var toAdd = collaborators.Except(current).ToList();
            foreach (var standardUser in toAdd)
            {
                document.Collaborators.Add(standardUser);
            }
        }

        public void Handle(LabelDeletedEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.Name))
            {
                foreach (var x in _memoRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
             foreach (var x in _acrobatFieldRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
              foreach (var x in _policyRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
                foreach (var x in _testRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
                foreach (var x in _trainingManualRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
                foreach (var x in _checkListRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
                foreach (var x in _CustomDocumentRepository.List.AsQueryable().Where(x => x.TrainingLabels.Contains(@event.Name)))
                {
                    x.TrainingLabels = x.TrainingLabels.Replace(@event.Name, string.Empty);
                    x.TrainingLabels = string.IsNullOrWhiteSpace(x.TrainingLabels) ? null : string.Join(",", x.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
                _memoRepository.SaveChanges();
            }
        }
    }
}