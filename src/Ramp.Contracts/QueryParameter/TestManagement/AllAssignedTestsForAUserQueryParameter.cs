using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class AllAssignedTestsForAUserQueryParameter : IQuery
    {
        public Guid UserId { get; set; }
        public string CertificateUrlbase { get; set; }
        public bool InMyTests { get; set; }
    }
}