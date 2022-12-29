using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Upload
{
    public class GetUploadContentQueryParameter : IQuery
    {
        public Guid Id { get; set; }
    }
}