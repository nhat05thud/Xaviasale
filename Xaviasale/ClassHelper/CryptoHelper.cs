using System;
using System.Security.Cryptography;
using System.Text;

namespace Xaviasale.ClassHelper
{
    public static class CryptoHelper
    {
        public static string EncryptString(string plainText)
        {
            var textBytes = Encoding.UTF8.GetBytes(plainText);
            //var encryptedBytes = Encrypt256(textBytes);
            return Convert.ToBase64String(textBytes);
        }

        public static string DecryptString(string encryptedText)
        {
            var textBytes = Convert.FromBase64String(encryptedText);
            //var decryptedBytes = Decrypt256(textBytes);
            return Encoding.UTF8.GetString(textBytes);
        }

        private const string AesIV256 = @"61QER7UJKL(FDR6!";
        private const string AesKey256 = @"FLKPO086JBML][POJUWENQ123EMNLOI;";

        public static byte[] Encrypt256(byte[] src)
        {
            // AesCryptoServiceProvider
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.IV = Encoding.UTF8.GetBytes(AesIV256);
                aes.Key = Encoding.UTF8.GetBytes(AesKey256);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // encryption
                using (ICryptoTransform encrypt = aes.CreateEncryptor())
                {
                    byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                    return dest;
                }
            }
        }

        public static byte[] Decrypt256(byte[] src)
        {
            // AesCryptoServiceProvider
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.IV = Encoding.UTF8.GetBytes(AesIV256);
                aes.Key = Encoding.UTF8.GetBytes(AesKey256);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // decryption
                using (ICryptoTransform decrypt = aes.CreateDecryptor())
                {
                    byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                    return dest;
                }
            }
        }
    }
}