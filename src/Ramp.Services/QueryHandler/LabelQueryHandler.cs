using Common.Query;
using Ramp.Contracts.Query.Label;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;
using Common.Data;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System.Linq.Expressions;
using Data.EF.Customer;
using Common.Collections;
using Common;

namespace Ramp.Services.QueryHandler
{
    public class LabelQueryHandler : IQueryHandler<LabelListQuery, IEnumerable<Label>>,
                                     IQueryHandler<LabelListQuery,IEnumerable<TrainingLabelListModel>>,
                                     IQueryHandler<LabelListQuery,IPaged<TrainingLabelListModel>>,
                                     IQueryHandler<FetchByIdQuery, TrainingLabelModel>,
                                     IQueryHandler<FetchByNameQuery, TrainingLabelModel>,
									IQueryHandler<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>
    {
        readonly ITransientReadRepository<Label> _repository;
        public LabelQueryHandler(ITransientReadRepository<Label> repository)
        {
            _repository = repository;
        }
        public IEnumerable<Label> ExecuteQuery(LabelListQuery query)
        {
            return _repository.List.AsQueryable().Where(x => !x.Deleted && query.Values.Contains(x.Name)).ToList();
        }

		public IEnumerable<TrainingLabelListModel> ExecuteQuery(TrainingLabelListQuery query) {
			var labels= _repository.List.AsQueryable().Where(x => !x.Deleted).Select(Project.Label_TrainingLabelListModel).ToList();
			return labels;
		}


		public TrainingLabelModel ExecuteQuery(FetchByIdQuery query)
        {
            var e = _repository.Find(query.Id?.ToString());
            if (e != null && !e.Deleted)
                return Project.Label_TrainingLabelModel.Compile().Invoke(e);
            return new TrainingLabelModel();
        }
		public TrainingLabelModel ExecuteQuery(FetchByNameQuery query)
        {
            var e = _repository.List.Where(c=>c.Name.Equals(query.Name)).FirstOrDefault();
            if (e != null && !e.Deleted)
                return Project.Label_TrainingLabelModel.Compile().Invoke(e);
            return new TrainingLabelModel();
        }

        IEnumerable<TrainingLabelListModel> IQueryHandler<LabelListQuery, IEnumerable<TrainingLabelListModel>>.ExecuteQuery(LabelListQuery query)
        {
            return _repository.List.AsQueryable().Where(x => !x.Deleted).Select(Project.Label_TrainingLabelListModel).ToList();
        }

		IPaged<TrainingLabelListModel> IQueryHandler<LabelListQuery, IPaged<TrainingLabelListModel>>.ExecuteQuery(LabelListQuery query) {
			return query.GetPagedWithoutProjection(_repository.List.AsQueryable().Where(x => !x.Deleted).Select(Project.Label_TrainingLabelListModel).ToList());
		}
	}
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<Label, TrainingLabelListModel>> Label_TrainingLabelListModel =
            x => new TrainingLabelListModel
            {
                Description = x.Description,
                Id = x.Id,
                Name = x.Name
            };
        public static readonly Expression<Func<Label, TrainingLabelModel>> Label_TrainingLabelModel =
           x => new TrainingLabelModel
           {
               Description = x.Description,
               Id = x.Id,
               Name = x.Name
           };
    }
}
