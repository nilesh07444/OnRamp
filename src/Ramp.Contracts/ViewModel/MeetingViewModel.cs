using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel {
	public class MeetingViewModel {
		public MeetingViewModel() {
			VirtualClassrooms = new List<VirtualClassModel>();
			Paginate = new Paginate();
		}
		public List<VirtualClassModel> VirtualClassrooms { get; set; }
		public Paginate Paginate { get; set; }
	}
}
