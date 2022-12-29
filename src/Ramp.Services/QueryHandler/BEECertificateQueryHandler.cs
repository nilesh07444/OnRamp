using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.Certificates;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Common.Data;
using Ramp.Services.Projection;
using Common;
using Ramp.Contracts.QueryParameter.ExternalTrainingProvider;
using Common.Collections;

namespace Ramp.Services.QueryHandler
{
    public class BEECertificateQueryHandler : IQueryHandler<BEECertificateListQuery, IEnumerable<BEECertificateListModel>>,
										      IQueryHandler<BEECertificateListQuery, IPaged<BEECertificateListModel>>,
											  IQueryHandler<FetchByIdQuery, BEECertificateModel>,
                                              IQueryHandler<FetchCertificatesForProviderQuery,IEnumerable<BEECertificateModel>>
    {
        private readonly IRepository<BEECertificate> _repository;
        private readonly IRepository<ExternalTrainingProvider> _externalTrainingProviderRepository;
		private readonly IQueryExecutor _executor;
		public BEECertificateQueryHandler(IRepository<BEECertificate> repository, IQueryExecutor executor, IRepository<ExternalTrainingProvider> externalTrainingProviderRepository)
        {
            _repository = repository;
			_executor = executor;
			 _externalTrainingProviderRepository = externalTrainingProviderRepository;
        }
        public IEnumerable<BEECertificateListModel> ExecuteQuery(BEECertificateListQuery query)
        {
            var result = new List<BEECertificateListModel>();
            var provider = _externalTrainingProviderRepository.Find(query.ExternalTrainingProviderId.ConvertToGuid());
            if (provider != null)
                result.AddRange(provider.BEECertificates.AsQueryable().Select(Project.ToBEECertifcateListModel));
            return result;
        }

        public BEECertificateModel ExecuteQuery(FetchByIdQuery query)
        {
            if (query.Id == null) { return new BEECertificateModel(); }
            var e = _repository.Find(query.Id);
            if (e == null)
                return null;
            return Project.ToBEECertifcateModel.Compile().Invoke(e);
        }

        public IEnumerable<BEECertificateModel> ExecuteQuery(FetchCertificatesForProviderQuery query)
        {
            var provider = _externalTrainingProviderRepository.Find(query.ProviderId.ConvertToGuid());
            IEnumerable<BEECertificateModel> result  = new List<BEECertificateModel>();
            if (provider != null)
            {
                if (query.Year.HasValue)
                    result = provider.BEECertificates.AsQueryable().Where(x => x.Year == query.Year.Value)
                        .Select(Project.ToBEECertifcateModel);
                else
                    result = provider.BEECertificates.AsQueryable().Select(Project.ToBEECertifcateModel);
            }
            return result;

        }

		IPaged<BEECertificateListModel> IQueryHandler<BEECertificateListQuery, IPaged<BEECertificateListModel>>.ExecuteQuery(BEECertificateListQuery query) {
			return query.GetPagedWithoutProjection(_executor.Execute<BEECertificateListQuery, IEnumerable<BEECertificateListModel>>(query));
		}
	}
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<BEECertificate, BEECertificateListModel>> ToBEECertifcateListModel
            = x => new BEECertificateListModel
            {
                Id = x.Id.ToString(),
                Year = x.Year
            };
        public static readonly Expression<Func<BEECertificate, BEECertificateModel>> ToBEECertifcateModel
            = x => new BEECertificateModel
            {
                Id = x.Id.ToString(),
                Year = x.Year,
                Upload = x.Upload == null ? null : ToFileUploadResultModel.Compile().Invoke(x.Upload)
            };
    }
}
