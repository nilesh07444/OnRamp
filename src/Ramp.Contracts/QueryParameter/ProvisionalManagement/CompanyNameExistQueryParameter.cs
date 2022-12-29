using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class CompanyNameExistQueryParameter : IQuery
    {
        public string CompanyName { get; set; }
    }
}