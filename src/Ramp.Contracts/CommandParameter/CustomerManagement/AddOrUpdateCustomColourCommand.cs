using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class AddOrUpdateCustomColourCommand :
        ICommand
    {
        public Guid? CompanyId { get; set; }
        public string ButtonColour { get; set; }
        public string FeedbackColour { get; set; }
        public string FooterColour { get; set; }
        public string HeaderColour { get; set; }
        public string LoginColour { get; set; }
        public string NavigationColour { get; set; }
        public string SearchColour { get; set; }

    }
}
