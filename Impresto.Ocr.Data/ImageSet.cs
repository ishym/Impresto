using System.Collections.Generic;

namespace Impresto.Ocr.Data
{
    public class ImageSet
    {
        public ImageSet()
        {
            this.Images = new List<Image>();
        }

        public string Name { get; set; }
        public List<Image> Images { get; set; }
    }  
}
