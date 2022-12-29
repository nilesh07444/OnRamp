using System;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class AllTrainingGuideToCreateTestQueryParameter
    {
        public Guid? Id { get; set; }
        public bool ShowPublishedRecords { get; set; }
        public Guid CompanyId { get; set; }
        public bool ShowAllTrainingGuide { get; set; }
        public bool ShowTrainingGuideForEdit { get; set; }
        public Guid ColaboratorId { get; set; }
    }
}