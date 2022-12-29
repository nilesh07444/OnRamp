
////new file added by softude


//using Common.Command;
//using Common.Data;
//using Common.Query;
//using Data.EF.Customer;
//using Domain.Customer;
//using Domain.Customer.Models;
//using Domain.Customer.Models.Document;
//using Domain.Customer.Models.Forms;
//using Ramp.Contracts.Command.Document;
//using Ramp.Contracts.ViewModel;
//using Ramp.Services.Projection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using Domain.Customer.Models.Test;
//using LinqKit;
//using Ramp.Contracts.Query.Document;
//using Ramp.Contracts.Query.Test;
//using Ramp.Contracts.Query.DocumentCategory;
//using Ramp.Contracts.Query.RecycleBinQuery;




//namespace Ramp.Services.QueryHandler
//{
//    public class FormQueryHandler : IQueryHandler<FetchByIdQuery, FormModel>,
//    IQueryHandler<FetchByIdQuery<Form>, FormResultModel>,
//    IQueryHandler<FormListQuery, IEnumerable<FormListModel>>,
//    IQueryHandler<RecycleFormQuery, IEnumerable<DocumentListModel>>,
//    IQueryHandler<FormListQuery, IEnumerable<DocumentListModel>>,
//    IQueryHandler<FetchByCategoryIdQuery, FormChartViewModel>,
//    IQueryHandler<FetchTotalDocumentsQuery<Form>, int>
//    {
//        readonly ITransientReadRepository<Form> _repository;
//        readonly ICommandDispatcher _commandDispatcher;
//        private readonly IQueryExecutor _queryExecutor;
//        private readonly IRepository<DocumentUrl> _documentUrlRepository;
//        private readonly IRepository<StandardUser> _standardUser;

//        public FormQueryHandler(ITransientReadRepository<Form> repository, ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor, IRepository<DocumentUrl> documentUrlRepository, IRepository<StandardUser> standardUser)
//        {
//            _repository = repository;
//            _commandDispatcher = commandDispatcher;
//            _queryExecutor = queryExecutor;
//            _documentUrlRepository = documentUrlRepository;
//            _standardUser = standardUser;
//        }

//        public FormChartViewModel ExecuteQuery(FetchByCategoryIdQuery query)
//        {
//            var id = Convert.ToString(query.Id);
//            var form = _repository.List.Where(c => c.CategoryId == id);
//            var result = new FormChartViewModel()
//            {
//                FormCount = form.Count()
//            };
//            return result;
//        }

//        public FormModel ExecuteQuery(FetchByIdQuery query)
//        {
//            var form = _repository.Find(query.Id?.ToString());
//            if (form == null || (form != null && form.Deleted))
//                return new FormModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
//            var model = Project.Form_FormModel.Invoke(form);

//            _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model });
//            return model;
//        }

//        public IEnumerable<DocumentListModel> ExecuteQuery(FormListQuery query)
//        {
//            var entries = Filter(query).Where(x => x.IsCustomDocument == null).Select(Project.Form_DocumentListModel).ToList();
//            entries.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));

//            var users = _standardUser.List.ToList();

//            entries.ForEach(x =>
//            {
//                if (x.Approver != null)
//                {
//                    var userIds = x.Approver.Split(',').ToList();
//                    string names = null;
//                    foreach (var i in userIds)
//                    {
//                        foreach (var u in users)
//                        {
//                            if (u.Id.ToString() == i)
//                            {
//                                if (names == null)
//                                {
//                                    names = u.FirstName + " " + u.LastName;
//                                }
//                                else if (names != null)
//                                {
//                                    names = names + ", " + u.FirstName + " " + u.LastName;
//                                }
//                            }
//                        }
//                    }

//                    x.ApproversName = names;
//                }
//                else x.ApproversName = "none";
//            });

//            return entries;
//        }

//    }
//}

//namespace Ramp.Services.Projection
//{
//    public static partial class Project
//    {

//        public static readonly Expression<Func<Form, FormModel>> Form_FormModel =
//               x => new FormModel
//               {

//                   CreatedBy = x.CreatedBy,
//                   CreatedOn = x.CreatedOn,
//                   LastEditedBy = x.LastEditedBy,
//                   Deleted = x.Deleted,
//                   Description = x.Description,
//                   Id = x.Id.ToString(),
//                   LastEditDate = x.LastEditDate,
//                   ReferenceId = x.ReferenceId,
//                   Title = x.Title,
//                   DocumentType = DocumentType.Form,
//                   FormContentModels = x.FormChapters.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(FormChapter_FormChapterModel).ToArray(),
//               };


//        public static readonly Expression<Func<FormChapter, FormChapterModel>> FormChapter_FormChapterModel =
//                x => new FormChapterModel
//                {
//                    Deleted = x.Deleted,
//                    Id = x.Id.ToString(),
//                    Number = x.Number,
//                    Title = x.Title,
//                    CheckRequired = x.CheckRequired,
//                    CustomDocumentOrder = x.CustomDocumentOrder,
//                    Content = x.Content,
//                    FormFieldTitle = x.FormFieldTitle,
//                    FormFieldValue = x.FormFieldValue,
//                    IsConditionalLogic = x.IsConditionalLogic
//                };


//        public static readonly Expression<Func<Form, DocumentListModel>> Form_DocumentListModel =
//            x => new DocumentListModel
//            {
//                Approver = x.Approver,
//                ApproverId = x.ApproverId,
//                PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
//                CreatedBy = x.CreatedBy.ToString(),
//                CreatedOn = x.CreatedOn.Value,
//                Deleted = x.Deleted,
//                Description = x.Description,
//                Id = x.Id.ToString(),
//                DocumentStatus = x.DocumentStatus,
//                DocumentType = DocumentType.Test,
//                LastEditDate = x.LastEditDate,
//                Points = x.Points,
//                Printable = x.Printable,
//                ReferenceId = x.ReferenceId,
//                Title = x.Title,
//                CategoryId = x.CategoryId,
//                CoverPictureId = x.CoverPictureId,
//                TrainingLabels = string.IsNullOrEmpty(x.TrainingLabels) ? "none" : x.TrainingLabels,
//                LastEditedBy = x.LastEditedBy,
//                IsGlobalAccessed = x.IsGlobalAccessed,
//                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort).ToList()
//            };


//    }
//}
