using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon.Reporting;

namespace Ramp.Contracts.ViewModel
{
    public class AttachmentModel
    {
        public string Description { get; set; }
        public string Reference { get; set; }
        public ImageBlock Thumbnail { get; set; }
        public byte[] Data { get; set; }
        public TrainingDocumentTypeEnum Type { get; set; }
        public string Name { get; set; }
        public int? Parent { get; set; }
    }
}
