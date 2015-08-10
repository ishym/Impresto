using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Impresto.Ocr.Data;
using Impresto.Ocr.Core;

namespace Impresto.Ocr.Server.Utis
{
    public class ImageSetMediaTypeFormatter : MediaTypeFormatter
    {
        public ImageSetMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
        }
        
        public override bool CanReadType(Type type)
        {
            return type == typeof (ImageSet);
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        Func<HttpContent, SideEnum> GetSide = (HttpContent content) =>
        {
            SideEnum se = SideEnum.None;

            var side = content.Headers.SingleOrDefault(h => h.Key.ToLower() == "side");
            if (side.Value is string[])
            {
                Enum.TryParse((side.Value as string[]).SingleOrDefault(), out se); 
            }

            return se;
        };


        public async override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var imageSet = new ImageSet();

            var provider = await content.ReadAsMultipartAsync();

            //extract model
            var modelContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.NormalizeName() == "imageset");
            if (modelContent != null)
            {
                imageSet = await modelContent.ReadAsAsync<ImageSet>();
            }

            if (!imageSet.Images.Any())
            {
                //try extract from image content
                var fileContents = provider.Contents.Where(c => c.Headers.ContentDisposition.Name.NormalizeName().Matches(@"image\d+")).ToList();
                foreach (var fileContent in fileContents)
                {
                    imageSet.Images.Add(new Image
                    {
                        FileName = fileContent.Headers.ContentDisposition.FileName.NormalizeName(),
                        MimeType = fileContent.Headers.ContentType.MediaType,
                        Side = GetSide(fileContent),
                        ImageData = await fileContent.ReadAsByteArrayAsync()
                    });
                }
            }

            return imageSet;
        }
    }
}