using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon;

namespace Domain.Enums
{
    public enum FileUploadType
    {
        Pdf,
        PdfForm,
        Image,
        Excel,
        PowerPoint,
        WordDocument,
        Vimeo,
        YoutubeVideo,
        Other,
        Video,
        CSS,
        Trophy,
        Certificate,
        BEECertificate,
        [EnumFriendlyName("Training Activity Document")]
        TrainingActivityDocument,
        [EnumFriendlyName("Training Activity Invoice")]
        TrainingActivityInvoice
    }
}