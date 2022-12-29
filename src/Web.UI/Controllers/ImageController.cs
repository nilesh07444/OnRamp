using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.UI.Controllers
{
    public class ImageController : Controller
    {

        [HttpGet]
        public ActionResult Resolve(string relativePath, int? width, int? height)
        {
            var type = MimeMapping.GetMimeMapping(relativePath);
            return File(GetThumbnailData(GetStandardImageData(Url.Content(relativePath)), height, width), type);
        }

        private byte[] GetThumbnailData(byte[] data, int? height, int? width, bool crop = false, VirtuaCon.Percentage? cropFactor = null)
        {
            try
            {
                return VirtuaCon.Drawing.ImageUtil.Resize(data, width, height, crop, cropFactor);
            }
            catch (OutOfMemoryException)
            {
                return data;
            }
        }

        private byte[] GetStandardImageData(string path)
        {
            var p = path;

            if (p.StartsWith("~") || p.StartsWith("/"))
                p = Server.MapPath(p);

            return Utility.Convertor.BytesFromFilePath(p);
        }
    }
}