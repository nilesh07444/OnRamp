using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel {
	public class CopyDocumentsFromCustomerCompanyViewModel
    {
        public IEnumerable<MemoModel> Memos { get; set; }
        public IEnumerable<AcrobatFieldModel> AcrobatField { get; set; }
       public IEnumerable<PolicyModel> Policies { get; set; }
        public IEnumerable<TestModel> Tests { get; set; }
        public IEnumerable<TrainingManualModel> TrainingManuals { get; set; }
		public IEnumerable<CheckListModel> CheckLists { get; set; }

        #region This block is modified by Softude -- Shivam Sharma
        /// <summary>
        /// Added Custom Document Model For Cloning Functionality
        /// </summary>
        public IEnumerable<CustomDocumentModel> CustomDocument { get; set; }
        #endregion

    }
}
