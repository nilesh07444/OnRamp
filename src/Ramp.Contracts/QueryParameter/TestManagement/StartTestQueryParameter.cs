using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class StartTestQueryParameter : IQuery
    {
        public Guid TrainingTestId { get; set; }
        public string UploadThumbnailUrlBase { get; set; }
        public string UploadUrlBase { get; set; }
    }
}