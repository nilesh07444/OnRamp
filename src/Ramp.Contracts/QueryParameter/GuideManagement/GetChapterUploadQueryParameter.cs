using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class GetChapterUploadQueryParameter : IQuery
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public bool IncludeContent { get; set; }
    }
}
