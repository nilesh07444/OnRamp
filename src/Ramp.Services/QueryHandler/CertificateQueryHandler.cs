using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Data;
using Domain.Customer.Models;
using java.net;
using Ramp.Contracts.QueryParameter.Certificates;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using VirtuaCon;
using LinqKit;
using Domain.Customer;
using Common.Collections;
using Ramp.Contracts.Query.Document;

namespace Ramp.Services.QueryHandler
{
    public class CertificateQueryHandler :  IQueryHandler<CertificateListQuery, IEnumerable<CertificateListModel>>,
											IQueryHandler<CertificateListQuery, IPaged<CertificateListModel>>,
											IQueryHandler<FetchByIdQuery, CertificateModel>,
											IQueryHandler<FetchByIdQuery,Certificate>,
                                            IQueryHandler<CertificateListQuery,IEnumerable<UploadResultViewModel>>
    {
        private readonly IRepository<Certificate> _repository;
		private readonly IQueryExecutor _queryDispatcher;
		public CertificateQueryHandler(IQueryExecutor queryDispatcher, IRepository<Certificate> repository)
        {
			_queryDispatcher = queryDispatcher;
			_repository = repository;
        }

        public IEnumerable<CertificateListModel> ExecuteQuery(CertificateListQuery query)
        {
            var certificates = _repository.List.AsQueryable().Where(x => x.Upload != null && !x.Upload.Deleted).ToList();

			if (!string.IsNullOrWhiteSpace(query.MatchText)) {

				//(from u in certificates.AsEnumerable()
				// where u.Title.Contains(query.MatchText)
				// select new CertificateListModel {
				//	 Title = u.Title
				// }).ToList();

				certificates = certificates.Where(x => x.Title.Contains(query.MatchText)
				).ToList();



			}
			if (!string.IsNullOrWhiteSpace(query.SortingOrder)) {
				string orderedProperty = null;
				var sortOrder = query.SortingOrder.String_SortingOrder(out orderedProperty);
				if (sortOrder.HasValue) {
					switch (orderedProperty) {
						case "Type":
							certificates = sortOrder.Value == SortOrder.Descending ? certificates.OrderByDescending(x => x.Type).ToList() : certificates.OrderBy(x => x.Type).ToList();
							break;
						case "Title":
							certificates = sortOrder.Value == SortOrder.Descending ? certificates.OrderByDescending(x => x.Title).ToList() : certificates.OrderBy(x => x.Title).ToList();
							break;
						case "CreatedOn":
							certificates = sortOrder.Value == SortOrder.Descending ? certificates.OrderByDescending(x => x.CreatedOn).ToList() : certificates.OrderBy(x => x.CreatedOn).ToList();
							break;
						default:
							certificates.OrderBy(x => x.Id).ToArray();
							break;
					}
				}
			}
			//var @default = certificates.First(x => x.Upload.Description == "Default");

			//         certificates.Remove(@default);
			// var ordered = certificates.OrderBy(x => x.Upload.Description).ToList();

			//ordered.Insert(0, @default);

			return certificates.AsQueryable().Select(Project.ToCertificateListModel).ToList();
        }

        public CertificateModel ExecuteQuery(FetchByIdQuery query)
        {
            return query.Id == null
                ? null
                : Project.ToCertificateModel.Compile().Invoke(_repository.Find(query.Id));
        }

		Certificate IQueryHandler<FetchByIdQuery, Certificate>.ExecuteQuery(FetchByIdQuery query)
        {
            return query.Id == null ? null : _repository.List.AsQueryable().FirstOrDefault(x => x.UploadId == query.Id.ToString() && x.Upload != null && !x.Upload.Deleted);
        }

		IEnumerable<UploadResultViewModel> IQueryHandler<CertificateListQuery, IEnumerable<UploadResultViewModel>>.ExecuteQuery(CertificateListQuery query)
        {
            return _repository.List.AsQueryable().Where(x => x.Upload != null && !x.Upload.Deleted).Select(Project.Certificate_UploadResultViewModel).ToList();
        }



		IPaged<CertificateListModel> IQueryHandler<CertificateListQuery, IPaged<CertificateListModel>>.ExecuteQuery(CertificateListQuery query) {
			return query.GetPagedWithoutProjection(_queryDispatcher.Execute<CertificateListQuery, IEnumerable<CertificateListModel>>(query));
		}
	}
}

namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<Certificate, CertificateModel>> ToCertificateModel
            = x => x != null && x.Upload != null && !x.Upload.Deleted ?
            new CertificateModel
            {
                Id = x.Id,
                Description = x.Upload.Description,
                Upload = Upload_UploadResultViewModel.Invoke(x.Upload),
                UploadId = x.UploadId,
				CreatedOn = x.CreatedOn,
				Title = x.Title,
				Type = x.Type
			} : null;

        public static readonly Expression<Func<Certificate, CertificateListModel>> ToCertificateListModel
            = x => x != null && x.Upload != null ?
            new CertificateListModel
            {
                Id = x.Id,
                Description = x.Upload.Description,
                UploadId = x.UploadId,
				CreatedOn = x.CreatedOn,
				Title = x.Title,
				Type = x.Type
            } : null;

        public static readonly Expression<Func<Certificate, UploadResultViewModel>> Certificate_UploadResultViewModel
            = x => x != null && x.Upload != null ?
            new UploadResultViewModel
            {
                Id = x.UploadId,
                Description = x.Upload.Description,
                Name = x.Upload.Name,
                Type = DocumentType.Certificate.ToString()
            } : null;
    }
}