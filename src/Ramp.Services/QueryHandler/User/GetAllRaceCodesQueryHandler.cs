using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.User
{
    public class GetAllRaceCodesQueryHandler : IQueryHandler<GetAllRaceCodesQuery, List<RaceCodeViewModel>>
    {
        private readonly IRepository<RaceCodes> _raceCodeRepository;

        public GetAllRaceCodesQueryHandler(IRepository<RaceCodes> raceCodeRepository)
        {
            _raceCodeRepository = raceCodeRepository;
        }

        public List<RaceCodeViewModel> ExecuteQuery(GetAllRaceCodesQuery query)
        {
            return _raceCodeRepository.List.Select(x => new RaceCodeViewModel
            {
                Code = x.Code,
                Description = x.Description,
                Id = x.Id
            }).OrderBy(x => x.Description).ToList();
        }
    }
}