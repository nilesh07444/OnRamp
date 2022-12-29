using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class AllTrainingGuideForMasterAdminQueryParameter : IQuery
    {
        public Guid? Id { get; set; }
        public bool ShowPublishedRecords { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
    }
}