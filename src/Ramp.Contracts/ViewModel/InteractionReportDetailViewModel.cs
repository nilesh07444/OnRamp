using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Customer;

namespace Ramp.Contracts.ViewModel
{
    public class InteractionReportDetailViewModel
    {
        public DateTime GeneratedDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentTitle { get; set; }
        public string PassRequirement { get; set; }
        public IList<string> Groups { get; set; }
        public IList<InteractionDetailModel> Interactions { get; set; }
		public IList<InteractionDetailModel> GlobalInteractions { get; set; }
		
		public class InteractionDetailModel
        {
			public string UserId { get; set; }
            public string Name { get; set; }
            public string IDNumber { get; set; }
            public string Group { get; set; }
            public string Status { get; set; }
			public DateTime ViewDate { get; set; }
			public string Result1 { get; set; }
            public string Result2 { get; set; }
            public string Result3 { get; set; }
            public string ResultId1 { get; set; }
            public string ResultId2 { get; set; }
            public string ResultId3 { get; set; }
			public TimeSpan Duration { get; set; }
			public DateTime DateCompleted { get; set; }
			public DateTime DateSubmitted { get; set; }
			public string ChecksCompleted { get; set; }
		}
    }
}