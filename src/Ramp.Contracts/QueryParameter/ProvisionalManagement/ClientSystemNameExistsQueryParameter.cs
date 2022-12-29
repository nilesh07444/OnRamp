using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class ClientSystemNameExistsQueryParameter : IQuery
    {
        public string ClientSystemName { get; set; }
    }
}