using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class LayerSubDomainExistsQueryParameter : IQuery
    {
        public string LayerSubDomainName { get; set; }
    }
}