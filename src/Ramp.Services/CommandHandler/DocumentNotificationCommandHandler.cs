using Common.Command;
using Common.Data;
using Data.EF.Customer;
using Domain.Customer.Models.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
	public class DocumentNotificationCommandHandler : ICommandHandlerBase<List<DocumentNotificationViewModel>>
	{
		private readonly IRepository<DocumentNotification> _documentNotificationRepository;
		private readonly CustomerContext _context;

		public DocumentNotificationCommandHandler(IRepository<DocumentNotification> documentNotificationRepository,
			CustomerContext context)
		{
			_documentNotificationRepository = documentNotificationRepository;
			_context = context;
		}

		public CommandResponse Execute(List<DocumentNotificationViewModel> command)
		{
			try
			{
				var response = new CommandResponse();
				foreach (var notification in command)
				{
					var vm = new DocumentNotification()
					{
						Id = notification.Id,
						AssignedDate = DateTime.Now,
						DocumentId = notification.DocId,
						UserId = notification.UserId,
						IsViewed = notification.IsViewed,
						Message = notification.Message,
						NotificationType = notification.NotificationType
					};
					if (vm.IsViewed && vm.Id > 0)
					{
						var entity = _documentNotificationRepository.Find(vm.Id);
						if (entity != null)
						{
							_context.Entry(entity).CurrentValues.SetValues(vm);
						}
					}
					else
					{
						var list = _documentNotificationRepository.List.Where(x => x.DocumentId == notification.DocId && x.UserId == notification.UserId).ToList();
						foreach (var obj in list)
						{
							_documentNotificationRepository.Delete(obj);
						}
						if (list.Count <= 0)
						{
							_documentNotificationRepository.Add(vm);
						}
					}
				}
				_documentNotificationRepository.SaveChanges();
				return response;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
