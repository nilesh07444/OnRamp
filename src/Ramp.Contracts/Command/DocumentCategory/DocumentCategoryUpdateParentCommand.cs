using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.DocumentCategory
{
    public class DocumentCategoryUpdateParentCommand
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
    }
}
