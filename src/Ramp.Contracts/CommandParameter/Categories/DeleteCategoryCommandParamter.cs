using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Categories
{
    public class DeleteCategoryCommandParamter : ICommand
    {
        public Guid Id { get; set; }
    }
}