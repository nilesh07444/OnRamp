using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler {
	public class DocumentNotificationQueryHandler : IQueryHandler<DocumentNotification, IEnumerable<DocumentNotificationViewModel>> {
		private readonly ITransientRepository<DocumentNotification> _documentNotificationRepository;

		public DocumentNotificationQueryHandler(ITransientRepository<DocumentNotification> documentNotificationRepository) {
			_documentNotificationRepository = documentNotificationRepository;
		}
		public IEnumerable<DocumentNotificationViewModel> ExecuteQuery(DocumentNotification query) {
			var docNotifications = _documentNotificationRepository.List.Where(x => x.UserId == query.UserId)
				.Select(x => new DocumentNotificationViewModel {
					Id = x.Id,
					UserId = x.UserId,
					DocId = x.DocumentId,
					AssignedDate = x.AssignedDate,
					IsViewed = x.IsViewed,
					Message= x.Message,
					NotificationType = x.NotificationType
				});
			return docNotifications;
		}
	}
}
