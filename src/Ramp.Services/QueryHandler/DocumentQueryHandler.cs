using Common;
using Common.Collections;
using Common.Command;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using LinqKit;
using Ramp.Contracts.Command.Label;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Ramp.Contracts.Security;
using Ramp.Contracts.Query.CheckList;
using Domain.Customer.Models.CheckLists;
using System.Web.Mvc;
using Ramp.Contracts.Query.CustomDocument;
using Common.Data;

namespace Ramp.Services.QueryHandler
{
    public class DocumentQueryHandler : IQueryHandler<DocumentListQuery, IEnumerable<DocumentListModel>>,
                                        IQueryHandler<DocumentListQuery, IPaged<DocumentListModel>>,
                                        IQueryHandler<DocumentReferenceIdQuery, string>,
                                        IQueryHandler<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>,
                                        IQueryHandler<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>,
                                        IQueryHandler<SyncDocumentLabelsQuery, IEnumerable<Label>>,
                                        IQueryHandler<SyncDocumentCoverPictureQuery, Upload>,
                                        IQueryHandler<GetDocumentsToCopyFromCompanyQuery, CopyDocumentsFromCustomerCompanyViewModel>,
                                        IQueryHandler<DocumentCollaboratorsQuery, IEnumerable<UserModelShort>>,

