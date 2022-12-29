using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;

/// <summary>
/// Summary description for Global_Encryptor_Decryptor
/// Author: Wasif Subzwari
/// This Global Class with methods designed to encrypt/decrypt object and return the same object to calling method
/// </summary>
public static class Cryptography
{
    private static string key { get; set; }
    private static string ivk { get; set; }

    static Cryptography()
    {
        key = "7061737323313233";
        ivk = "7a0b3c2dz1h588wb";
    }
    public static Object EncryptObject(Object obj)
    {
        if (obj != null)
        {
            string Type = obj.GetType().ToString();

            var propertyInfos = obj.GetType().GetProperties();

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (propertyInfos[i].PropertyType == typeof(string) || propertyInfos[i].PropertyType == typeof(String))
                {
                    bool isPublic = ((obj.GetType().GetMembers()[i]).ReflectedType).IsPublic;
                    if (isPublic)
                    {
                        obj.GetType().GetProperty(propertyInfos[i].Name).SetValue(obj, Cryptography.EncryptData(obj.GetType().GetProperty(propertyInfos[i].Name).GetValue(obj, null)));
                    }
                }
            }
        }
        return obj;
    }

    public static Object DecryptObject(Object obj)
    {
        if (obj != null)
        {
            string Type = obj.GetType().ToString();

            var propertyInfos = obj.GetType().GetProperties();

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (propertyInfos[i].PropertyType == typeof(string) || propertyInfos[i].PropertyType == typeof(String))
                {
                    bool isPublic = ((obj.GetType().GetMembers()[i]).ReflectedType).IsPublic;
                    if (isPublic)
                    {
                        obj.GetType().GetProperty(propertyInfos[i].Name).SetValue(obj, Cryptography.DecryptData(obj.GetType().GetProperty(propertyInfos[i].Name).GetValue(obj, null)));
                    }
                }
            }
        }
        return obj;
    }

    /// <summary>
    /// This method is to encrypt any single value passed
    /// </summary>
    /// <param name="value">Any value of primitive data type</param>
    /// <returns>Encrypted value wrapped in an Object, this must be type casted to string</returns>
    public static object EncryptData(object value)
    {
        var keybytes = Encoding.UTF8.GetBytes(key);
        var iv = Encoding.UTF8.GetBytes(ivk);
        
        Object encValue = Convert.ToBase64String(EncryptStringToBytes(value.ToString(), keybytes, iv));

        return encValue;
    }

    /// <summary>
    /// This method is to decrypt any single encrypted value passed to it
    /// </summary>
    /// <param name="value">Any encrypted value using same key</param>
    /// <returns>Decrypted value wrapped in an Object, this must be type casted to expected type.</returns>
    public static object DecryptData(object encValue)
    {
        var keybytes = Encoding.UTF8.GetBytes(key);
        var iv = Encoding.UTF8.GetBytes(ivk);
        var encryptedValue = Convert.FromBase64String(encValue.ToString());

        string decValue = DecryptStringFromBytes(encryptedValue, keybytes, iv);
        return decValue;
    }

    //This Method uses RSA decryption algorithm
    private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
        {
            throw new ArgumentNullException("cipherText");
        }
        if (key == null || key.Length <= 0)
        {
            throw new ArgumentNullException("key");
        }
        if (iv == null || iv.Length <= 0)
        {
            throw new ArgumentNullException("key");
        }

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an RijndaelManaged object
        // with the specified key and IV (IV is Index Vector Bytes array).
        using (var rijAlg = new RijndaelManaged())
        {
            //Settings
            rijAlg.Mode = CipherMode.CBC;
            rijAlg.Padding = PaddingMode.PKCS7;
            rijAlg.FeedbackSize = 128;

            rijAlg.Key = key;
            rijAlg.IV = iv;

            // Create a decrytor to perform the stream transform.
            var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

            // Create the streams used for decryption.
            using (var msDecrypt = new MemoryStream(cipherText))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }
        return plaintext;
    }

    // This Method uses RSA encryption algorithm
    private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
        {
            throw new ArgumentNullException("plainText");
        }
        if (key == null || key.Length <= 0)
        {
            throw new ArgumentNullException("key");
        }
        if (iv == null || iv.Length <= 0)
        {
            throw new ArgumentNullException("key");
        }
        byte[] encrypted;
        // Create a RijndaelManaged object
        // with the specified key and IV.
        using (var rijAlg = new RijndaelManaged())
        {
            rijAlg.Mode = CipherMode.CBC;
            rijAlg.Padding = PaddingMode.PKCS7;
            rijAlg.FeedbackSize = 128;

            rijAlg.Key = key;
            rijAlg.IV = iv;

            // Create a decrytor to perform the stream transform.
            var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

            // Create the streams used for encryption.
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }
        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

}