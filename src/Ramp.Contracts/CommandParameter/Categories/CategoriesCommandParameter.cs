using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.Categories
{
    public class CategoriesCommandParameter : ICommand
    {
        public Guid Id { get; set; }
        public string CategoryTitle { get; set; }
        public string Description { get; set; }
        public Guid CreatedUnderCompanyId { get; set; }
        public Guid ParentCategoryId { get; set; }
    }
}