using Domain.Customer.Models.Feedback;

namespace Ramp.Contracts.ViewModel
{
    public class UserFeedbackViewModel
    {
        public UserFeedback UserFeedback { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public UserModelShort Collaborator { get; set; }
        public DocumentListModel DocumentListModel { get; set; }
    }
}