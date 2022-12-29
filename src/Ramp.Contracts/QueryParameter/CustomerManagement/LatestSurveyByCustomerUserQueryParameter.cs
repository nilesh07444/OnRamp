using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class LatestSurveyByCustomerUserQueryParameter : IQuery
    {
        public Guid CurrentUserId { get; set; }
    }
}