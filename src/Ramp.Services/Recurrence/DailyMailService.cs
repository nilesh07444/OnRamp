using Common.Command;
using Common.Events;
using Common.Query;
using Common.RecurringJob;
using Data.EF;
using Data.EF.Customer;
using Domain.Models;
using Hangfire;
using Ramp.Contracts.Events.Document;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace Ramp.Services.Recurrence {
	public class DailyMailService : IRecurringJob {
		public string When => Cron.Daily(17, 00);

		private readonly ICommandDispatcher _dispacther;
		private readonly IQueryExecutor _executor;
		private CustomerContext _context;

		public DailyMailService() {
			_dispacther = new CommandDispatcher();
			_executor = new QueryExecutor();
		}

		public void Work() {
		//	SendEmails();
		}

		public void SendEmails() {
			var companies = new MainContext().Set<Company>().AsQueryable().Where(x => x.CompanyType == Domain.Enums.CompanyType.CustomerCompany);
			companies.ToList().ForEach(delegate (Company company) {
				string companyConnStringOld = ConfigurationManager.ConnectionStrings["CustomerContext"].ConnectionString;
				string conString = companyConnStringOld.Replace("DBNAME", "" + company.CompanyName.Replace(" ", string.Empty));
				_context = new CustomerContext(conString);
				DateTime dt = DateTime.Now.Date;
				var documents = _context.AssignedDocuments.Where(c => DbFunctions.TruncateTime(c.AssignedDate) == dt).ToList();
				foreach (var item in documents) {
					var userDocument = _executor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery {
						Id = item.DocumentId,
						DocumentType = item.DocumentType
					});

					var user = _executor.Execute<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery {
						Id = item.UserId
					});


					var documentTitles = _executor.Execute<DocumentTitlesQuery, IEnumerable<DocumentTitlesAndTypeQuery>>(
				   new DocumentTitlesQuery {
					   Identifiers = documents.Select(x =>
						   new DocumentIdentifier { DocumentId = item.DocumentId, DocumentType = item.DocumentType, AdditionalMsg=item.AdditionalMsg })
				   }).ToList();

					var eventPublisher = new EventPublisher();
					eventPublisher.Publish(new DocumentsAssignedEvent {
						UserViewModel = new UserViewModel() { FirstName = user.Name, EmailAddress = user.Email },
						CompanyViewModel = new CompanyViewModel(),
						DocumentTitles = documentTitles,
						Subject = $"{documentTitles.Count()} Document{(documentTitles.Count() > 1 ? "s" : "")} Assigned"
					});
				}
			});


		}
	}

	public class NotificationModel {
		public string FirstName { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string FooterUrl { get; set; }
	}
	public class MainContextModel {
		public string Company { get; set; }
		public string Domain { get; set; }
		public string LogoImageUrl { get; set; }
	}

	public class NotificationDetailModel {

		public string UserId { get; set; }
		public string DocumentId { get; set; }
		public int DocumentType { get; set; }
		public DateTime AssignedDate { get; set; }
		public string FirstName { get; set; }
		public string EmailAddress { get; set; }
	}
}
