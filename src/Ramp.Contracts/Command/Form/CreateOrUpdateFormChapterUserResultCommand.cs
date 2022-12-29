
//new file added by softude

using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.Form
{
    public class CreateOrUpdateFormChapterUserResultCommand : IdentityModel<string>
    {
		public string AssignedDocumentId { get; set; }
		public string FormChapterId { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ChapterTrackedDate { get; set; }
		public string UserId { get; set; }
	}
}
