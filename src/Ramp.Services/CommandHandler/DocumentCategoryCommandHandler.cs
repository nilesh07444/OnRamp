using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Ramp.Contracts.Command.DocumentCategory;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Ramp.Services.CommandHandler
{
    public class DocumentCategoryCommandHandler : ICommandHandlerAndValidator<DocumentCategoryUpdateParentCommand>,
        ICommandHandlerAndValidator<DocumentUpdateCategoryCommand>,
        ICommandHandlerAndValidator<CreateDocumentCategoryCommand>,
        ICommandHandlerAndValidator<RenameDocumentCategoryCommand>,
        ICommandHandlerAndValidator<DeleteDocumentCategoryCommand>
    {
        private readonly IRepository<DocumentCategory> _categoryRepository;
        private readonly IRepository<Memo> _memoRepository;
         private readonly IRepository<AcrobatField> _acrobatFieldRepository;
       private readonly IRepository<Policy> _policyRepository;
        private readonly IRepository<Test> _testRepository;
        private readonly IRepository<TrainingManual> _trainingManualRepository;
        private readonly IRepository<CheckList> _checkListRepository;
    private readonly IRepository<Domain.Customer.Models.CustomDocument> _customDocumentRepository;

        public DocumentCategoryCommandHandler(IRepository<DocumentCategory> categoryRepository,
            IRepository<Memo> memoRepository,
           IRepository<AcrobatField> acrobatFieldRepository,
           IRepository<Policy> policyRepository,
            IRepository<Test> testRepository,
            IRepository<TrainingManual> trainingManualRepository,
			IRepository<CheckList> checkListRepository,
            IRepository<Domain.Customer.Models.CustomDocument> customDocumentRepository
            )
        {
            _categoryRepository = categoryRepository;
            _acrobatFieldRepository = acrobatFieldRepository;
            _policyRepository = policyRepository;
            _testRepository = testRepository;
            _trainingManualRepository = trainingManualRepository;
			_checkListRepository = checkListRepository;
            _customDocumentRepository = customDocumentRepository;
		}

        public IEnumerable<IValidationResult> Validate(DocumentCategoryUpdateParentCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Id))
                yield return new ValidationResult(nameof(command.Id), "Id is required");
            else if (_categoryRepository.Find(command.Id) == null)
                yield return new ValidationResult(nameof(command.Id),
                    $"Category with id: {command.Id} does not exist.");
            if (command.ParentId != null && _categoryRepository.Find(command.ParentId) == null)
                yield return new ValidationResult(nameof(command.ParentId),
                    $"Parent category with id: {command.ParentId}, does not exist.");
        }

        public CommandResponse Execute(DocumentCategoryUpdateParentCommand command)
        {
            var category = _categoryRepository.Find(command.Id);
            category.ParentId = command.ParentId;
            _categoryRepository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(DocumentUpdateCategoryCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Id))
                yield return new ValidationResult(nameof(command.Id), "Id is required");
            if (string.IsNullOrWhiteSpace(command.CategoryId))
                yield return new ValidationResult(nameof(command.CategoryId), "CategoryId is required");
            else if (_categoryRepository.Find(command.CategoryId) == null)
                yield return new ValidationResult(nameof(command.CategoryId),
                    $"Category with id: {command.CategoryId}, does not exist");
        }

        public CommandResponse Execute(DocumentUpdateCategoryCommand command)
        {
            var category = _categoryRepository.Find(command.CategoryId);
            switch (command.DocumentType)
            {
                case DocumentType.Memo:
                    var memo = _memoRepository.Find(command.Id);
                    if (memo == null)
                        return new CommandResponse
                        {
                            ErrorMessage = $"Memo with id: {command.Id}, does not exist"
                        };
                    memo.Category = category;
                    _memoRepository.SaveChanges();
                    break;
                case DocumentType.AcrobatField:
                    var acrobatField = _acrobatFieldRepository.Find(command.Id);
                    if (acrobatField == null)
                        return new CommandResponse
                        {
                            ErrorMessage = $"Acrobat Field with id: {command.Id}, does not exist"
                        };
                    acrobatField.Category = category;
                    _acrobatFieldRepository.SaveChanges();
                    break;
                case DocumentType.Policy:
                    var policy = _policyRepository.Find(command.Id);
                    if (policy == null)
                        return new CommandResponse
                        {
                            ErrorMessage = $"Policy with id: {command.Id}, does not exist"
                        };
                    policy.Category = category;
                    _policyRepository.SaveChanges();
                    break;
                case DocumentType.Test:
                    var test = _testRepository.Find(command.Id);
                    if (test == null)
                        return new CommandResponse
                        {
                            ErrorMessage = $"Test with id: {command.Id}, does not exist"
                        };
                    test.Category = category;
                    _testRepository.SaveChanges();
                    break;
                case DocumentType.TrainingManual:
                    var trainingManual = _trainingManualRepository.Find(command.Id);
                    if (trainingManual == null)
                        return new CommandResponse
                        {
                            ErrorMessage = $"Training Manual with id: {command.Id}, does not exist"
                        };
                    trainingManual.Category = category;
                    _trainingManualRepository.SaveChanges();
                    break;
				case DocumentType.Checklist:
                    var checklist = _checkListRepository.Find(command.Id);
                    if (checklist == null)
                        return new CommandResponse
                        {
                            ErrorMessage = $"Checklist with id: {command.Id}, does not exist"
                        };
					checklist.Category = category;
					_checkListRepository.SaveChanges();
                    break;
                case DocumentType.custom:
                    var customDocument = _customDocumentRepository.Find(command.Id);
                    if (customDocument == null)
                        return new CommandResponse
                        {
                            ErrorMessage = $"Custom Document Field with id: {command.Id}, does not exist"
                        };
                    customDocument.Category = category;
                    _customDocumentRepository.SaveChanges();
                    break;
            }

            return null;
        }

        public IEnumerable<IValidationResult> Validate(CreateDocumentCategoryCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Id))
                yield return new ValidationResult(nameof(command.Id), "Id is required");
            if (command.ParentId != null && _categoryRepository.Find(command.ParentId) == null)
                yield return new ValidationResult(nameof(command.ParentId),
                    $"No parent category with id: {command.ParentId}, exists");
            if (string.IsNullOrWhiteSpace(command.Title))
                yield return new ValidationResult(nameof(command.Title), "Title is required");
        }

        public CommandResponse Execute(CreateDocumentCategoryCommand command)
        {
            var category = new DocumentCategory
            {
                Id = command.Id,
                ParentId = command.ParentId,
                Title = command.Title
            };
            _categoryRepository.Add(category);
            _categoryRepository.SaveChanges();

            return null;
        }

        public IEnumerable<IValidationResult> Validate(RenameDocumentCategoryCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Id))
                yield return new ValidationResult(nameof(command.Id), "Id is required");
            else if (_categoryRepository.Find(command.Id) == null)
                yield return new ValidationResult(nameof(command.Id), $"No category with id: {command.Id}, exists");
            if (string.IsNullOrWhiteSpace(command.Title))
                yield return new ValidationResult(nameof(command.Title), "Title is required");
            else
            {
                var category = _categoryRepository.List.FirstOrDefault(c => c.Title == command.Title);
                if (category != null && category.Id != command.Id)
                    yield return new ValidationResult(nameof(command.Title),
                        $"Category with title: {command.Title} already exists");
            }
        }

        public CommandResponse Execute(RenameDocumentCategoryCommand command)
        {
            var category = _categoryRepository.Find(command.Id);
            category.Title = command.Title;
            _categoryRepository.SaveChanges();

            return null;
        }

        public IEnumerable<IValidationResult> Validate(DeleteDocumentCategoryCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Id))
                yield return new ValidationResult(nameof(command.Id), "Id is required");
            else if (_categoryRepository.Find(command.Id) == null)
                yield return new ValidationResult(nameof(command.Id), $"No category with id: {command.Id}, exists");
        }

        public CommandResponse Execute(DeleteDocumentCategoryCommand command)
        {
            // recursively get all descendants
            var allCategories = _categoryRepository.GetAll().ToList();
            var parent = _categoryRepository.Find(command.Id);
            var descendants = GetCategoryDescendants(command.Id, allCategories);
            var categories = (new List<DocumentCategory> {parent})
                .Concat(descendants).Reverse().ToList();

            // check if any has documents assigned to category
            var queryExecutor = DependencyResolver.Current.GetService<IQueryExecutor>();
            var documents =
                queryExecutor.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery());

            if (documents.Select(d => d.Category.Id).Intersect(categories.Select(c => c.Id)).Any())
            {
                return new CommandResponse()
                {
                    ErrorMessage = "Categories that contain documents cannot be deleted"
                };
            }

            // if not delete all
            foreach (var category in categories)
            {
                _categoryRepository.Delete(category);
            }

            _categoryRepository.SaveChanges();

            return null;
        }

        private IEnumerable<DocumentCategory> GetCategoryDescendants(string parentId,
            IEnumerable<DocumentCategory> collection)
        {
            var children = collection.Where(c => c.ParentId == parentId).ToList();
            var descendants = new List<DocumentCategory>();
            descendants.AddRange(children);

            foreach (var child in children)
            {
                descendants.AddRange(GetCategoryDescendants(child.Id, collection));
            }

            return descendants;
        }
    }
}