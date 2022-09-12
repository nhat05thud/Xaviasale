using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Umbraco.Core.Composing;

namespace Xaviasale.ClassHelper
{
    public static class Utils
    {
        public static readonly string SortByName = "name";
        public static readonly string SortByCreatedDate = "created";
        public static readonly string SortByPrice = "price";
        public static readonly string OrderBy = "asc";
        public static readonly string OrderByDescending = "desc";
        public static string ConvertImageToBase64(string imagePath)
        {
            using (Image image = Image.FromFile(HttpContext.Current.Server.MapPath("~" + imagePath)))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }
        public static string CreateRandomPassword(int passwordLength)
        {
            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
            var chars = new char[passwordLength];
            var rd = new Random();
            for (var i = 0; i < passwordLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            return new string(chars);
        }
        public static IEnumerable<List<T>> Partition<T>(this IList<T> source, Int32 size)
        {
            for (int i = 0; i < Math.Ceiling(source.Count / (Double)size); i++)
                yield return new List<T>(source.Skip(size * i).Take(size));
        }

        public static string ConvertThousandPrice(decimal price)
        {
            return string.Format("{0:n0}", price);
        }
        public static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
        public static string ToMd5(this string str)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(str);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }

                return sb.ToString();
            }
        }
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
    }
}