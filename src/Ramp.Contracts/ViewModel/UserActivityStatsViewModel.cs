using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ramp.Contracts.ViewModel
{
    public class UserActivityStatsViewModel : IViewModel
    {
        public UserActivityStatsViewModel()
        {
            UserList = new List<UserModelShort>();
            CustomerCompanyList = new List<CompanyModelShort>();
            ProvisionalCompanyList = new List<CompanyModelShort>();
        }

        public IEnumerable<SerializableSelectListItem> UserDropDown{ get; set; }
        public IEnumerable<SerializableSelectListItem> CustomerCompanyDropDown{ get; set; }
        public IEnumerable<SerializableSelectListItem> ProvisionalCompanyDropDown{ get; set; }

        public List<UserModelShort> UserList { get; set; }
        public List<CompanyModelShort> CustomerCompanyList { get; set; }
        public List<CompanyModelShort> ProvisionalCompanyList { get; set; }
        public IEnumerable<GroupViewModelShort> Groups { get; set; }
        public IEnumerable<UserModelShort> Users { get; set; }

        public Guid SelectedUserId { get; set; }
        public Guid SelectedCustomerCompanyId { get; set; }
        public Guid SelectedProvisionalCompanyId { get; set; }
        public bool IsMonthly { get; set;}
        public bool IsYearly { get; set; }

        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter valid number.")]
        public int ExpiringInXdays { get; set; }
    }
}