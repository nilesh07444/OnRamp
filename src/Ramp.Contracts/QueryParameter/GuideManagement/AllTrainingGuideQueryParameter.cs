using Common.Query;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class AllTrainingGuideQueryParameter : IQuery
    {
        public Guid? Id { get; set; }
        public String TrainingGuideReferenceId { get; set; }
        public String Title { get; set; }
        public bool ShowPublishedRecords { get; set; }
        public bool OnlyActive { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid CompanyId { get; set; }
        public string PathOfSavedUploadedDocs { get; set; }
        public string TempCompanyName { get; set; }
    }
}