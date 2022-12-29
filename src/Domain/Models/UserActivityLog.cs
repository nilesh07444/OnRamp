using System;
using System.ComponentModel;

namespace Domain.Models
{
    public class UserActivityLog : DomainObject
    {
        public UserActivityEnum ActivityType { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public DateTime ActivityDate { get; set; }
        public virtual User User { get; set; }
    }

    public enum UserActivityEnum
    {
        [Description("logged into the system")]
        Login = 0,

        [Description("logged out from the system")]
        Logout = 1,

        [Description("created customer user")]
        CreateCustomerUser = 2,

        [Description("created customer company")]
        CreateCustomerCompany = 3,

        [Description("deleted customer user")]
        DeleteCustomerUser = 4,

        [Description("deleted customer company")]
        DeleteCustomerCompany = 5,

        [Description("reset account password")]
        ResetPassword = 6,

        [Description("created play book")]
        CreateTrainingGuide = 7,

        [Description("updated play book")]
        EditTrainingGuide = 8,

        [Description("deleted play book")]
        DeleteTrainingGuide = 9,

        [Description("viewed play book")]
        ViewTrainingGuide = 10,

        [Description("created training test")]
        CreateTrainingTest = 11,

        [Description("updated training test")]
        EditTrainingTest = 12,

        [Description("deleted training test")]
        DeleteTrainingTest = 13,

        [Description("took training test")]
        TakeTest = 14,

        [Description("updated profile")]
        UpdateProfile = 15,

        [Description("updated company profile")]
        UpdateCompanyProfile = 16,

        [Description("updated profile")]
        UpdateUserProfile = 17,
    }
}