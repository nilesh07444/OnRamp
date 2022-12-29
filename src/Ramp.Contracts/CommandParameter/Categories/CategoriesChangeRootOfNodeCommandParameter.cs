using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Categories
{
    public class CategoriesChangeRootOfNodeCommandParameter : ICommand
    {
        public Guid Id { get; set; }
        public string CategoryTitle { get; set; }
        public string Description { get; set; }
        public Guid CreatedUnderCompanyId { get; set; }
        public Guid ParentCategoryId { get; set; }
    }
}