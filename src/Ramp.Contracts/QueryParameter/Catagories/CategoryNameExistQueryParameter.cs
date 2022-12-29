using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.Catagories
{
    public class CategoryNameExistsQueryParameter : IQuery
    {
        public string CategoryName { get; set; }
        public Guid CategoryId { get; set; }
    }

    public enum CategoryAlterationType
    {
        Create,
        Edit
    }
}