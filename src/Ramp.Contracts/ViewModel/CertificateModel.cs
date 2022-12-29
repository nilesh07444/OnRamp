using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;

namespace Ramp.Contracts.ViewModel
{
    public class CertificateListModel : IdentityModel<string>
    {
        public string Description { get; set; }
        public string UploadId { get; set; }
        public string ThumbnailUrl { get; set; }
		public string Title { get; set; }
		public DateTime CreatedOn { get; set; }
		public string Type { get; set; }

	}

	public class CertificateModel : CertificateListModel
    {
        public UploadResultViewModel Upload { get; set; }
    }
}
