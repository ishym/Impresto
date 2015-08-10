using Impresto.Ocr.Data;
using Impresto.Ocr.Core;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;

namespace Impresto.Ocr.Test
{
    public class WebApiTester
    {
        public static void Test1()
        {
            string fileName = "op_front_1.jpg";

            System.Drawing.Image imgFile = System.Drawing.Image.FromFile(fileName);
            var imageSet = new ImageSet()
            {
                Name = "op_1",
            };

            var image = new Image()
            {
                FileName = fileName,
                MimeType = "image/jpeg",
                Side = SideEnum.Front,
                ImageData = imgFile.ToArray(System.Drawing.Imaging.ImageFormat.Jpeg),
            };

            imageSet.Images.Add(image);

            var multipartContent = new MultipartFormDataContent();

            var imageSetJson = JsonConvert.SerializeObject(imageSet, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
           
            //multipartContent.Add(new StringContent(imageSetJson, Encoding.UTF8, "application/json"), "imageset");

            int counter = 0;
            foreach (var img in imageSet.Images)
            {
                var imageContent = new ByteArrayContent(img.ImageData);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(img.MimeType);
                imageContent.Headers.Add("Side", img.Side.ToString());
                multipartContent.Add(imageContent, "image" + counter++, img.FileName);
            }
            

            string url = "http://93.153.125.236/Impresto.Ocr/api/recognize";
            //string url = "http://localhost:65000/api/recognize";

            var response = new HttpClient()
                .PostAsync(url, multipartContent)
                .Result;

            var responseContent = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine(responseContent);
        }
    }
}
