using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class InteractionReportViewModel
    {
        public IList<InteractionModel> TrainingManualInteractions { get; set; }
        public IList<InteractionModel> MemoInteractions { get; set; }
        public IList<PolicyInteractionModel> PolicyInteractions { get; set; }
        public IList<TestInteractionModel> TestInteractions { get; set; }
		public IList<InteractionModel> CheckListInteractions { get; set; }
		public IList<InteractionModel> GlobalTrainingManualInteractions { get; set; }
        public IList<InteractionModel> GlobalMemoInteractions { get; set; }
        public IList<PolicyInteractionModel> GlobalPolicyInteractions { get; set; }
        public IList<TestInteractionModel> GlobalTestInteractions { get; set; }
		public IList<InteractionModel> GlobalCheckListInteractions { get; set; }
        public IList<InteractionModel> CustomDocumentsInteractions { get; set; } 

        public class InteractionModel
        {
            public string Title { get; set; }
            public int Allocated { get; set; }
            public int Interacted { get; set; }
            public int GloballyAccessed { get; set; }
			public string TrainingLabels { get; set; }
			public string DocumentId { get; set; }
			public int YetToInteract { get; set; }
		}

        public class PolicyInteractionModel
        {
			public string TrainingLabels { get; set; }
			public string Title { get; set; }
            public int Allocated { get; set; }
            public int ViewLater { get; set; }
            public int Accepted { get; set; }
            public int Rejected { get; set; }
			public int GloballyAccessed { get; set; }
			public string DocumentId { get; set; }
			public int YetToInteract { get; set; }
		}

        public class TestInteractionModel
        {
			public string TrainingLabels { get; set; }
			public string Title { get; set; }
            public int Allocated { get; set; }
            public int Passed { get; set; }
            public int Failed { get; set; }
			public int GloballyAccessed { get; set; }
			public string DocumentId { get; set; }
			public int YetToInteract { get; set; }
		}
    }
}