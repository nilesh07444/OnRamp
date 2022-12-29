using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.Catagories
{
    public class AllCategoriesQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }
        public Guid ParentCategoryId { get; set; }
    }
}