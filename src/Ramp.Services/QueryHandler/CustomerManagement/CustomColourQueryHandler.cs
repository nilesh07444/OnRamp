using System;
using Common.Query;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Common.Data;
using System.Linq;
using Domain.Models;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class CustomColourQueryHandler :
        QueryHandlerBase<CustomColourQuery, CustomColourViewModel>
    {
        private readonly IRepository<Company> _repository;

        public CustomColourQueryHandler(IRepository<Company> repository)
        {
            _repository = repository;
        }
        public override CustomColourViewModel ExecuteQuery(CustomColourQuery query)
        {
            var customColours = _repository.Find(query.CompanyId)?.CustomColours;

            if (customColours == null)
                return new CustomColourViewModel
                {
                    ButtonColour = "#27B899",
                    FeedbackColour = "#27B899",
                    FooterColour = "#27B899",
                    HeaderColour = "#27B899",
                    LoginColour = "#FFFFFF",
                    NavigationColour = "#27B899",
                    SearchColour = "#27B899"
				};
            else
            {
                return new CustomColourViewModel
                {
                    ButtonColour = customColours.ButtonColour,
                    FeedbackColour = customColours.FeedbackColour,
                    FooterColour = customColours.FooterColour,
                    HeaderColour = customColours.HeaderColour,
                    LoginColour = customColours.LoginColour,
                    NavigationColour = customColours.NavigationColour,
                    SearchColour = customColours.SearchColour

				};
            }
        }
    }
}
