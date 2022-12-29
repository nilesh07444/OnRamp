using Common.Query;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class CheckForTrainingTestTitleExistQueryParameter : IQuery
    {
        public string TestName { get; set; }
    }
}