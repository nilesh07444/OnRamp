using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Policy;
using LinqKit;
using Ramp.Contracts.Command.Policy;
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
    public class PolicyCloneCommandHandler : ICommandHandlerAndValidator<CloneCommand<Policy>>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ITransientRepository<Policy> _repository;
        private readonly ITransientRepository<DocumentCategory> _categoryRepository;
        private readonly ITransientRepository<Upload> _uploadRepository;
		private readonly ITransientRepository<DocumentUrl> _documentUrlRepository;


		public PolicyCloneCommandHandler(IQueryExecutor queryExecutor,
                                         ICommandDispatcher commandDispatcher,
                                         ITransientRepository<Policy> repository,
                                         ITransientRepository<DocumentCategory> categoryRepository,
                                         ITransientRepository<Upload> uploadRepository, ITransientRepository<DocumentUrl> documentUrlRepository)
        {
            _queryExecutor = queryExecutor;
            _commandDispatcher = commandDispatcher;
            _repository = repository;
            _categoryRepository = categoryRepository;
            _uploadRepository = uploadRepository;
			_documentUrlRepository = documentUrlRepository;
		}
        public CommandResponse Execute(CloneCommand<Policy> command)
        {
            if (!string.IsNullOrWhiteSpace(command.SourceCompanyId))
                _repository.SetCustomerCompany(command.SourceCompanyId);
            var e = _repository.Find(command.Id);
            var clonedUploads = e.ContentBoxes.Where(x => !x.Deleted).Select(x => new { x.Id, x.Uploads }).Select(x => x.Uploads.Clone(x.Id)).ToList();
            clonedUploads.AddRange(e.ContentBoxes.Where(x => !x.Deleted).Select(x => new { x.Id, x.Content, x.ContentToolsUploads }).Select(x => x.ContentToolsUploads.Clone(x.Id, command.SourceCompanyId, command.TargetCompanyId, x.Content)).ToList());
            var coverPicture = e.CoverPicture != null ? Project.Upload_ClonedUpload.Invoke(e.CoverPicture) : null;
            if (e.CoverPicture != null)
                clonedUploads.Add(e.CoverPicture.Clone());
            var title = e.Title;
            if (command.SourceCompanyId == command.TargetCompanyId)
            {
                var matchText =  e.Title.IndexOf("/") != -1 ? e.Title.Substring(0, e.Title.LastIndexOf("/")) : e.Title;
                var num = _queryExecutor.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery { MatchText = command.NewVersion ? matchText + "/V" : matchText + "/D" }).Count();
                e.Title = num == 0 && command.NewVersion ? $"{e.Title}/V{++num}" : e.Title;
                e.DocumentStatus = command.NewVersion ? Domain.Customer.DocumentStatus.Recalled : e.DocumentStatus;
                title = command.NewVersion ? $"{matchText}/V{++num}" : $"{matchText}/D{++num}";
            }
            _repository.SetCustomerCompany(command.TargetCompanyId);
            clonedUploads.SelectMany(x => x.Uploads).ToList().ForEach(u => _uploadRepository.Add(u.Upload));
            _repository.SaveChanges();
            var cloneCommand = Clone(e, clonedUploads, title, command.Id);
            if (command.SourceCompanyId == command.TargetCompanyId)
            {
                cloneCommand.Collaborators = e.Collaborators;
            }
            _commandDispatcher.Dispatch(cloneCommand);
			foreach (var contentModel in cloneCommand.ContentModels) {
				foreach (var docUrl in contentModel.DocLinks) {
					_documentUrlRepository.Add(new DocumentUrl() {
						ChapterId = contentModel.Id,
						DocumentId = cloneCommand.Id,
						Url = docUrl.Url
					});
				}
				_documentUrlRepository.SaveChanges();
			}
			command.Id = cloneCommand.Id;
            _repository.SetCustomerCompany(command.SourceCompanyId);
            return null;
        }
        private CreateOrUpdatePolicyCommand Clone(Policy e, IList<UploadContentCloneMapping> uploadCloneMappings, string title, string oldDocId = "")
        {
            var command = new CreateOrUpdatePolicyCommand
            {
                Id = Guid.NewGuid().ToString(),
                DocumentStatus = Domain.Customer.DocumentStatus.Draft,
                Deleted = false,
                Description = e.Description,
                Points = e.Points,
                PreviewMode = e.PreviewMode,
                Printable = e.Printable,
                Title = title,
                CallToAction = e.CallToAction,
                CallToActionMessage = e.CallToActionMessage,
                TrainingLabels = e.TrainingLabels,
				IsGlobalAccessed = e.IsGlobalAccessed
			};
			var docUrls = new List<DocumentUrlViewModel>();
			if (oldDocId != "") {
				docUrls = _documentUrlRepository.GetAll().Where(x => x.DocumentId == oldDocId).ToList().Select(x => new DocumentUrlViewModel() {
					ChapterId = x.ChapterId,
					DocumentId = x.DocumentId,
					Url = x.Url
				}).ToList();
			}
			var targetCategory = _categoryRepository.List.AsQueryable().First(x => x.Title == "Default");
            //var targetCategory = e.Category;

            command.Category = Project.Category_CategoryViewModelShort.Invoke(targetCategory);
            command.CoverPicture = e.CoverPicture != null ? Project.Upload_UploadResultViewModel.Invoke(uploadCloneMappings.Where(x => x.Type == UploadCloneMappingType.CoverPicture).FirstOrDefault(e.CoverPictureId)) : null;

            foreach (var c in e.ContentBoxes.Where(x => !x.Deleted).OrderBy(x => x.Number).ToArray())
            {
                var contentToolUploads = uploadCloneMappings.FirstOrDefault(x => x.Type == UploadCloneMappingType.Html && x.Id == c.Id);
                var contentBox = new PolicyContentBoxModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = c.Title,
                    Content = contentToolUploads?.Content ?? c.Content,
                    Deleted = false,
                    New = true,
                    Number = c.Number,
                };
				var docLinks = new List<DocumentUrlViewModel>();
				foreach (var docUrl in docUrls) {
					if (docUrl.ChapterId == c.Id) {
						var link = new DocumentUrlViewModel() {
							ChapterId = contentBox.Id,
							DocumentId = command.Id,
							Url = docUrl.Url
						};
						docLinks.Add(link);
					}
				}
				contentBox.DocLinks = docLinks;
				(contentBox.Attachments as List<UploadResultViewModel>).AddRange(c.Uploads.AsQueryable()
                                                                                        .Where(x => !x.Deleted)
                                                                                        .Select(x => Project.Upload_UploadResultViewModel.Invoke(uploadCloneMappings.Where(u => u.Type == UploadCloneMappingType.Normal && u.Id == c.Id).FirstOrDefault(x.Id)))
                                                                                        .OrderBy(x => x.Number));
                if (contentToolUploads != null)
                    (contentBox.ContentToolsUploads as List<UploadFromContentToolsResultModel>).AddRange(contentToolUploads.Uploads.Select(x => Project.Upload_UploadFromContentToolsResultModel.Invoke(x.Upload)).ToList());
                (command.ContentModels as IList<PolicyContentBoxModel>).Add(contentBox);
            }
            return command;
        }
        public IEnumerable<IValidationResult> Validate(CloneCommand<Policy> command)
        {
            return Enumerable.Empty<IValidationResult>();
        }
    }
}
