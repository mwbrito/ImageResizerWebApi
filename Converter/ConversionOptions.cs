using ImageMagick;

namespace ImageResizer.Converter
{
    public class ConversionOptions
    {
        public const int MaxSize = 3000;

        public string Name { get; set; }
        public MagickFormat TargetFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Quality { get; set; }

        public string TargetMimeType => GetTargetMimeType();

        private string GetTargetMimeType()
        {
            switch(TargetFormat)
            {
                case MagickFormat.Png24: 
                    return "image/png";
                default: 
                    return "image/jpeg";
            }
        }
        
    }
}