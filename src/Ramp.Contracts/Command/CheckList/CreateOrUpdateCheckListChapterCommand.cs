using Common.Data;

namespace Ramp.Contracts.Command.CheckList {
	public class CreateOrUpdateCheckListChapterCommand : IdentityModel<string> {

		public string CheckListId { get; set; }
		public string Title { get; set; }
		public int Number { get; set; }
		public string Content { get; set; }
		public bool Deleted { get; set; }
		public bool AttachmentRequired { get; set; }
		public bool IsChecked { get; set; }
	
	}
}
