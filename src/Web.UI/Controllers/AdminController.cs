using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Common;
using Common.Command;
using Common.Query;
using Data.EF;
using Domain.Models;
using Newtonsoft.Json;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers
{
    public class AdminController : RampController
    {
        private readonly MainContext _mainContext = new MainContext();

        public ActionResult ViewDocuments()
        {
            var dummyModel = new CompanyModelShort
            {
                Name = "Select Company",
                Id = Guid.Empty
            };

            var provisonalCompanyList = ExecuteQuery<AllCustomerUserQueryParameter, CompanyViewModel>(
                new AllCustomerUserQueryParameter
                {
                    LoginUserCompanyId = PortalContext.Current.UserCompany.Id,
                    LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList()
                });

            provisonalCompanyList.CompanyList.Insert(0, dummyModel);
            provisonalCompanyList.Companies =
                provisonalCompanyList.CompanyList.Select(c => new SerializableSelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            return View(provisonalCompanyList);
        }

        [HttpPost]
        public PartialViewResult CustomerCompanyViewDocumentsPartial(Guid companyId)
        {
            var originalId = PortalContext.Current.UserCompany.Id;
            ViewBag.CompanyId = companyId;
            PortalContext.Override(companyId);
            var documents = Enumerable.Empty<DocumentListModel>();
            if (PortalContext.Current != null)
            {
               
                documents =
                    ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery() { EnableChecklistDocument=PortalContext.Current.UserCompany.EnableChecklistDocument});
            }

            PortalContext.Override(originalId);

            return PartialView(documents);
        }

        public ActionResult CopyDocuments()
        {
            var data = ExecuteQuery<AllCustomerUserQueryParameter, IEnumerable<SerializableSelectListItem>>(
                new AllCustomerUserQueryParameter
                {
                    LoginUserCompanyId = PortalContext.Current.UserCompany.Id,
                    LoginUserRole = Thread.CurrentPrincipal.GetRoles().ToList()
                });
            return View(new CopyDocumentsViewModel {Companies = data});
        }

        [HttpPost]
        public PartialViewResult CopyDocumentsFromCompanyPartial(Guid companyId)
        {
            ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = companyId.ToString() });

            var data = Enumerable.Empty<DocumentListModel>();
            if (PortalContext.Current != null)
            {
                data = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery() { EnableChecklistDocument=PortalContext.Current.UserCompany.EnableChecklistDocument});
            }

            ExecuteCommand(new UpdateConnectionStringCommand());

            return PartialView(data);
        }

        [HttpPost]
        public PartialViewResult CopyDocumentsToCompanyPartial(Guid companyId)
        {
            ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = companyId.ToString()});

            var data = Enumerable.Empty<DocumentListModel>();
            if (PortalContext.Current != null)
            {
                data = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery() { EnableChecklistDocument=PortalContext.Current.UserCompany.EnableChecklistDocument});
                var documentsRemaining =
                    ExecuteQuery<DocumentsRemainingQuery, int>(new DocumentsRemainingQuery {CompanyId = companyId});
                ViewBag.DocumentAdd =
                    $"You can only copy {documentsRemaining} more Documents";
                ViewBag.RemainingDocuments = documentsRemaining;
            }

            ExecuteCommand(new UpdateConnectionStringCommand());

            return PartialView(data);
        }

        [HttpPost]
        public ActionResult GenerateCopyDocumentsCommand(Guid fromCustomerCompanyId, Guid toCustomerCompanyId,
            IEnumerable<string> documents)
        {
            PortalContext.Override(fromCustomerCompanyId);
			//var docs = documents.Select(c => c.Split(':')[1]).ToList();
            var copyDocumentsFromDocumentsViewModel =
                ExecuteQuery<GetDocumentsToCopyFromCompanyQuery, CopyDocumentsFromCustomerCompanyViewModel>(
                    new GetDocumentsToCopyFromCompanyQuery
                    {
                        FromCustomerCompanyId = fromCustomerCompanyId,
                        ToCustomerCompanyId = toCustomerCompanyId,
                        DocumentList = documents
					});

            var command = new CopyDocumentsToAnotherCustomerCommand
            {
                ToCustomerCompanyId = toCustomerCompanyId.ToString(),
                FromCustomerCompanyId = fromCustomerCompanyId.ToString(),
                CopyDocumentsFromCustomerCompanyViewModel = copyDocumentsFromDocumentsViewModel
            };

            try
            {
                var serialized = JsonConvert.SerializeObject(command,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    });

                var auditTrailCommand = new CreateCommandAuditTrailCommand
                {
                    Id = Guid.NewGuid(),
                    Command = serialized,
                    CommandType = command.GetType().FullName,
                    Executed = false
                };
                ExecuteCommand(auditTrailCommand);

                return new JsonResult {Data = auditTrailCommand.Id.ToString()};
            }
            catch (Exception)
            {
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        public HttpStatusCodeResult CopyDocumentsToAnotherCustomer(string commandId)
        {
            if (string.IsNullOrWhiteSpace(commandId) || commandId.ConvertToGuid() == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var c = ExecuteQuery<FetchByIdQuery<CommandAuditTrail>, CommandAuditTrail>(
                new FetchByIdQuery<CommandAuditTrail>
                {
                    Id = commandId
                });

            var command = JsonConvert.DeserializeObject<CopyDocumentsToAnotherCustomerCommand>(c.Command);

            PortalContext.Override(command.ToCustomerCompanyId.ConvertToGuid());

            var result = ExecuteCommand(command);
            if (!result.Validation.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
    }
}