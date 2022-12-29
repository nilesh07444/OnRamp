using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Command.DocumentCategory
{
    public class DocumentUpdateCategoryCommand
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}
