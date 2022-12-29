using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class StandardUserGroupViewModel {

		public StandardUserGroupViewModel() {
			 GroupList = new List<StandardUserGroupModel>();
		}
		
		public Guid UserId { get; set; }
		public List<StandardUserGroupModel> GroupList { get; set; }

	}

	public class StandardUserGroupModel {

		public Guid? GroupId { get; set; }
		public string Title { get; set; }


	}
}
