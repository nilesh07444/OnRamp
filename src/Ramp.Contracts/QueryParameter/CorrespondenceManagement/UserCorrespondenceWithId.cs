using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CorrespondenceManagement
{
    public class UserCorrespondenceWithIdQueryParameter : IQuery
    {
        public Guid Id { get; set; }
    }
}