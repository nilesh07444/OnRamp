using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ramp.Services.Helpers
{
    public class EncryptionHelper
    {
        private static readonly byte[] Key =
        {
            188, 217, 19, 11, 24, 172, 185, 45, 114, 184, 76, 162, 37, 112, 59, 199,
            241, 224, 76, 144, 173, 53, 196, 29, 24, 126, 17, 218, 54, 136, 153, 209
        };

        private static readonly byte[] Vector =
        {
            146, 164, 191, 111, 23, 66, 113, 119, 231, 121, 123, 112, 79, 98, 100,
            90
        };

        private readonly ICryptoTransform _decryptor;
        private readonly ICryptoTransform _encryptor;
        private readonly UTF8Encoding _utfEncoding;

        public EncryptionHelper()
        {
            var rijndaelManaged = new RijndaelManaged();
            _encryptor = rijndaelManaged.CreateEncryptor(Key, Vector);
            _decryptor = rijndaelManaged.CreateDecryptor(Key, Vector);
            _utfEncoding = new UTF8Encoding();
        }

        public string Encrypt(string value)
        {
            byte[] bytes = _utfEncoding.GetBytes(value);
            var memoryStream = new MemoryStream();

            var cryptoStream = new CryptoStream(memoryStream, _encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();

            memoryStream.Position = 0;
            var encrypted = new byte[memoryStream.Length];
            memoryStream.Read(encrypted, 0, encrypted.Length);

            cryptoStream.Close();
            memoryStream.Close();
            return ByteArrToString(encrypted);
        }

        public string Decrypt(string value)
        {
            byte[] encryptedValues = StrToByteArray(value);
            var encryptedStream = new MemoryStream();
            var decryptStream = new CryptoStream(encryptedStream, _decryptor, CryptoStreamMode.Write);
            decryptStream.Write(encryptedValues, 0, encryptedValues.Length);
            decryptStream.FlushFinalBlock();

            encryptedStream.Position = 0;
            var decryptedBytes = new byte[encryptedStream.Length];
            encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
            encryptedStream.Close();

            return _utfEncoding.GetString(decryptedBytes);
        }

        /// Convert a string to a byte array.  NOTE: Normally we'd create a Byte Array from a string using an ASCII encoding (like so).
        //      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        //      return encoding.GetBytes(str);
        // However, this results in character values that cannot be passed in a URL.  So, instead, I just
        // lay out all of the byte values in a long string of numbers (three per - must pad numbers less than 100).
        private byte[] StrToByteArray(string str)
        {
            if (str.Length == 0)
                throw new Exception("Invalid string value in StrToByteArray");

            byte val;
            var byteArr = new byte[str.Length/3];
            int i = 0;
            int j = 0;
            do
            {
                val = byte.Parse(str.Substring(i, 3));
                byteArr[j++] = val;
                i += 3;
            } while (i < str.Length);
            return byteArr;
        }

        // Same comment as above.  Normally the conversion would use an ASCII encoding in the other direction:
        //      System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        //      return enc.GetString(byteArr);    
        private static string ByteArrToString(byte[] byteArr)
        {
            byte val;
            string tempStr = "";
            for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
            {
                val = byteArr[i];
                if (val < 10)
                    tempStr += "00" + val.ToString(CultureInfo.InvariantCulture);
                else if (val < 100)
                    tempStr += "0" + val.ToString(CultureInfo.InvariantCulture);
                else
                    tempStr += val.ToString(CultureInfo.InvariantCulture);
            }
            return tempStr;
        }
    }
}