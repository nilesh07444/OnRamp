using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.FileUploads
{
    public class FileUploadsQueryParameter : IQuery
    {
        public string Type { get; set; }
        public Guid CreatedUnderCompanyId { get; set; }
    }
}
