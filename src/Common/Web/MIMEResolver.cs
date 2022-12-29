using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Web
{
    public static class MIMEResolver
    {
        #region MimeTypes

        private static readonly Dictionary<string, string> _MIMEMap = new Dictionary<string, string>()
            {
                {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
                {".pps", "application/vnd.ms-powerpoint"},
                {".pdf", "application/pdf"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".ppt", "application/vnd.ms-powerpoint"},
                {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {".doc", "application/msword"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".mp4", "video/mp4"},
                {".jpeg", "image/jpeg"},
                {".png", "image/png"},
                {".gif", "image/gif"},
                {".bmp", "image/bmp"},
                {".jpg", "image/jpeg"}
            };

        #endregion MimeTypes

        public static string GetMIMEType(this string name)
        {
            var extension = Path.GetExtension(name.ToLowerInvariant());
            if (_MIMEMap.ContainsKey(extension.ToLowerInvariant()))
                return _MIMEMap[extension.ToLowerInvariant()];
            return null;
        }
    }
}