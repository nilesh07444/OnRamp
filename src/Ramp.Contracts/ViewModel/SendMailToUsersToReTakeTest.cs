using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class SendMailToUsersToReTakeTest : IViewModel
    {
        public SendMailToUsersToReTakeTest()
        {
            Users = new List<UserViewModel>();
        }
        public List<UserViewModel> Users { get; set; }
        public string TestTitle { get; set; }
        public string TrainingGuideTitle { get; set; }
    }
}