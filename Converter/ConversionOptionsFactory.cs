using System;
using ImageMagick;
using ImageResizer.Models;

namespace ImageResizer.Converter
{
    public class ConversionOptionsFactory
    {
        public static ConversionOptions FromResizeRequest(ResizeRequestModel requestModel)
        {
            var option = new ConversionOptions
            {
                Name = requestModel.Name,
            };

            if(requestModel.Width.HasValue && requestModel.Width > 0)
            {
                option.Width = Math.Min(requestModel.Width.Value, ConversionOptions.MaxSize);
            }

            if(requestModel.Height.HasValue && requestModel.Width > 0)
            {
                option.Width = Math.Min(requestModel.Height.Value, ConversionOptions.MaxSize);
            }

            option.TargetFormat = GetMagickFormat(requestModel.Format);

            if(requestModel.Quality.HasValue && requestModel.Quality >= 1 && requestModel.Quality <= 100)
                option.Quality = requestModel.Quality.Value;
            else
                option.Quality = option.TargetFormat == MagickFormat.Png24 ? 100 : 82;
            

            return option;
        }

        public static MagickFormat GetMagickFormat (string format)
        {   
            if(! string.IsNullOrWhiteSpace(format))
            {
                switch(format.ToLower())
                {
                    case "png": 
                        return MagickFormat.Png24;
                    case "jpeg": 
                    case "jpg":
                        return MagickFormat.Jpeg;
                }
            }

            return MagickFormat.Jpeg;
        }
        
    }
}