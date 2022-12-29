using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class AddSurveyDetailsByCustomerUserCommandHandler :
        CommandHandlerBase<AddSurveyDetailsByCustomerUserCommand>
    {
        private readonly IRepository<CustomerSurveyDetail> _customerSurveyDetailRepository;

        public AddSurveyDetailsByCustomerUserCommandHandler(IRepository<CustomerSurveyDetail> customerSurveyDetailRepository)
        {
            _customerSurveyDetailRepository = customerSurveyDetailRepository;
        }

        public override CommandResponse Execute(AddSurveyDetailsByCustomerUserCommand command)
        {
            var model = new CustomerSurveyDetail
            {
                Id = Guid.NewGuid(),
                Rating = command.Rating,
                RatedOn = DateTime.Now,
                UserId = command.CurrentUserId,
                Comment = command.Comment,
                Browser = command.Browser,
				Category = command.Category
            };
            _customerSurveyDetailRepository.Add(model);
            _customerSurveyDetailRepository.SaveChanges();
            return null;
        }
    }
}