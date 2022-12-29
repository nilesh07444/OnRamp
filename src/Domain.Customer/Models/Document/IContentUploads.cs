using System.Collections.Generic;

namespace Domain.Customer.Models.Document
{
    public interface IContentUploads
    {
        ICollection<Upload> ContentToolsUploads { get; set; }
        ICollection<Upload> Uploads { get; set; }
    }
}