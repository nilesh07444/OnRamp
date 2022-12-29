using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class GroupViewModelLong
    {
        public GroupViewModelLong()
        {
            GroupList = new List<GroupViewModel>();
        }
        public IEnumerable<SerializableSelectListItem> DropDownForProvisionalCompanies { get; set; }
        public IEnumerable<SerializableSelectListItem> DropDownForCustomerCompanies { get; set; }
        public List<GroupViewModel> GroupList { get; set; }
        public GroupViewModel GroupViewModel { get; set; }
    }
}