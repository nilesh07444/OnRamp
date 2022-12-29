using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class TestNotApperUsersViewModel : IViewModel
    {
        public TestNotApperUsersViewModel()
        {
            UserList = new List<UserViewModel>();
            TestList = new List<TestViewModel>();
        }

        public IEnumerable<SerializableSelectListItem> DropDownForTest { get; set; }
        public IList<UserViewModel> UserList { get; set; }
        public IList<TestViewModel> TestList { get; set; }
        public Guid SelectedTest { get; set; }

    }
}
