using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class StandardUserActivityLog : Base.CustomerDomainObject
    {
        public const string Login = "Login",
            Logout = "Logout",
            CreateCustomerUser = "CreateCustomerUser",
            DeleteCustomerUser = "DeleteCustomerUser",
            ResetPassword = "ResetPassword",
            CreateTrainingGuide = "CreateTrainingGuide",
            EditTrainingGuide = "EditTrainingGuide",
            DeleteTrainingGuide = "DeleteTrainingGuide",
            ViewTrainingGuide = "ViewTrainingGuide",
            CreateTrainingTest = "CreateTrainingTest",
            EditTrainingTest = "EditTrainingTest",
            DeleteTrainingTest = "DeleteTrainingTest",
            TakeTest = "TakeTest",
            UpdateProfile = "UpdateProfile",
            UpdateCompanyProfile = "UpdateCompanyProfile",
            UpdateUserProfile = "UpdateUserProfile",
            LegalDisclaimer="LegalDisclaimer";

        public const string LoginDesc = "logged into the system",
            LogoutDesc = "logged out from the system",
            CreateCustomerUserDesc = "created customer user",
            DeleteCustomerUserDesc = "deleted customer user",
            ResetPasswordDesc = "reset account password",
            CreateTrainingGuideDesc = "created playbook",
            EditTrainingGuideDesc = "updated playbook",
            DeleteTrainingGuideDesc = "deleted playbook",
            ViewTrainingGuideDesc = "viewed playbook",
            CreateTrainingTestDesc = "created training test",
            EditTrainingTestDesc = "updated training test",
            DeleteTrainingTestDesc = "deleted training test",
            TakeTestDesc = "took training test",
            UpdateProfileDesc = "updated profile",
            UpdateCompanyProfileDesc = "updated company profile",
            UpdateUserProfileDesc = "updated profile";

        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime ActivityDate { get; set; }
        public virtual StandardUser User { get; set; }
    }
}