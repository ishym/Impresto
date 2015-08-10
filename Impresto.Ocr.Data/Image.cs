
namespace Impresto.Ocr.Data
{
    public class Image
    {
        public Image()
        {
            this.Side = SideEnum.None;
        }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public SideEnum Side { get; set; }

        public byte[] ImageData { get; set; }
    }
}
