using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class EditTrainingTestQueryParameter : IQuery
    {
        public Guid TrainingTestId { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
        public string UploadUrlBase { get; set; }
        public string UploadThumbnailUrlBase { get; set; }
        public string UploadDeleteUrlBase { get; set; }
    }
}