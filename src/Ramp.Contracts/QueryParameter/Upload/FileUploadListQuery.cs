using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter
{
    public class FileUploadListQuery
    {
        public IEnumerable<string> Ids { get; set; } = new List<string>();
    }
    public class UploadListQuery
    {
        public IEnumerable<string> Ids { get; set; } = new List<string>();
    }
}
