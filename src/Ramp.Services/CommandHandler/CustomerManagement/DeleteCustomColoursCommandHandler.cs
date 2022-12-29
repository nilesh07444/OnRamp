using System;
using Common.Command;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Common.Data;
using System.Linq;
using Domain.Models;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class DeleteCustomColoursCommandHandler :
        CommandHandlerBase<DeleteCustomColourCommand>
    {
        private readonly IRepository<Company> _repository;
        private readonly IRepository<CustomColour> _colorRepository;

        public DeleteCustomColoursCommandHandler(IRepository<Company> repository, IRepository<CustomColour> colorRepository)
        {
            _repository = repository;
            _colorRepository = colorRepository;
        }

        public override CommandResponse Execute(DeleteCustomColourCommand command)
        {
            var company = _repository.Find(command.CompanyId);
            var cc = company?.CustomColours;
            if (company != null && cc != null)
            {
                _colorRepository.Delete(cc);
                _colorRepository.SaveChanges();
                company.CustomColours = null;
            }
            _repository.SaveChanges();

            return null;
        }
    }
}
