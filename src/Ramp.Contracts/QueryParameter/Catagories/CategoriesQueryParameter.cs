using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.Catagories
{
    public class CategoriesQueryParameter : IQuery
    {
        public Guid? Id { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
        public Guid ParentCategoryId { get; set; }
    }
}