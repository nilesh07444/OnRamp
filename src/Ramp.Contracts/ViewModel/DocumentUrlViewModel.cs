using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class DocumentUrlViewModel 
	{
		public int Id { get; set; }
		public string DocumentId { get; set; }
		public string Url { get; set; }
		public string ChapterId { get; set; }
		public string Name { get; set; }
	}
}
