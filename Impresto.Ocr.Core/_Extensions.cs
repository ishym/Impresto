using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace Impresto.Ocr.Core
{
    public static class _Extensions
    {
        public static byte[] ToArray(this Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        public static Image ToImage(this byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }
        public static string NormalizeName(this string text)
        {
            return text.Replace("\"", "");
        }

        public static bool Matches(this string text, string pattern)
        {
            return Regex.IsMatch(text, pattern);
        }
    }
}
