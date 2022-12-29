using Domain.Customer.Models.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class Document_DocumentUsageModel<T>
    {
        public T Document { get; set; }
        public IList<DocumentUsage> Usages { get; set; }
    }
}
