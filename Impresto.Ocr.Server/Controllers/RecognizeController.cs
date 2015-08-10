using System.Web.Http;
using System.Collections.Generic;
using Impresto.Ocr.Server.Singletones;
using Impresto.Ocr.Data;
using Impresto.Ocr.Server.Base;

namespace Impresto.Ocr.Server.Controllers
{
    public class RecognizeController : BaseApiController
    {
        #region Ctor

        public RecognizeController()
        {
        }

        #endregion

        [Route("api/recognize")]
        public Dictionary<string, object> Recognize(ImageSet model)
        {
            var res = new Dictionary<string, object>();

            string condition = model.Name ?? "";
            int i = 0;

            foreach (var image in model.Images)
            {
                if (i > 1)
                {
                    break;
                }

                switch (image.Side)
                {
                    case SideEnum.None:
                        if (i == 0)
                        {
                            condition = string.Concat(condition, "_front");
                        }
                        else if (i == 1)
                        {
                            condition = string.Concat(condition, "_back");
                        }
                        break;
                    case SideEnum.Front:
                        condition = string.Concat(condition, "_front");
                        break;
                    case SideEnum.Back:
                        condition = string.Concat(condition, "_back");
                        break;
                }

                Logger.Instance.InfoFormat("Start recognize setName: {0}, side: {1}, imageName: {2}, mimeType: {3}, length: {4}, condition: {5}",
                    !string.IsNullOrEmpty(model.Name) ? model.Name : "n/a",
                    image.Side,
                    image.FileName,
                    image.MimeType,
                    image.ImageData.Length,
                    condition);

                var dic = RecognitionProcessor.Instance.GetRecognizeData(image.ImageData, condition);
                foreach (var d in dic)
                {
                    res.Add(d.Key, d.Value);
                }
            
                i++;
            }

            Logger.Instance.InfoFormat("Return recognize data with {0} key(s)", res.Keys.Count);

            return res;
        }
    }
}