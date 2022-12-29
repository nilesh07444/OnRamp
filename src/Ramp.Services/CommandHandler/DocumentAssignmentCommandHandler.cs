using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Common;
using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.PolicyResponse;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Models;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Command.TestSession;
using Ramp.Contracts.Events.Account;
using Ramp.Contracts.Events.Document;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.TestSession;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using VirtuaCon;

namespace Ramp.Services.CommandHandler
{
	public class DocumentAssignmentCommandHandler : ICommandHandlerBase<AssignDocumentsToUsers>,
		ICommandHandlerBase<UnassignDocumentsFromUsers>,
		ICommandHandlerBase<UnassignDocumentFromAllUsersCommand>,
		IEventHandler<StandardUserDeletedEvent>
	{
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly IRepository<StandardUser> _userRepository;
		private readonly IRepository<CheckList> _checkListRepository;
		private readonly IRepository<Memo> _memoRepository;
		private readonly IRepository<AcrobatField> _acrobatFieldRepository;
		private readonly IRepository<Test> _testRepository;
		private readonly IRepository<Policy> _policyRepository;
		private readonly IRepository<TrainingManual> _trainingManualRepository;
		private readonly ITransientRepository<Test_Result> _testResultRepository;
		private readonly IRepository<Company> _companyRepository;
		private readonly IQueryExecutor _queryExecutor;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ITransientRepository<DocumentAuditTrack> _documentAuditRepository;
		private readonly IRepository<PolicyResponse> _policyResponseRepository;
		private readonly IRepository<DocumentUsage> _documentUsageRepository;
		private readonly IRepository<CustomDocument> _customDocumentRepository;

		public DocumentAssignmentCommandHandler(
			IRepository<DocumentUsage> documentUsageRepository,
			IRepository<AssignedDocument> assignedDocumentRepository,
			IRepository<CheckList> checkListRepository,
			IRepository<Memo> memoRepository,
			IRepository<AcrobatField> acrobatFieldRepository,
			IRepository<Test> testRepository,
			IRepository<Policy> policyRepository,
			IRepository<TrainingManual> trainingManualRepository,
			IRepository<StandardUser> userRepository,
			ITransientRepository<Test_Result> testResultRepository,
			ITransientRepository<DocumentAuditTrack> documentAuditRepository,
			IRepository<Company> companyRepository, IRepository<CustomDocument> customDocumentRepository,
			IRepository<PolicyResponse> policyResponseRepository,
			IQueryExecutor queryExecutor,
			ICommandDispatcher commandDispatcher)
		{
			_documentUsageRepository = documentUsageRepository;
			_assignedDocumentRepository = assignedDocumentRepository;
			_userRepository = userRepository;
			_companyRepository = companyRepository;
			_queryExecutor = queryExecutor;
			_testResultRepository = testResultRepository;
			_commandDispatcher = commandDispatcher;
			_policyRepository = policyRepository;
			_testRepository = testRepository;
			_memoRepository = memoRepository;
			_acrobatFieldRepository = acrobatFieldRepository;
			_trainingManualRepository = trainingManualRepository;
			_checkListRepository = checkListRepository;
			_documentAuditRepository = documentAuditRepository;
			_policyResponseRepository = policyResponseRepository;
			_documentUsageRepository = documentUsageRepository;
			_customDocumentRepository = customDocumentRepository;
		}

		public CommandResponse Execute(AssignDocumentsToUsers command)
		{
			var response = new CommandResponse();

			if (command.IsReassigned)
			{
				foreach (var d in command.AssignmentViewModels)
				{
					var entity = _documentUsageRepository.List.Where(x => x.DocumentId == d.DocumentId && x.UserId == d.UserId).OrderByDescending(x => x.ViewDate).ToList();

					if (entity != null && entity.Count > 0)
					{
						foreach (var x in entity)
						{
							_documentUsageRepository.Delete(x);
						}
						_documentUsageRepository.SaveChanges();
					}
				}
			}
			var assignVmdata = command.AssignmentViewModels.FirstOrDefault();

			var message = assignVmdata != null ? assignVmdata.AdditionalMsg : null;

			var additionalMessage = "";

			var usersDocuments = command.AssignmentViewModels.GroupBy(a => a.UserId,
				a => new { a.DocumentId, a.DocumentType, a.OrderNumber }, (key, g) => new { UserId = key, Documents = g.ToList() });

			var AssignTodayDateList = new List<DateTime>();

			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

			var userDetails = _userRepository.Find(userId.ConvertToGuid());


			foreach (var user in usersDocuments)
			{

				var standardUser = _userRepository.Find(user.UserId.ConvertToGuid());

				var company = _companyRepository.Find(command.CompanyId);

				foreach (var document in user.Documents)
				{
					var additionalMsg = "";
					if (!string.IsNullOrEmpty(message))
					{
						var additionalMsgList = command.AssignmentViewModels.FirstOrDefault().AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
						additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == document.DocumentId).Msg;
					}

					//this one for Recurring
					if (!string.IsNullOrEmpty(command.MultipleAssignedDates))
					{
						var multipleDates = command.MultipleAssignedDates.Split(',');
						foreach (var item in multipleDates)
						{
							AssignTodayDateList.Add(Convert.ToDateTime(item));
							_assignedDocumentRepository.Add(new AssignedDocument
							{
								Id = Guid.NewGuid().ToString(),
								AssignedBy = command.AssignedBy,
								AssignedDate = Convert.ToDateTime(item).ToUniversalTime(),
								DocumentId = document.DocumentId,
								DocumentType = document.DocumentType,
								UserId = user.UserId,
								IsRecurring = true,
								AdditionalMsg = additionalMsg,
								OrderNumber = document.OrderNumber,

							});
						}

						_assignedDocumentRepository.SaveChanges();

						#region This one for sending email 
						var documentTitles = _queryExecutor.Execute<DocumentTitlesQuery, IEnumerable<DocumentTitlesAndTypeQuery>>(
				new DocumentTitlesQuery
				{
					Identifiers = user.Documents.Select(x =>
						new DocumentIdentifier { DocumentId = x.DocumentId, DocumentType = x.DocumentType, AdditionalMsg = additionalMsg }).GroupBy(c => c.DocumentId).Select(f => f.FirstOrDefault())
				}).ToList();

						foreach (var item in AssignTodayDateList.Where(c => c.Date == DateTime.UtcNow.Date))
						{

							new EventPublisher().Publish(new DocumentsAssignedEvent
							{
								UserViewModel = Project.UserViewModelFrom(standardUser),
								CompanyViewModel = Project.CompanyViewModelFrom(company),
								DocumentTitles = documentTitles,
								Subject = $"{documentTitles.Count()} Document{(documentTitles.Count() > 1 ? "s" : "")}{(command.IsReassigned ? "Reassigned" : "Assigned")}"
							});

						}

						#endregion

						#region keep the document audit track history
						var title = GetDocumentTitle(document.DocumentType, document.DocumentId);
						var documentAudit = new DocumentAuditTrack
						{
							Id = Guid.NewGuid(),
							LastEditDate = DateTime.UtcNow,
							LastEditedBy = userDetails.Id.ToString(),
							User = userDetails,
							DocumentId = document.DocumentId,
							DocumentName = title,
							DocumentType = document.DocumentType.ToString(),
							UserName = userDetails.LastName + " " + userDetails.FirstName,
						};

						documentAudit.DocumentStatus = "Document Assigned";

						_documentAuditRepository.Add(documentAudit);
						_documentAuditRepository.SaveChanges();
						#endregion

					}
					//this one for Schedule and Today
					else
					{

						_assignedDocumentRepository.Add(new AssignedDocument
						{
							Id = Guid.NewGuid().ToString(),
							AssignedBy = command.AssignedBy,
							AssignedDate = command.AssignedDate == null ? DateTime.UtcNow : command.AssignedDate.Value,
							DocumentId = document.DocumentId,
							DocumentType = document.DocumentType,
							UserId = user.UserId,
							IsRecurring = false,
							AdditionalMsg = additionalMsg,
							OrderNumber = document.OrderNumber


						});

						_assignedDocumentRepository.SaveChanges();

						#region keep the document audit track history
						var title = GetDocumentTitle(document.DocumentType, document.DocumentId);
						var documentAudit = new DocumentAuditTrack
						{
							Id = Guid.NewGuid(),
							LastEditDate = DateTime.UtcNow,
							LastEditedBy = userDetails.Id.ToString(),
							User = userDetails,
							DocumentId = document.DocumentId,
							DocumentName = title,
							DocumentType = document.DocumentType.ToString(),
							UserName = userDetails.LastName + " " + userDetails.FirstName,
						};

						documentAudit.DocumentStatus = "Document Assigned";

						_documentAuditRepository.Add(documentAudit);
						_documentAuditRepository.SaveChanges();
						#endregion
					}
				}

				#region This one for sending email 
				if (command.AssignedDate != null && command.AssignedDate.Value.Date == DateTime.UtcNow.Date)
				{

					if (!string.IsNullOrEmpty(message))
					{
						var additionalMsgList = command.AssignmentViewModels.FirstOrDefault().AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
						additionalMessage = additionalMsgList.FirstOrDefault()?.Msg;
						var documentTitles = _queryExecutor.Execute<DocumentTitlesQuery, IEnumerable<DocumentTitlesAndTypeQuery>>(
						new DocumentTitlesQuery
						{
							Identifiers = user.Documents.Where(c => c.DocumentType != DocumentType.VirtualClassRoom).Select(x =>
								new DocumentIdentifier { DocumentId = x.DocumentId, DocumentType = x.DocumentType, AdditionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == x.DocumentId && !string.IsNullOrEmpty(f.Msg)).Msg }).GroupBy(c => c.DocumentId).Select(f => f.FirstOrDefault())
						}).ToList();
						foreach (var doc in documentTitles)
						{
							//var docModel = _queryExecutor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery() {
							//	Id = doc.DocumentId,
							//	DocumentType = doc.DocumentType
							//});
							//doc.Author = docModel.Author ?? "OnRamp Administrator";
							//doc.Points = docModel.Points;

							//below code commented by me
							var expiryDate = command.AssignedDate == null ? DateTime.UtcNow : command.AssignedDate.Value;

							var expiryDateCheck = false;


							switch (doc.DocumentType)
							{
								case DocumentType.Memo:
									var memo = _queryExecutor.Execute<FetchByIdQuery, MemoModel>(new FetchByIdQuery { Id = doc.DocumentId });
									doc.Author = memo.Author ?? "OnRamp Administrator";
									doc.Points = memo.Points;
									break;
								case DocumentType.AcrobatField:
									var AcrobatField = _queryExecutor.Execute<FetchByIdQuery, AcrobatFieldModel>(new FetchByIdQuery { Id = doc.DocumentId });
									doc.Author = AcrobatField.Author ?? "OnRamp Administrator";
									doc.Points = AcrobatField.Points;
									break;
								case DocumentType.Policy:
									var policy = _queryExecutor.Execute<FetchByIdQuery, PolicyModel>(new FetchByIdQuery { Id = doc.DocumentId });
									doc.Author = policy.Author ?? "OnRamp Administrator";
									doc.Points = policy.Points;
									break;
								case DocumentType.Test:
									var test = _queryExecutor.Execute<FetchByIdQuery, TestModel>(new FetchByIdQuery { Id = doc.DocumentId });
									doc.Author = test.Author ?? "OnRamp Administrator";
									doc.Points = test.Points;
									doc.Passmark = test.PassMarks;
									var expiryDays = test.TestExpiresNumberDaysFromAssignment ?? 0;
									if (doc.ExpiryDate == null || doc.ExpiryDate == Convert.ToDateTime("01 - Jan - 01 12:00:00 AM"))
									{

										doc.ExpiryDate = null;
									}

									if (test.TestExpiresNumberDaysFromAssignment > 0)
										doc.ExpiryDate = expiryDate.AddDays(expiryDays);

									break;
								case DocumentType.TrainingManual:
									var trainingManual = _queryExecutor.Execute<FetchByIdQuery, TrainingManualModel>(new FetchByIdQuery { Id = doc.DocumentId });
									doc.Author = trainingManual.Author ?? "OnRamp Administrator";
									doc.Points = trainingManual.Points;
									break;
								case DocumentType.Checklist:
									var checklist = _queryExecutor.Execute<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = doc.DocumentId });
									doc.Author = checklist.Author ?? "OnRamp Administrator";
									doc.Points = checklist.Points;
									doc.Passmark = checklist.PassMarks;
									//neeraj
									if (doc.ExpiryDate == null || doc.ExpiryDate == Convert.ToDateTime("01 - Jan - 01 12:00:00 AM"))
									{
										doc.ExpiryDate = null;
									}
									if (command.ExpiryDate != null)
										doc.ExpiryDate = expiryDate.AddDays(checklist.TestExpiresNumberDaysFromAssignment);
									break;
								case DocumentType.custom:
									var customDoc = _queryExecutor.Execute<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery { Id = doc.DocumentId });
									doc.Author = customDoc.Author ?? "OnRamp Administrator";
									doc.Points = customDoc.Points;
									break;
							}
						}
						if (documentTitles.Any())
						{
							new EventPublisher().Publish(new DocumentsAssignedEvent
							{
								UserViewModel = Project.UserViewModelFrom(standardUser),
								CompanyViewModel = Project.CompanyViewModelFrom(company),
								DocumentTitles = documentTitles,
								AdditionalMessage = additionalMessage,
								IsAssigned = true,
								Subject = $"{documentTitles.Count()} Document{(documentTitles.Count() > 1 ? "s" : "")} {(command.IsReassigned ? "Reassigned" : "Assigned")}"
							});
						}

					}
					else
					{
						var documentTitles = _queryExecutor.Execute<DocumentTitlesQuery, IEnumerable<DocumentTitlesAndTypeQuery>>(
						new DocumentTitlesQuery
						{
							Identifiers = user.Documents.Where(c => c.DocumentType != DocumentType.VirtualClassRoom).Select(x =>
							new DocumentIdentifier { DocumentId = x.DocumentId, DocumentType = x.DocumentType, AdditionalMsg = "" }).GroupBy(c => c.DocumentId).Select(f => f.FirstOrDefault())
						}).ToList();
						if (documentTitles.Any())
						{
							new EventPublisher().Publish(new DocumentsAssignedEvent
							{
								UserViewModel = Project.UserViewModelFrom(standardUser),
								CompanyViewModel = Project.CompanyViewModelFrom(company),
								DocumentTitles = documentTitles,
								IsAssigned = true,
								Subject = $"{documentTitles.Count()} Document{(documentTitles.Count() > 1 ? "s" : "")} {(command.IsReassigned ? "Reassigned" : "Assigned")}"
							});
						}
					}

				}
				#endregion
			}
			return response;
		}

		public CommandResponse Execute(UnassignDocumentsFromUsers command)
		{
			var response = new CommandResponse();
			var userId = ""; var sendcount = 0;
			foreach (var u in command.AssignmentViewModels)
			{
				userId = u.UserId;
				var additionalMessage = "";

				//var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
				var userDetails = _userRepository.Find(userId.ConvertToGuid());
				var documentsUsers = command.AssignmentViewModels.GroupBy(a => a.UserId,
					a => new { a.DocumentId, a.DocumentType }, (key, g) => new { UserId = key, Documents = g.ToList() }).ToList();
				var additionalMsgList = command.AssignmentViewModels.FirstOrDefault().AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
				//var documentsUsers = command.AssignmentViewModels.GroupBy(a => new { a.DocumentId, a.DocumentType,a.AdditionalMsg },
				//	a => a.UserId,
				//	(key, g) => new { key.DocumentId, key.DocumentType,key.AdditionalMsg, UserIds = g.ToList() });
				foreach (var user in documentsUsers)
				{
					if (user.UserId == userId)
					{
						var standardUser = _userRepository.Find(user.UserId.ConvertToGuid());
						var company = _companyRepository.Find(command.CompanyId);
						foreach (var document in user.Documents)
						{
							var additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == document.DocumentId && !string.IsNullOrEmpty(f.Msg)).Msg;
							//&& x.DocumentType == document.DocumentType &&
							//				x.Deleted == false &&

							var entity = _documentUsageRepository.List.Where(x => x.DocumentId == document.DocumentId && x.UserId == userId).ToList();
							if (entity.Count > 0)
							{
								foreach (var x in entity)
								{
									_documentUsageRepository.Delete(x);
								}

								_documentUsageRepository.SaveChanges();
							}
							additionalMessage = additionalMsg;
							_assignedDocumentRepository.List
								.Where(x => x.DocumentId == document.DocumentId && user.UserId == x.UserId && x.DocumentType == document.DocumentType &&
											x.Deleted == false)
								.ForEach(x => {
									x.Deleted = true;
									x.AdditionalMsg = additionalMsg;
									if (document.DocumentType == DocumentType.Test)
									{
										_testResultRepository.List.Where(test => test.UserId == x.UserId && test.TestId == document.DocumentId).ForEach(t => {
											t.Deleted = true;
										});
										var testSession = _queryExecutor.Execute<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
										{
											UserId = x.UserId
										});
										if (testSession?.CurrentTestId == x.DocumentId)
										{
											_commandDispatcher.Dispatch(new TestSessionEndCommand
											{
												UserId = x.UserId
											});
										}
									}
									if (document.DocumentType == DocumentType.Policy)
									{
										_policyResponseRepository.List.Where(r => user.UserId == x.UserId && r.PolicyId == document.DocumentId).ForEach(p => {
											//&& r.Response != null && r.Response != false
											//neeraj
											p.Response = null;
										});
									}
								}
							);
							#region keep the document audit track history
							var title = GetDocumentTitle(document.DocumentType, document.DocumentId);
							var documentAudit = new DocumentAuditTrack
							{
								Id = Guid.NewGuid(),
								LastEditDate = DateTime.UtcNow,
								LastEditedBy = userDetails.Id.ToString(),
								User = userDetails,
								DocumentId = document.DocumentId,
								DocumentName = title,
								DocumentType = document.DocumentType.ToString(),
								UserName = userDetails.LastName + " " + userDetails.FirstName,


							};

							documentAudit.DocumentStatus = "Document Unassigned";

							_documentAuditRepository.Add(documentAudit);
							_documentAuditRepository.SaveChanges();
							#endregion
						}

						#region This one for sending email 
						if (sendcount == 0)
						{
							var documentTitles = _queryExecutor.Execute<DocumentTitlesQuery, IEnumerable<DocumentTitlesAndTypeQuery>>(
								new DocumentTitlesQuery
								{
									Identifiers = user.Documents.Where(c => c.DocumentType != DocumentType.VirtualClassRoom).Select(x =>
									new DocumentIdentifier { AdditionalMsg = additionalMessage, DocumentId = x.DocumentId, DocumentType = x.DocumentType }).GroupBy(c => c.DocumentId).Select(f => f.FirstOrDefault())
								}).ToList();
							if (documentTitles.Any())
							{
								new EventPublisher().Publish(new DocumentsAssignedEvent
								{
									UserViewModel = Project.UserViewModelFrom(standardUser),
									CompanyViewModel = Project.CompanyViewModelFrom(company),
									DocumentTitles = documentTitles,
									AdditionalMessage = additionalMessage,
									IsAssigned = false,
									Subject = $"{documentTitles.Count()} Document{(documentTitles.Count() > 1 ? "s" : "")} Unassigned"
								});
								sendcount++;
							}
						}
						#endregion
					}
				}

				_assignedDocumentRepository.SaveChanges();

			}
			return response;

		}

		public CommandResponse Execute(UnassignDocumentFromAllUsersCommand command)
		{
			var documentType = DocumentType.Unknown;
			if (command.Type == typeof(TrainingManual))
				documentType = DocumentType.TrainingManual;
			else if (command.Type == typeof(Test))
				documentType = DocumentType.Test;
			else if (command.Type == typeof(Memo))
				documentType = DocumentType.Memo;
			else if (command.Type == typeof(AcrobatField))
				documentType = DocumentType.AcrobatField;
			else if (command.Type == typeof(Policy))
				documentType = DocumentType.Policy;
			else if (command.Type == typeof(CheckList))
				documentType = DocumentType.Checklist;
			else if (command.Type == typeof(CustomDocument))
				documentType = DocumentType.custom;

			_assignedDocumentRepository.List
				.Where(x => x.DocumentId == command.DocumentId && x.DocumentType == documentType && x.Deleted == false)
				.ForEach(x => x.Deleted = true);

			_assignedDocumentRepository.SaveChanges();

			return null;
		}

		public string GetDocumentTitle(DocumentType type, string documentId)
		{
			var title = "";

			switch (type)
			{
				case DocumentType.TrainingManual:
					title = _trainingManualRepository.Find(documentId).Title;
					break;
				case DocumentType.Test:
					title = _testRepository.Find(documentId).Title;
					break;
				case DocumentType.Policy:
					title = _policyRepository.Find(documentId).Title;
					break;
				case DocumentType.Memo:
					title = _memoRepository.Find(documentId).Title;
					break;
				case DocumentType.AcrobatField:
					title = _acrobatFieldRepository.Find(documentId).Title;
					break;
				case DocumentType.Checklist:
					title = _checkListRepository.Find(documentId).Title;
					break;
				case DocumentType.custom:
					title = _customDocumentRepository.Find(documentId).Title;
					break;
			}
			return title;
		}

		public void Handle(StandardUserDeletedEvent @event)
		{
			if (!string.IsNullOrWhiteSpace(@event.Id))
			{
				_assignedDocumentRepository.List.AsQueryable().Where(x => x.UserId == @event.Id).ToList().ForEach(x => _assignedDocumentRepository.Delete(x));
				_assignedDocumentRepository.SaveChanges();
			}
		}
	}
}