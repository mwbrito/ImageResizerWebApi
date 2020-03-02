using System.IO;
using System.Text;
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
                    return ".png";
                default: 
                    return ".jpg";
            }
        }
        private string GetExtension()
        {
            switch(TargetFormat)
            {
                case MagickFormat.Png24: 
                    return "image/png";
                default: 
                    return "image/jpeg";
            }
        }

        public string GetCacheKey()
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                return string.Empty;
            }

            var fileExtension = Path.GetExtension(Name);
            var fileName = string.IsNullOrWhiteSpace(fileExtension) == false
                ? Name.Substring(0, Name.Length - fileExtension.Length)
                : Name;

            var widthKey = Width == MaxSize ? 0 : Width;
            var heightKey = Height == MaxSize ? 0 : Height;

            var builder = new StringBuilder();
            builder.Append($"{fileName}.");
            builder.Append($"w_{Width}");
            builder.Append($",h_{Height}");
            builder.Append($",q_{Quality}");
            builder.Append(GetExtension());

            return builder.ToString();   
        }
        
    }
}