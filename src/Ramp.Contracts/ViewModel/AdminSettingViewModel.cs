using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class AdminSettingsViewModel : IViewModel {
		public AdminSettingsViewModel()
		{
			Company = new CompanyViewModel();
			AddCustomUserRole = new AddCustomUserRoleViewModel();
		}
		public CompanyViewModel Company { get; set; }
		public AddCustomUserRoleViewModel AddCustomUserRole { get; set; }
	}
}
