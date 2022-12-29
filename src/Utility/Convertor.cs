using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class Convertor
    {
        public static byte[] BytesFromFilePath(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return null;
        }

        public static string Base64StringFromFilePath(string filePath)
        {
            var bytes = BytesFromFilePath(filePath);
            if (bytes != null)
            {
                return Convert.ToBase64String(bytes);
            }
            return null;
        }

        public static bool CreateFileFromBytes(byte[] content, string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, content);
                return true;
            }
            return false;
        }

        public static string Base64StringFromBytes(byte[] bytes)
        {
            if (bytes != null)
            {
                return Convert.ToBase64String(bytes);
            }
            return null;
        }
    }
}