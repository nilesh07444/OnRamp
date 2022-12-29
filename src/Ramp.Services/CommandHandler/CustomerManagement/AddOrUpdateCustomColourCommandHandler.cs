using System;
using Common.Command;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Common.Data;
using System.Linq;
using Domain.Models;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class AddOrUpdateCustomColourCommandHandler :
        CommandHandlerBase<AddOrUpdateCustomColourCommand>
    {
        private readonly IRepository<Company> _repository;
        private readonly IRepository<CustomColour> _colorRepository;

        public AddOrUpdateCustomColourCommandHandler(IRepository<Company> repository, IRepository<CustomColour> colorRepositoryy)
        {
            _repository = repository;
            _colorRepository = colorRepositoryy;
        }

        public override CommandResponse Execute(AddOrUpdateCustomColourCommand command)
        {
            var company = _repository.Find(command.CompanyId);
            var customColour = company?.CustomColours;
            if (customColour == null)
            {
                customColour = new CustomColour();
                customColour.Id = Guid.NewGuid();
                customColour.Company = company;
                _colorRepository.Add(customColour);
            }

            customColour.ButtonColour = command.ButtonColour;
            customColour.FeedbackColour = command.FeedbackColour;
            customColour.FooterColour = command.FooterColour;
            customColour.HeaderColour = command.HeaderColour;
            customColour.LoginColour = command.LoginColour;
            customColour.NavigationColour = command.NavigationColour;
            customColour.SearchColour = command.SearchColour;
            _colorRepository.SaveChanges();
            _repository.SaveChanges();

            return null;
        }
    }
}
