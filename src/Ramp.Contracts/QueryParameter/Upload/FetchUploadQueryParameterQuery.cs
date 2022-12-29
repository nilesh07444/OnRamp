using Common.Data;
using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Upload
{
    public class FetchUploadQueryParameter : IdentityModel<string>
    {
        public bool ExcludeBytes { get; set; }
        public bool? MainContext { get; set; }
    }
}