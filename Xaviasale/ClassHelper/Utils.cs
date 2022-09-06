using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
    }
}