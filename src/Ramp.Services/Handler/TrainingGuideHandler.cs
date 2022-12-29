using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.Query.TrainingGuide;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Handler
{
    public class TrainingGuideHandler : IQueryHandler<TrainingGuideListQuery, IEnumerable<TrainingGuideListModel>>
    {
        private readonly IRepository<TrainingGuide> _guideRepository;
        public TrainingGuideHandler(IRepository<TrainingGuide> guideRepository)
        {
            _guideRepository = guideRepository;
        }
        public IEnumerable<TrainingGuideListModel> ExecuteQuery(TrainingGuideListQuery query)
        {
            var guides = _guideRepository.List.AsQueryable();
            if (query.OnlyActive)
                guides = guides.Where(c => c.IsActive);
            if (query.CollaboratorId.HasValue)
                guides = guides.Where(t => t.Collaborators.Any(c => c.Id.Equals(query.CollaboratorId.Value)));
            return guides.Select(Projection.ToListModel).AsEnumerable();
        }
    }
    public static partial class Projection
    {
        public static readonly Expression<Func<TrainingGuide, TrainingGuideListModel>> ToListModel
            = x => new TrainingGuideListModel
            {
                Description = x.Description,
                HasTest = x.TestVersion.CurrentVersion == null ? false : true,
                Id = x.Id,
                LastEditDate = x.LastEditDate,
                Published = x.IsActive,
                ReferenceId = x.ReferenceId,
                Title = x.Title
            };
    }
}
