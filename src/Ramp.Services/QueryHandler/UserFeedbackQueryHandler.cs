using Common.Query;
using Ramp.Contracts.Query.UserFeedback;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using LinqKit;
using Ramp.Contracts.Query.Document;
using Ramp.Services.Projection;
using VirtuaCon;

namespace Ramp.Services.QueryHandler
{
    public class UserFeedbackQueryHandler : IQueryHandler<UserFeedbackListQuery, IEnumerable<UserFeedbackListModel>>,
        IQueryHandler<FetchByIdQuery, UserFeedbackModel>,
        IQueryHandler<FilteredUserFeedbackQuery, IEnumerable<UserFeedbackViewModelShort>>
    {
        private readonly IRepository<UserFeedback> _userFeedbackRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IQueryExecutor _queryExecutor;

        public UserFeedbackQueryHandler(
            IRepository<UserFeedback> userFeedbackRepository,
            IRepository<StandardUser> standardUserRepository,
            IQueryExecutor queryExecutor)
        {
            _userFeedbackRepository = userFeedbackRepository;
            _standardUserRepository = standardUserRepository;
            _queryExecutor = queryExecutor;
        }

        public IEnumerable<UserFeedbackListModel> ExecuteQuery(UserFeedbackListQuery query)
        {
            var feedback = _userFeedbackRepository.List.Where(f =>
                    f.Deleted == false && f.DocumentId == query.DocumentId && f.DocumentType == query.DocumentType)
                .ToList();

            var userIds = feedback.Select(f => f.CreatedById).Distinct();
            var users = _standardUserRepository.List.Where(u => userIds.Contains(u.Id.ToString()));

            var result = feedback.Select(f => new UserFeedbackListModel
            {
                Id = f.Id,
                Content = f.Content,
                ContentType = f.ContentType,
                Created = f.Created,
                CreatedByModel = Project.StandardUserToUserModelShort.Compile()
                    .Invoke(users.FirstOrDefault(u => u.Id.ToString() == f.CreatedById)),
                DocumentId = f.DocumentId,
                DocumentType = f.DocumentType,
                Type = f.Type
            }).ToList();

            return result;
        }

        public UserFeedbackModel ExecuteQuery(FetchByIdQuery query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserFeedbackViewModelShort> ExecuteQuery(FilteredUserFeedbackQuery query)
        {
            var feedback = _userFeedbackRepository.List.Where(x => x.Deleted == false);

            if (!string.IsNullOrEmpty(query.UserId))
            {
                feedback = feedback.Where(f => f.CreatedById == query.UserId);
            }

            if (query.Documents?.Any() ?? false)
            {
                feedback = feedback.Where(f =>
                    query.Documents.Any(d => d.DocumentId == f.DocumentId) && query.Documents.FirstOrDefault(d => d.DocumentId == f.DocumentId)?.DocumentType == f.DocumentType);
            }
            else if (query.DocumentTypes?.Any() ?? false)
            {
                feedback = feedback.Where(f => query.DocumentTypes.Any(t => t == f.DocumentType));
            }

            if (query.FeedbackTypes?.Any() ?? false)
            {
                feedback = feedback.Where(f => query.FeedbackTypes.Any(t => t == f.ContentType));
            }

            if (query.FromDate.HasValue)
            {
                query.FromDate = query.FromDate.AtBeginningOfDay();
                feedback = feedback.Where(f => f.Created >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                query.ToDate = query.ToDate.AtEndOfDay();
                feedback = feedback.Where(f => f.Created <= query.ToDate.Value);
            }

            feedback = feedback.OrderByDescending(f => f.Created);

            var userIds = feedback.Select(f => f.CreatedById).Distinct();
            var users = _standardUserRepository.List.Where(u => userIds.Any(id => id == u.Id.ToString()) && u.IsActive).AsQueryable().Select(Project.StandardUserToUserModelShort_Firstname_LastNames).ToDictionary(x => x.Id.ToString());

            var documents = feedback.Select(f => new {DocumentType = f.DocumentType, DocumentId = f.DocumentId})
                .Distinct()
                .Select(f => _queryExecutor.Execute<DocumentQuery, DocumentListModel>(
                    new DocumentQuery
                    {
                        Id = f.DocumentId,
                        DocumentType = f.DocumentType
                    })).Where(c=>c.IsGlobalAccessed==query.IsGlobalAccess).ToDictionary(d => d.Id);

            var userFeedbackViewModels = new List<UserFeedbackViewModelShort>();
            feedback.ForEach(f =>
            {
                if (documents.ContainsKey(f.DocumentId))
                {
                    userFeedbackViewModels.Add(new UserFeedbackViewModelShort
                    {
                        Id = f.Id,
                        Created = f.Created,
                        Subject =
                            $"{VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(f.DocumentType)} Feedback - {f.ContentType.ToString()} from {(users.ContainsKey(f.CreatedById) ? users[f.CreatedById].Name : "Deleted")}",
                        ContentType = f.ContentType,
                        Content = f.Content,
                        User = users.ContainsKey(f.CreatedById)
                            ? users[f.CreatedById]
                            : new UserModelShort
                            {
                                Id = (Guid) f.CreatedById.ConvertToGuid(),
                                Name = "Deleted",
                            },
                        DocumentId = f.DocumentId,
                        DocumentType = f.DocumentType,
                        DocumentTitle = documents[f.DocumentId].Title
                    });
                }
            });

            if (!string.IsNullOrEmpty(query.Text))
            {
                return userFeedbackViewModels.Where(uf => uf.Subject.ToLower().Contains(query.Text.ToLower()));
            }

            return userFeedbackViewModels;
        }
    }
}