using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ramp.Contracts.QueryParameter.Upload
{
    public class GetFileUploadFromPostedFileQuery
    {
        public HttpPostedFileBase File { get; set; }
        public Guid Id { get; set; }
    }
}