        IQueryHandler<DocumentQuery, DocumentListModel>
    {
        private readonly IQueryExecutor _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;


        private readonly IRepository<CustomDocumentMessageCenter> _CustomDocumentMessageCenterRepository;


        public DocumentQueryHandler(IQueryExecutor queryDispatcher, ICommandDispatcher commandDispatcher,
            IRepository<CustomDocumentMessageCenter> CustomDocumentMessageCenterRepository)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;

            _CustomDocumentMessageCenterRepository = CustomDocumentMessageCenterRepository;
        }
        public IEnumerable<DocumentListModel> ExecuteQuery(DocumentListQuery query)
        {
            var documents = new List<DocumentListModel>();
            var types = GetDocumentFilters(query.DocumentFilters, (x => x == "Type")).String_DocumentType();
            query.DocumentStatus = GetDocumentFilters(query.DocumentFilters, (x => x == "Status"));

            bool IGA = true;
            var filter = query.DocumentFilters.Where(x => x.Contains("IsGlobalAccessed")).FirstOrDefault();
            if (filter != null)
            {
                var temp = filter.Split(':');
                IGA = Convert.ToBoolean(temp[1]);
            }

            AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Memo) : true, _queryDispatcher.Execute<MemoListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_MemoListQuery.Invoke(query)));

            AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Policy) : true, _queryDispatcher.Execute<PolicyListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_PolicyListQuery.Invoke(query)));
            AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Test) : true, _queryDispatcher.Execute<TestListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_TestListQuery.Invoke(query)));
            if (query.EnableChecklistDocument.HasValue && query.EnableChecklistDocument.Value)
            {
                AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Checklist) : true, _queryDispatcher.Execute<CheckListQuery, IEnumerable<DocumentListModel>>(Project.CheckListQueryBase_CheckListQuery.Invoke(query)));
            }
            AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.custom) : true, _queryDispatcher.Execute<CustomDocumentListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_CustomDocumentListQuery.Invoke(query)));
            var CustomDoc = _queryDispatcher.Execute<FetchAllRecordsQuery, IEnumerable<DocumentListModel>>(new FetchAllRecordsQuery { });
            documents.AddRange(CustomDoc);

            AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.TrainingManual) : true, _queryDispatcher.Execute<TrainingManualListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_TrainingManualListQuery.Invoke(query)));

            var temp1 = _queryDispatcher.Execute<TrainingManualListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_TrainingManualListQuery.Invoke(query));

            if (!string.IsNullOrWhiteSpace(query.MatchText))
                documents = documents.Where(x => ContainsText(query.MatchText, x)).ToList();
            if (!string.IsNullOrWhiteSpace(query.SortingOrder))
            {
                string orderedProperty = null;
                var sortOrder = query.SortingOrder.String_SortingOrder(out orderedProperty);
                if (sortOrder.HasValue)
                {
                    switch (orderedProperty)
                    {
                        case "Type":
                            documents = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.DocumentType.ToString()).ToList() : documents.OrderBy(x => x.DocumentType.ToString()).ToList();
                            break;
                        case "Title":
                            documents = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.Title).ToList() : documents.OrderBy(x => x.Title).ToList();
                            break;
                        case "LastEdited":
                            documents = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.LastEditDate).ToList() : documents.OrderBy(x => x.LastEditDate).ToList();
                            break;
                        case "Status":
                            documents = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.DocumentStatus.ToString()).ToList() : documents.OrderBy(x => x.DocumentStatus.ToString()).ToList();
                            break;
                        default:
                            documents.OrderBy(x => x.ReferenceId).ToArray();
                            break;
                    }
                }
            }
            else
            {
                documents = documents.OrderByDescending(x => x.LastEditDate).ToList();
            }

            if (!query.TemplatePortal && Thread.CurrentPrincipal.IsInRole(Role.ContentAdmin) && !Thread.CurrentPrincipal.IsInRole(Role.CustomerAdmin))
            {
                var userId = (Guid)(Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims
                    .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value.ConvertToGuid();
                documents = documents.Where(d => d.Approver == userId.ToString() || d.CreatedBy.ToString() == userId.ToString()).ToList();
            }

            //neeraj
            if (Thread.CurrentPrincipal.IsInRole(Role.ContentCreator))
            {
                var id1 = (Guid)(Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims
                    .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value.ConvertToGuid();
                documents = documents.Where(d =>  d.Approver == id1.ToString() || d.CreatedBy.ToString() == id1.ToString()).ToList();
            }
            //neeraj
            var documentTemp = new List<DocumentListModel>();
            if (Thread.CurrentPrincipal.IsInRole(Role.ContentApprover))
            {
                var id1 = (Guid)(Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims
                    .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value.ConvertToGuid();

                //neeraj change wrt content approve list
                documents = documents.Where(x => (x.Approver!=null? x.Approver.Split(',').Contains(id1.ToString()): x.Approver==id1.ToString()) || x.CreatedBy.ToString() == id1.ToString()).ToList();

                var types1 = GetDocumentFilters(query.DocumentFilters, (x => x == "Type")).String_DocumentType();
                query.DocumentStatus = GetDocumentFilters(query.DocumentFilters, (x => x == "Status"));

                var tempMemo = _queryDispatcher.Execute<MemoListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_MemoListQuery.Invoke(query));
                AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Memo) : true, tempMemo.Where(tm => tm.Approver != null && tm.Approver.Contains(id1.ToString())));

                var tempPolicy = _queryDispatcher.Execute<PolicyListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_PolicyListQuery.Invoke(query));
                AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Policy) : true, tempPolicy.Where(tm => tm.Approver != null && tm.Approver.Contains(id1.ToString())));

                var tempTest = _queryDispatcher.Execute<TestListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_TestListQuery.Invoke(query));
                AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Test) : true, tempTest.Where(tm => tm.Approver != null && tm.Approver.Contains(id1.ToString())));

                if (query.EnableChecklistDocument.HasValue && query.EnableChecklistDocument.Value)
                {
                    var tempCKL = _queryDispatcher.Execute<CheckListQuery, IEnumerable<DocumentListModel>>(Project.CheckListQueryBase_CheckListQuery.Invoke(query));
                    AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.Checklist) : true, tempCKL.Where(tm => tm.Approver != null && tm.Approver.Contains(id1.ToString())));
                }

                var tempTM = _queryDispatcher.Execute<TrainingManualListQuery, IEnumerable<DocumentListModel>>(Project.DocumentListQueryBase_TrainingManualListQuery.Invoke(query));
                AddConditional(documents, () => types.Any() ? types.Contains(DocumentType.TrainingManual) : true, tempTM.Where(tm => tm.Approver != null && tm.Approver.Contains(id1.ToString())));

                if (!string.IsNullOrWhiteSpace(query.MatchText))
                    documents = documents.Where(x => ContainsText(query.MatchText, x)).ToList();
                if (!string.IsNullOrWhiteSpace(query.SortingOrder))
                {
                    string orderedProperty = null;
                    var sortOrder = query.SortingOrder.String_SortingOrder(out orderedProperty);
                    if (sortOrder.HasValue)
                    {
                        switch (orderedProperty)
                        {
                            case "Type":
                                documentTemp = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.DocumentType.ToString()).ToList() : documents.OrderBy(x => x.DocumentType.ToString()).ToList();
                                break;
                            case "Title":
                                documentTemp = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.Title).ToList() : documents.OrderBy(x => x.Title).ToList();
                                break;
                            case "LastEdited":
                                documentTemp = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.LastEditDate).ToList() : documents.OrderBy(x => x.LastEditDate).ToList();
                                break;
                            case "Status":
                                documentTemp = sortOrder.Value == SortOrder.Descending ? documents.OrderByDescending(x => x.DocumentStatus.ToString()).ToList() : documents.OrderBy(x => x.DocumentStatus.ToString()).ToList();
                                break;
                            default:
                                documentTemp.OrderBy(x => x.ReferenceId).ToArray();
                                break;
                        }
                    }
                }
                else
                {
                    //documentTemp = documents.OrderByDescending(x => x.LastEditDate).ToList();
                }
            }

            //if(documentTemp != null)
            //{
            //	documents.AddRange(documentTemp);
            //}
            foreach (var item in documents)
            {
                if (item.DocumentType == DocumentType.Test && item.Deleted != true)
                {
                    var model = _queryDispatcher.Execute<FetchByIdQuery, TestModel>(new FetchByIdQuery() { Id = item.Id });
                    item.Certificate = new UploadResultViewModel();
                    item.Certificate = model.Certificate;
                    item.PassMarks = model.PassMarks;
                    item.Duration = model.Duration;
                }

                item.DeclineMessages = _CustomDocumentMessageCenterRepository.List.Where(z => z.DocumentId == item.Id.ToString()).Select(c => new DeclineMessages { messages = c.Messages }).ToList();
            }

            if (IGA == false)
            {
                documents = documents.Where(x => x.IsGlobalAccessed == false && x.IsGlobalAccessed != true).ToList();
            }

            return documents.GroupBy(x => x.Id).Select(x => x.First());
        }
        private IEnumerable<string> GetDocumentFilters(IEnumerable<string> filters, Func<string, bool> where)
        {
            var result = new List<string>();
            foreach (var v in filters)
            {
                if (string.IsNullOrWhiteSpace(v))
                    continue;
                var split = v.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2 && split[0] != null && split[1] != null)
                {
                    if (where(split[0]))
                        result.Add(split[1]);
                }

            }
            return result;
        }
        private bool ContainsText(string text, DocumentListModel model)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(model.TrainingLabels, text, CompareOptions.IgnoreCase) >= 0 || CultureInfo.InvariantCulture.CompareInfo.IndexOf(model.Title, text, CompareOptions.IgnoreCase) >= 0 ||
                (model.LastEditedByModel != null ? CultureInfo.InvariantCulture.CompareInfo.IndexOf(model.LastEditedByModel.Name, text, CompareOptions.IgnoreCase) >= 0 : false) ||
                (model.LastEditDate.HasValue ? CultureInfo.InvariantCulture.CompareInfo.IndexOf(model.LastEditDate.Value.ToString("dd/MM/yyy"), text, CompareOptions.IgnoreCase) >= 0 : false);
        }
        private List<DocumentListModel> AddConditional(List<DocumentListModel> list, Func<bool> condition, IEnumerable<DocumentListModel> entries)
        {
            if (condition())
                list.AddRange(entries);
            return list;
        }

        public string ExecuteQuery(DocumentReferenceIdQuery query)
        {
            var numberOfDocuments =
                _queryDispatcher.Execute<FetchTotalDocumentsQuery<TrainingManual>, int>(new FetchTotalDocumentsQuery<TrainingManual>()) +
                _queryDispatcher.Execute<FetchTotalDocumentsQuery<Memo>, int>(new FetchTotalDocumentsQuery<Memo>()) +
                _queryDispatcher.Execute<FetchTotalDocumentsQuery<Policy>, int>(new FetchTotalDocumentsQuery<Policy>()) +
                 _queryDispatcher.Execute<FetchTotalDocumentsQuery<CheckList>, int>(new FetchTotalDocumentsQuery<CheckList>()) +
                _queryDispatcher.Execute<FetchTotalDocumentsQuery<Test>, int>(new FetchTotalDocumentsQuery<Test>());
            var temp = (numberOfDocuments + 1).ToString();
            var result = new StringBuilder("RF");
            while ((result.Length + temp.Length) != 8)
            {
                result.Append("0");
            }
            result.Append(temp);
            return result.ToString();
        }

        public IEnumerable<Upload> ExecuteQuery(SyncDocumentContentUploadsQuery query)
        {
            var result = new List<Upload>();
            var old = query.ExistingModelIds;
            var added = query.Models.Select(x => x.Id.ToString()).Where(x => !old.Contains(x));
            var updated = old.Intersect(query.Models.Select(x => x.Id.ToString()));
            var removed = old.Where(x => !added.Contains(x) && !updated.Contains(x)).ToList();

            query.Models.ToList().ForEach(u =>
            {
                var upload = _queryDispatcher.Execute<FetchByIdQuery, Upload>(new FetchByIdQuery { Id = u.Id });
                if (upload != null)
                    if (added.Contains(upload.Id))
                        result.Add(upload);
            });
            removed.ForEach((id) =>
            {
                _commandDispatcher.Dispatch(new DeleteUploadCommand { Id = id });
            });
            return result;
        }

        public IEnumerable<Upload> ExecuteQuery(SyncDocumentContentToolsUploadsQuery query)
        {
            var result = new List<Upload>();
            var modelUploadIds = query.Models.Select(GetContentToolsUploadId).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            var old = query.ExistingModelIds;
            var added = modelUploadIds.Select(x => x.ToString()).Where(x => !old.Contains(x));
            var removed = old.Where(x => !added.Contains(x) && !modelUploadIds.Contains(x)).ToList();

            query.Models.ToList().ForEach(u =>
            {
                var id = GetContentToolsUploadId(u);
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var upload = _queryDispatcher.Execute<FetchByIdQuery, Upload>(new FetchByIdQuery { Id = id });
                    if (upload != null)
                        if (added.Contains(upload.Id))
                            result.Add(upload);
                }
            });
            //removed.ForEach(id =>
            //{
            //    _commandDispatcher.Dispatch(new DeleteUploadCommand { Id = id });
            //});
            return result;
        }

        public IEnumerable<Label> ExecuteQuery(SyncDocumentLabelsQuery query)
        {
            var result = new List<Label>();
            _commandDispatcher.Dispatch(new SyncLabelCommand { Values = query.Values });
            var old = query.ExistingModelIds;
            var added = query.Values.Where(x => !old.Contains(x));
            var removed = old.Where(x => !query.Values.Contains(x));

            _queryDispatcher.Execute<LabelListQuery, IEnumerable<Label>>(new LabelListQuery { Values = query.Values }).ToList().ForEach(x =>
            {
                if (!removed.Contains(x.Name))
                    result.Add(x);
            });
            return result;
        }

        public Upload ExecuteQuery(SyncDocumentCoverPictureQuery query)
        {
            if (!string.IsNullOrEmpty(query.ExistingUploadId) && (string.IsNullOrEmpty(query.ModelId) || query.ModelId != query.ExistingUploadId))
                _commandDispatcher.Dispatch(new DeleteUploadCommand { Id = query.ExistingUploadId });

            return _queryDispatcher.Execute<FetchByIdQuery, Upload>(new FetchByIdQuery { Id = query.ModelId });
        }

        IPaged<DocumentListModel> IQueryHandler<DocumentListQuery, IPaged<DocumentListModel>>.ExecuteQuery(DocumentListQuery query)
        {
            return query.GetPagedWithoutProjection(_queryDispatcher.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(query));
        }

        private string GetContentToolsUploadId(UploadFromContentToolsResultModel x)
        {
            var matchText = x != null && x.url != null ?
                           x.url.IndexOf("/Upload/Get/") > -1 ? "/Upload/Get/" :
                           x.url.IndexOf("/Upload/GetThumbnail/") > -1 ? "/Upload/GetThumbnail/" :
                           x.url.IndexOf("/Upload/GetFromCompany/") > -1 ? "/Upload/GetFromCompany/" :
                           x.url.IndexOf("/Upload/GetThumbnailFromCompany/") > -1 ? "/Upload/GetThumbnailFromCompany/" : null : null;
            var sId = x.url.Length == 36 ? x.url : matchText == null ? null : x.url.Replace(x.url.Substring(0, x.url.IndexOf(matchText) + matchText.Length), string.Empty);
            if (sId == null) return null;
            if (sId.IndexOf('?') > -1)
                sId = sId.Substring(0, sId.IndexOf('?'));
            if (Guid.TryParse(sId, out var gId))
                return sId;
            return null;
        }

        public CopyDocumentsFromCustomerCompanyViewModel ExecuteQuery(GetDocumentsToCopyFromCompanyQuery query)
        {
            var memos = new List<MemoModel>();
            var policies = new List<PolicyModel>();
            var tests = new List<TestModel>();
            var trainingManuals = new List<TrainingManualModel>();
            var checkLists = new List<CheckListModel>();
            var CustomDocumentLists = new List<CustomDocumentModel>();
            foreach (var documentString in query.DocumentList)
            {
                var args = documentString.Split(':'); // $"{type}:{id}"
                if (Enum.TryParse(args[0], out DocumentType type))
                {
                    switch (type)
                    {
                        case DocumentType.Memo:
                            var memoModel = _queryDispatcher.Execute<FetchByIdQuery, MemoModel>(new FetchByIdQuery
                            {
                                Id = args[1]
                            });
                            if (memoModel != null)
                            {
                                memos.Add(memoModel);
                            }
                            break;
                        case DocumentType.Policy:
                            var policyModel = _queryDispatcher.Execute<FetchByIdQuery, PolicyModel>(new FetchByIdQuery
                            {
                                Id = args[1]
                            });
                            if (policyModel != null)
                            {
                                policies.Where(x => x.Deleted != true);
                                policies.Add(policyModel);
                            }
                            break;
                        case DocumentType.Test:
                            var testModel = _queryDispatcher.Execute<FetchByIdQuery, TestModel>(new FetchByIdQuery
                            {
                                Id = args[1]
                            });
                            if (testModel != null)
                            {
                                tests.Add(testModel);
                            }
                            break;
                        case DocumentType.TrainingManual:
                            var trainingManuaModel = _queryDispatcher.Execute<FetchByIdQuery, TrainingManualModel>(new FetchByIdQuery
                            {
                                Id = args[1]
                            });
                            if (trainingManuaModel != null)
                            {
                                trainingManuals.Add(trainingManuaModel);
                            }
                            break;
                        case DocumentType.Checklist:
                            var checkListModel = _queryDispatcher.Execute<FetchByIdQuery, CheckListModel>(new FetchByIdQuery
                            {
                                Id = args[1]
                            });
                            if (checkListModel != null)
                            {
                                checkLists.Add(checkListModel);
                            }
                            break;
                        case DocumentType.custom:
                            var CustomDocumentListModel = _queryDispatcher.Execute<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery
                            {
                                Id = args[1]
                            });
                            if (CustomDocumentListModel != null)
                            {
                                CustomDocumentLists.Add(CustomDocumentListModel);
                            }
                            break;
                    }
                }
            }

            return new CopyDocumentsFromCustomerCompanyViewModel()
            {
                Memos = memos,
                Policies = policies,
                Tests = tests,
                TrainingManuals = trainingManuals,
                CheckLists = checkLists
            };
        }

        public IEnumerable<UserModelShort> ExecuteQuery(DocumentCollaboratorsQuery query)
        {
            DocumentListModel doc = null;
            switch (query.DocumentType)
            {
                case DocumentType.Memo:
                    doc = _queryDispatcher.Execute<FetchByIdQuery, MemoModel>(new FetchByIdQuery
                    {
                        Id = query.DocumentId
                    });
                    break;
                case DocumentType.Policy:
                    doc = _queryDispatcher.Execute<FetchByIdQuery, PolicyModel>(new FetchByIdQuery
                    {
                        Id = query.DocumentId
                    });
                    break;
                case DocumentType.TrainingManual:
                    doc = _queryDispatcher.Execute<FetchByIdQuery, TrainingManualModel>(new FetchByIdQuery
                    {
                        Id = query.DocumentId
                    });
                    break;
                case DocumentType.Test:
                    doc = _queryDispatcher.Execute<FetchByIdQuery, TestModel>(new FetchByIdQuery
                    {
                        Id = query.DocumentId
                    });
                    break;
                case DocumentType.Checklist:
                    doc = _queryDispatcher.Execute<FetchByIdQuery, CheckListModel>(new FetchByIdQuery
                    {
                        Id = query.DocumentId
                    });
                    break;
                case DocumentType.custom:
                    doc = _queryDispatcher.Execute<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery
                    {
                        Id = query.DocumentId
                    });
                    break;
            }

            return doc?.Collaborators ?? Enumerable.Empty<UserModelShort>();
        }

        public DocumentListModel ExecuteQuery(DocumentQuery query)
        {
            if (!query.DocumentType.HasValue) return null;

            switch (query.DocumentType)
            {
                case DocumentType.Memo:
                    return _queryDispatcher.Execute<FetchByIdQuery, MemoModel>(new FetchByIdQuery { Id = query.Id });
                case DocumentType.Policy:
                    return _queryDispatcher.Execute<FetchByIdQuery, PolicyModel>(new FetchByIdQuery { Id = query.Id });
                case DocumentType.Test:
                    return _queryDispatcher.Execute<FetchByIdQuery, TestModel>(new FetchByIdQuery { Id = query.Id });
                case DocumentType.TrainingManual:
                    return _queryDispatcher.Execute<FetchByIdQuery, TrainingManualModel>(new FetchByIdQuery { Id = query.Id });
                case DocumentType.Checklist:
                    return _queryDispatcher.Execute<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = query.Id });
                case DocumentType.custom:
                    return _queryDispatcher.Execute<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery { Id = query.Id });
            }

            return null;
        }


    }
}

