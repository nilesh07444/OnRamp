using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.ExternalTrainingProvider;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ramp.Services.Projection;
using Common;
using VirtuaCon.Reporting;
using Common.Collections;

namespace Ramp.Services.QueryHandler {
	public class ExternalTrainingProviderQueryHandler : ReportingQueryHandler<ExternalTrainingProviderListQuery>,
														IQueryHandler<ExternalTrainingProviderListQuery, IEnumerable<ExternalTrainingProviderListModel>>,
														IQueryHandler<ExternalTrainingProviderListQuery, IPaged<ExternalTrainingProviderListModel>>,
														IQueryHandler<FetchAllRecordsQuery, List<ExternalTrainingProviderListModel>>,
														IQueryHandler<FetchByIdQuery, ExternalTrainingProviderModel>,
														IQueryHandler<ExternalTrainingProviderListQuery, IEnumerable<ExternalTrainingProvider>>,
														IQueryHandler<ExternalTrainingProviderListQuery, ExternalTrainingProviderReportModel>,
		IQueryHandler<ExternalTrainingProviderQueryParameter, ExternalTrainingProviderModel> {
		private readonly IRepository<ExternalTrainingProvider> _repository;
		private readonly IQueryExecutor _executor;
		public ExternalTrainingProviderQueryHandler(IRepository<ExternalTrainingProvider> repository,
			IQueryExecutor executor) {
			_repository = repository;
			_executor = executor;
		}

		public override void BuildReport(ReportDocument document, out string title,out string recepitent, ExternalTrainingProviderListQuery query) {
			title = "External Training Provider Report";
			recepitent = "";
			var data = _executor.Execute<ExternalTrainingProviderListQuery, ExternalTrainingProviderReportModel>(query);
			var columns = new List<Tuple<string, int>>
			{
				new Tuple<string, int>("Company Name",40),
				new Tuple<string, int>("Address",40),
				new Tuple<string, int>("Contact Number",50),
				new Tuple<string, int>("Contact Person",40),
				new Tuple<string, int>("Email",40),
				new Tuple<string, int>("BEE Status",20),
				new Tuple<string, int>($"BEE Certificate For {DateTime.Today.Year}",60)
			};
			var section = base.CreateSection(title);
			var grid = base.CreateGrid();
			base.CreateTableHeader(grid, columns.ToArray());
			foreach (var x in data.FilteredResults) {
				base.CreateTableDataRow(grid,
					x.CompanyName,
					x.Address,
					x.ContactNumber,
					x.ContactPerson,
					x.EmailAddress,
					x.BEEStatusLevel,
					x.BEECertificates.FirstOrDefault(r => r.Year == DateTime.Today.Year)?.Upload?.Description);
			}
			section.AddElement(grid);
			document.AddElement(section);
		}

		public new IEnumerable<ExternalTrainingProviderListModel> ExecuteQuery(ExternalTrainingProviderListQuery query) {
			var data = _repository.List.AsQueryable();
			var externalTrainingProvider = new List<ExternalTrainingProviderListModel>();
			var trainingProvidersPresentOrNotPresent = new List<ExternalTrainingProviderListModel>();
			var levelList = new List<string>();
			var certificateList = new List<string>();

			if (query.ExternalTrainingFilter != null && query.ExternalTrainingFilter.Count() != 0) {
				foreach (var filter in query.ExternalTrainingFilter) {
					if (filter == "Certificate uploaded" || filter == "Certificate not uploaded") {
						certificateList.Add(filter);
					} else {
						levelList.Add(filter);
					}
				}
			}
			if (levelList.Count >= 1)
				data = data.Where(c => levelList.Contains(c.BEEStatusLevel));
			if (certificateList.Count == 2) {
				data = data.Where(c => c.CertificateUploadId == null || c.CertificateUploadId != null);
			} else if (certificateList.Contains("Certificate uploaded")) {
				data = data.Where(c => c.CertificateUploadId != null);
			} else if (certificateList.Contains("Certificate not uploaded")) {
				data = data.Where(c => c.CertificateUploadId == null);
			}
			if (!string.IsNullOrEmpty(query.SearchText)) {
				var search = query.SearchText.ToLower();
				data = data.Where(c => c.CompanyName.ToLower().Contains(search));
			}
			return data.OrderBy(x => x.CompanyName).Select(Project.ToExternalTrainingProviderListModel);
		}

		public ExternalTrainingProviderModel ExecuteQuery(FetchByIdQuery query) {
			if (string.IsNullOrWhiteSpace(query.Id?.ToString())) { return new ExternalTrainingProviderModel(); }
			var entry = _repository.Find(Guid.Parse(query.Id.ToString()));
			if (entry != null)
				return Project.ToExternalTrainingProviderModel.Compile().Invoke(entry);
			return null;
		}
		public List<ExternalTrainingProviderListModel> ExecuteQuery(FetchAllRecordsQuery query) {
			var externalTrainingProviders = _repository.List.AsQueryable().Select(Project.ToExternalTrainingProviderListModel).ToList();
			return externalTrainingProviders;
		}

		IEnumerable<ExternalTrainingProvider> IQueryHandler<ExternalTrainingProviderListQuery, IEnumerable<ExternalTrainingProvider>>.ExecuteQuery(ExternalTrainingProviderListQuery query) {
			return _repository.List.AsQueryable().Where(x => query.CompanyNames.Contains(x.CompanyName));
		}
		ExternalTrainingProviderReportModel IQueryHandler<ExternalTrainingProviderListQuery, ExternalTrainingProviderReportModel>.ExecuteQuery(ExternalTrainingProviderListQuery query) {
			var entries = _repository.List.AsQueryable();
			if (query.ShowOnlyCompaniesWithMissingCertificates.HasValue &&
				query.ShowOnlyCompaniesWithMissingCertificates.Value)
				entries = entries.Where(x => !x.BEECertificates.Any(r => r.Year.HasValue && r.Year.Value == DateTime.Today.Year));
			if (query.CompanyNames.Any())
				entries = entries.Where(x => query.CompanyNames.Contains(x.CompanyName));
			var result = entries.Select(Project.ToExternalTrainingProviderModel).ToList();
			result.ForEach(PostProcess);
			return new ExternalTrainingProviderReportModel {
				FilteredResults = result
			};
		}

		IPaged<ExternalTrainingProviderListModel> IQueryHandler<ExternalTrainingProviderListQuery, IPaged<ExternalTrainingProviderListModel>>.ExecuteQuery(ExternalTrainingProviderListQuery query) {
			return query.GetPagedWithoutProjection(_executor.Execute<ExternalTrainingProviderListQuery, IEnumerable<ExternalTrainingProviderListModel>>(query));
		}

		void PostProcess(ExternalTrainingProviderModel model) {
			model.BEECertificates =
				_executor.Execute<FetchCertificatesForProviderQuery, IEnumerable<BEECertificateModel>>(
					new FetchCertificatesForProviderQuery { ProviderId = model.Id, Year = DateTime.Today.Year }).ToList();
		}

		public ExternalTrainingProviderModel ExecuteQuery(ExternalTrainingProviderQueryParameter query) {
			var externalTrainingProvider = _repository.List.FirstOrDefault(u => u.EmialAddress.Equals(query.Email));

			if (externalTrainingProvider != null) {
				if (!string.IsNullOrEmpty(query.Id)) {
					var externalTraining = _repository.List.FirstOrDefault(u => u.Id == Guid.Parse(query.Id));
					if (externalTraining != null && externalTrainingProvider.EmialAddress != externalTraining.EmialAddress)
						return Project.ToExternalTrainingProviderModel.Compile().Invoke(externalTrainingProvider);
					else if (externalTraining != null && externalTrainingProvider.EmialAddress == externalTraining.EmialAddress)
						return null;
					else
						return Project.ToExternalTrainingProviderModel.Compile().Invoke(externalTrainingProvider);
				} else {
					return Project.ToExternalTrainingProviderModel.Compile().Invoke(externalTrainingProvider);
				}

			}
			return null;
		}
	}
}
namespace Ramp.Services.Projection {
	public static partial class Project {
		public static readonly Expression<Func<ExternalTrainingProvider, ExternalTrainingProviderListModel>> ToExternalTrainingProviderListModel
			= x => new ExternalTrainingProviderListModel {
				BEEStatusLevel = x.BEEStatusLevel,
				CompanyName = x.CompanyName,
				ContactNumber = x.ContactNumber,
				ContactPerson = x.ContactPerson,
				Id = x.Id.ToString(),
				MobileNumber = x.MobileNumber,
				EmailAddress = x.EmialAddress,
				CertificateUploadId = x.CertificateUploadId
			};
		public static readonly Expression<Func<ExternalTrainingProvider, ExternalTrainingProviderModel>> ToExternalTrainingProviderModel
			= x => new ExternalTrainingProviderModel {
				MobileNumber = x.MobileNumber,
				Address = x.Address,
				BEEStatusLevel = x.BEEStatusLevel,
				CompanyName = x.CompanyName,
				ContactNumber = x.ContactNumber,
				ContactPerson = x.ContactPerson,
				EmailAddress = x.EmialAddress,
				CertificateUploadId = x.CertificateUploadId,
				Id = x.Id.ToString()
			};
	}
}

