using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.TrainingManual;
using LinqKit;
using Ramp.Contracts.Command.TrainingManual;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Uploads;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Data;
using Domain.Customer.Models.Document;

namespace Ramp.Services.CommandHandler {
	public class TrainingManualCloneCommandHandler : ICommandHandlerAndValidator<CloneCommand<TrainingManual>> {
		private readonly IQueryExecutor _queryExecutor;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ITransientRepository<TrainingManual> _repository;
		private readonly ITransientRepository<DocumentCategory> _categoryRepository;
		private readonly ITransientRepository<Upload> _uploadRepository;
		private readonly ITransientRepository<DocumentUrl> _documentUrlRepository;

		public TrainingManualCloneCommandHandler(IQueryExecutor queryExecutor,
												 ICommandDispatcher commandDispatcher,
												 ITransientRepository<TrainingManual> repository,
												 ITransientRepository<DocumentCategory> categoryRepository,
												 ITransientRepository<DocumentUrl> documentUrlRepository,
												 ITransientRepository<Upload> uploadRepository) {
			_queryExecutor = queryExecutor;
			_commandDispatcher = commandDispatcher;
			_repository = repository;
			_categoryRepository = categoryRepository;
			_uploadRepository = uploadRepository;
			_documentUrlRepository = documentUrlRepository;
		}
		public CommandResponse Execute(CloneCommand<TrainingManual> command) {
			if (!string.IsNullOrWhiteSpace(command.SourceCompanyId))
				_repository.SetCustomerCompany(command.SourceCompanyId);
			var e = _repository.List.AsQueryable().Include(x => x.Chapters).First(x => x.Id == command.Id);

			var clonedUploads = e.Chapters.Where(x => !x.Deleted).Select(x => new { x.Id, x.Uploads }).Select(x => x.Uploads.Clone(x.Id)).ToList();
			clonedUploads.AddRange(e.Chapters.Where(x => !x.Deleted).Select(x => new { x.Id, x.Content, x.ContentToolsUploads }).Select(x => x.ContentToolsUploads.Clone(x.Id, command.SourceCompanyId, command.TargetCompanyId, x.Content)).ToList());
			if (e.CoverPicture != null)
				clonedUploads.Add(e.CoverPicture.Clone());
			var title = e.Title;
			if (command.SourceCompanyId == command.TargetCompanyId) {
				var matchText = e.Title.IndexOf("/") != -1 ? e.Title.Substring(0, e.Title.LastIndexOf("/")) : e.Title;
				var num = _queryExecutor.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery { MatchText = command.NewVersion ? matchText + "/V" : matchText + "/D" }).Count();
				e.Title = num == 0 && command.NewVersion ? $"{e.Title}/V{++num}" : e.Title;
				e.DocumentStatus = command.NewVersion ? Domain.Customer.DocumentStatus.Recalled : e.DocumentStatus;
				title = command.NewVersion ? $"{matchText}/V{++num}" : $"{matchText}/D{++num}";
			}
			_repository.SetCustomerCompany(command.TargetCompanyId);
			clonedUploads.SelectMany(x => x.Uploads).ToList().ForEach(u => _uploadRepository.Add(u.Upload));
			_repository.SaveChanges();
			var cloneCommand = Clone(e, clonedUploads, title, command.Id);
			if (command.SourceCompanyId == command.TargetCompanyId) {
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
		private CreateOrUpdateTrainingManualCommand Clone(TrainingManual e, IList<UploadContentCloneMapping> uploadCloneMappings, string title, string oldDocId = "") {

			var command = new CreateOrUpdateTrainingManualCommand {
				Id = Guid.NewGuid().ToString(),
				DocumentStatus = Domain.Customer.DocumentStatus.Draft,
				Deleted = false,
				Description = e.Description,
				Points = e.Points,
				PreviewMode = e.PreviewMode,
				Printable = e.Printable,
				Title = title,
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

			foreach (var c in e.Chapters.AsQueryable().Include(x => x.Uploads).Include(x => x.ContentToolsUploads).Where(x => !x.Deleted).OrderBy(x => x.Number).ToList()) {
				var contentToolUploads = uploadCloneMappings.FirstOrDefault(x => x.Type == UploadCloneMappingType.Html && x.Id == c.Id);
				var chapter = new TrainingManualChapterModel {
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
							ChapterId = chapter.Id,
							DocumentId = command.Id,
							Url = docUrl.Url
						};
						docLinks.Add(link);
					}
				}
				chapter.DocLinks = docLinks;
				(chapter.Attachments as List<UploadResultViewModel>).AddRange(c.Uploads.AsQueryable()
																						.Where(x => !x.Deleted)
																						.Select(x => Project.Upload_UploadResultViewModel.Invoke(uploadCloneMappings.Where(u => u.Type == UploadCloneMappingType.Normal && u.Id == c.Id).FirstOrDefault(x.Id)))
																						.OrderBy(x => x.Number));
				if (contentToolUploads != null)
					(chapter.ContentToolsUploads as List<UploadFromContentToolsResultModel>).AddRange(contentToolUploads.Uploads.Select(x => Project.Upload_UploadFromContentToolsResultModel.Invoke(x.Upload)).ToList());
				(command.ContentModels as IList<TrainingManualChapterModel>).Add(chapter);
			}
			return command;

		}
		public IEnumerable<IValidationResult> Validate(CloneCommand<TrainingManual> command) {
			return Enumerable.Empty<IValidationResult>();
		}
	}
}
namespace Ramp.Services.Uploads {
	public static partial class Extensions {
		public static UploadContentCloneMapping Clone(this Upload upload) {
			if (upload.Deleted)
				return new UploadContentCloneMapping();
			var result = new UploadContentCloneMapping {
				Type = UploadCloneMappingType.CoverPicture,
			};
			result.Uploads.Add(new UploadCloneMapping { OriginalId = upload.Id, Upload = Project.Upload_ClonedUpload.Invoke(upload) });
			return result;
		}
		public static UploadContentCloneMapping Clone(this ICollection<Upload> uploads, string id, string sourceCompanyId = null, string targetCompanyId = null, string content = null) {
			var result = new UploadContentCloneMapping() { Id = id };
			result.Type = !string.IsNullOrWhiteSpace(sourceCompanyId) && !string.IsNullOrWhiteSpace(targetCompanyId) && !string.IsNullOrWhiteSpace(content) ?
						  UploadCloneMappingType.Html : !string.IsNullOrWhiteSpace(id) ? UploadCloneMappingType.Normal : UploadCloneMappingType.CoverPicture;
			var temp = content ?? "";
			foreach (var u in uploads.Where(x => x != null && !x.Deleted).ToList()) {
				var clone = Project.Upload_ClonedUpload.Invoke(u);
				if (result.Type == UploadCloneMappingType.Html)
					temp = temp
								.Replace($"/Upload/GetFromCompany/{u.Id}?companyId={sourceCompanyId}", $"/Upload/GetFromCompany/{clone.Id}?companyId={targetCompanyId}")
								.Replace($"/Upload/GetThumbnailFromCompany/{u.Id}?companyId={sourceCompanyId}", $"/Upload/GetThumbnailFromCompany/{clone.Id}?companyId={targetCompanyId}")
								.Replace($"/Upload/Get/{u.Id}", $"/Upload/GetFromCompany/{clone.Id}?companyId={targetCompanyId}")
								.Replace($"/Upload/GetThumbnail/{u.Id}", $"/Upload/GetThumbnailFromCompany/{clone.Id}?companyId={targetCompanyId}");
				result.Uploads.Add(new UploadCloneMapping { OriginalId = u.Id, Upload = clone });
			}
			result.Content = temp;
			return result;
		}
		public static Upload FirstOrDefault(this IEnumerable<UploadContentCloneMapping> entries, string originalId) {
			return entries.SelectMany(x => x.Uploads).FirstOrDefault(x => x.OriginalId == originalId)?.Upload;
		}
	}
}
public class UploadContentCloneMapping : IdentityModel<string> {
	public string Content { get; set; } = string.Empty;
	public UploadCloneMappingType Type { get; set; }
	public IList<UploadCloneMapping> Uploads { get; set; } = new List<UploadCloneMapping>();
}
public enum UploadCloneMappingType {
	Normal,
	Html,
	CoverPicture
}
public class UploadCloneMapping {
	public string OriginalId { get; set; }
	public Upload Upload { get; set; }
}

namespace Ramp.Services.Projection {
	public static partial class Project {
		public static readonly Expression<Func<Upload, Upload>> Upload_ClonedUpload =
			e => new Upload {
				Id = Guid.NewGuid().ToString(),
				Content = e.Content,
				ContentType = e.ContentType,
				Data = e.Data,
				Deleted = e.Deleted,
				Description = e.Description,
				Name = e.Name,
				Order = e.Order,
				Type = e.Type
			};
	}
}