namespace Ramp.Services
{
    public static partial class Extensions
    {
        public static IEnumerable<DocumentType> String_DocumentType(this IEnumerable<string> documentTypeCollection)
        {
            var result = new List<DocumentType>();
            foreach (var t in documentTypeCollection)
            {
                if (Enum.TryParse<DocumentType>(t, out var type))
                    if (!result.Contains(type))
                        result.Add(type);
            }
            return result;
        }
        public static SortOrder? String_SortingOrder(this string sortOrder, out string property)
        {
            property = null;
            if (string.IsNullOrWhiteSpace(sortOrder))
                return null;
            var s = sortOrder.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length == 2 && s[0] != null && s[1] != null && Enum.TryParse<SortOrder>(s[1], out var so))
            {
                property = s[0];
                return so;
            }

            return null;
        }
    }
}

namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<StandardUser, UserModelShort>> StandardUser_UserModelShort =
            x => new UserModelShort
            {
                Id = x.Id,
                Name = x.FirstName + " " + x.LastName,
                Email = x.EmailAddress
            };

        public static readonly Expression<Func<CheckListQuery, DocumentListQueryBase>> CheckListQuery_DocumentListQueryBase =
           x => new DocumentListQueryBase
           {
               CategoryId = x.CategoryId,
               DocumentStatus = x.DocumentStatus,
               MatchText = x.MatchText,
               SortingOrder = x.SortingOrder
           };

        public static readonly Expression<Func<MemoListQuery, DocumentListQueryBase>> MemoListQuery_DocumentListQueryBase =
            x => new DocumentListQueryBase
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<PolicyListQuery, DocumentListQueryBase>> PolicyListQuery_DocumentListQueryBase =
            x => new DocumentListQueryBase
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<TestListQuery, DocumentListQueryBase>> TestListQuery_DocumentListQueryBase =
            x => new DocumentListQueryBase
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<TrainingManualListQuery, DocumentListQueryBase>> TrainingManualListQuery_DocumentListQueryBase =
            x => new DocumentListQueryBase
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<CustomDocumentListQuery, DocumentListQueryBase>> CustomDocumentListQuery_DocumentListQueryBase =
           x => new DocumentListQueryBase
           {
               CategoryId = x.CategoryId,
               DocumentStatus = x.DocumentStatus,
               MatchText = x.MatchText,
               SortingOrder = x.SortingOrder
           };
        public static readonly Expression<Func<DocumentListQueryBase, MemoListQuery>> DocumentListQueryBase_MemoListQuery =
            x => new MemoListQuery
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<DocumentListQueryBase, PolicyListQuery>> DocumentListQueryBase_PolicyListQuery =
            x => new PolicyListQuery
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<DocumentListQueryBase, TestListQuery>> DocumentListQueryBase_TestListQuery =
            x => new TestListQuery
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<DocumentListQueryBase, CheckListQuery>> CheckListQueryBase_CheckListQuery =
           x => new CheckListQuery
           {
               CategoryId = x.CategoryId,
               DocumentStatus = x.DocumentStatus,
               MatchText = x.MatchText,
               SortingOrder = x.SortingOrder
           };
        public static readonly Expression<Func<DocumentListQueryBase, TrainingManualListQuery>> DocumentListQueryBase_TrainingManualListQuery =
            x => new TrainingManualListQuery
            {
                CategoryId = x.CategoryId,
                DocumentStatus = x.DocumentStatus,
                MatchText = x.MatchText,
                SortingOrder = x.SortingOrder
            };
        public static readonly Expression<Func<DocumentListQueryBase, CustomDocumentListQuery>> DocumentListQueryBase_CustomDocumentListQuery =
           x => new CustomDocumentListQuery
           {
               CategoryId = x.CategoryId,
               DocumentStatus = x.DocumentStatus,
               MatchText = x.MatchText,
               SortingOrder = x.SortingOrder
           };

        public static readonly Expression<Func<DocumentListQueryBase, CustomDocumentListQuery>> CustomListQueryBase_CustomListQuery =
           x => new CustomDocumentListQuery
           {
               CategoryId = x.CategoryId,
               DocumentStatus = x.DocumentStatus,
               MatchText = x.MatchText,
               SortingOrder = x.SortingOrder

           };
    }
}
