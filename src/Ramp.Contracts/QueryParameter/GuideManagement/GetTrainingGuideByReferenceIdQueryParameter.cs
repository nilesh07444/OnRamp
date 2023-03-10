using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class GetTrainingGuideByReferenceIdQueryParameter : IQuery
    {
        public string RefrenceId { get; set; }
        public string UploadDeleteUrlBase { get; set; }
        public string UploadUrlBase { get; set; }
        public string UploadThumbnailUrlBase { get; set; }
        public string CoverPictureThumbnailUrlBase { get; set; }
        public Guid CompanyId { get; set; }
        public string UploadPreviewUrlBase { get; set; }
        public Guid? CurrentlyLoggedInUserId { get; set; }
    }
}