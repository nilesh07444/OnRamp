using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class EditTrainingGuideQueryParameter : IQuery
    {
        public Guid? TrainigGuideId { get; set; }
        public Guid CompanyId { get; set; }
    }
}