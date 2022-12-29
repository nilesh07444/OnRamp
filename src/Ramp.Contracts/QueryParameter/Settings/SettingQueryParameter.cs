using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.Settings
{
    public class SettingQueryParameter : IQuery
    {
        public Guid? Id { get; set; }
    }
}
